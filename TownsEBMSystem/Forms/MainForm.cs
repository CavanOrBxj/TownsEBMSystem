using System;
using System.Drawing;
using System.Windows.Forms;
using CCWin;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Gecko;
using System.Threading;
using System.IO.Ports;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using EBMTable;
using EBSignature;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ControlAstro.Utils;
using System.Diagnostics;

namespace TownsEBMSystem
{
    public partial class MainForm : Form
    {
        private readonly string xulrunnerPath = Application.StartupPath + "/xulrunner";
        private string testUrl = "http://192.168.4.233:8033/";
        private Gecko.GeckoWebBrowser Browser;

        public static IConfig cf = ConfigFile.Instanse;
        AutoSizeFormClass asc = new AutoSizeFormClass();
        public static IniFiles ini;
        public static IniFiles ini2;
        public static SerialPort ComDevice = new SerialPort();
        public Thread threadHeart;
        System.Timers.Timer t;
        System.Timers.Timer timer_organization;
        private IoServer iocp = new IoServer(10, 2048);
        private int TcpReceivePort = 0;

        public bool IsStartStream { get; set; }
        bool isInitStream = false;

        private DataDealHelper dataDealHelper;
        public DataHelper dataHelper;

        EBMStream EbmStream;

        EBIndexTable EB_Index_Table = new EBIndexTable();
        DailyBroadcastTable Daily_Broadcast_Table = new DailyBroadcastTable();
        EBConfigureTable EB_Configure_Table = new EBConfigureTable();
        EBContentTable EB_Content_Table = new EBContentTable();
        EBCertAuthTable EB_CertAuth_Table = new EBCertAuthTable();
        List<EBContentTable> list_EB_Content_Table = new List<EBContentTable>();


        private EBMIndexGlobal_ _EBMIndexGlobal;
        private EBMConfigureGlobal_ _EBMConfigureGlobal;
        MessageShowForm MessageShowDlg;
        UpgradeForm upgradeForm;
        public System.Timers.Timer timer;
        public EBMStream EbMStream
        {
            get { return EbmStream; }
            set { EbmStream = value; }
        }

        public static Calcle calcel;
        private Object Gtoken = null; //用于锁住

        private WebClient downWebClient = new WebClient();
        /***************获取鼠标键盘未操作时间***************************/
        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref    LASTINPUTINFO plii);

        public int SecondCount = 0;

        /***************获取鼠标键盘未操作时间***************************/
        public long getIdleTick()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref    vLastInputInfo)) return 0;
            return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }


        public MainForm()
        {
            InitializeComponent();
            Xpcom.Initialize(xulrunnerPath);
            this.ShowInTaskbar = false;
            CheckIniConfig();
            InitConfig();
            InitTCPServer();
            IsStartStream = false;
            EbmStream = new EBMStream();
            InitTable();
            InitEBStream();//执行完    isInitStream = true;未生效
            calcel = new Calcle();
            InitStreamTableNew();
            Gtoken = new object();
            _EBMIndexGlobal = new EBMIndexGlobal_();
            _EBMConfigureGlobal = new EBMConfigureGlobal_();
            dataDealHelper = new DataDealHelper();
            dataHelper = new DataHelper();
            DataHelper.MyEvent += new DataHelper.MyDelegate(GlobalDataDeal);
            Loginjudge();
            this.Load += MainForm_Load;
        }


        private void InitTimerServerTimer()
        {
            //设置定时间隔(毫秒为单位)
          //  int interval = 5000;
           // timer = new System.Timers.Timer(SingletonInfo.GetInstance().TimeServiceInterval*1000*60);
            //设置执行一次（false）还是一直执行(true)
         //   timer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
        //    timer.Enabled = true;
            //绑定Elapsed事件
         //   timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);
        }

        /// <summary>
        /// Timer类执行定时到点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                SingleTimeServerSend(DateTime.Now.AddMinutes(1));
            }
            catch (Exception ex)
            {
            }
        }

        private void SingleTimeServerSend(DateTime time)
        {
            List<TimeService_> listTS = new List<TimeService_>();
            TimeService_ select = new TimeService_();

            System.Guid guid = new Guid();
            guid = Guid.NewGuid();
            select.ItemID = guid.ToString();
            select.Configure = new EBConfigureTimeService();

            select.Configure.Real_time = time;

            select.GetSystemTime = true;
            select.SendTick = 60;
            listTS.Add(select);
            DealTimeService(listTS);

        }

        private void DealTimeService(List<TimeService_> listTS)
        {
            lock (Gtoken)
            {
                if (_EBMConfigureGlobal.ListTimeService != null)
                {
                    //去同项
                    foreach (TimeService_ item in listTS)
                    {
                        TimeService_ tmp = _EBMConfigureGlobal.ListTimeService.Find(s => s.ItemID.Equals(item.ItemID));
                        if (tmp != null)
                        {
                            _EBMConfigureGlobal.ListTimeService.Remove(tmp);
                        }
                    }
                }
                else
                {
                    _EBMConfigureGlobal.ListTimeService = new List<TimeService_>();
                }

                //增新项
                foreach (TimeService_ item in listTS)
                {
                    _EBMConfigureGlobal.ListTimeService.Add(item);
                }

                EbmStream.EB_Configure_Table = GetConfigureTable(ref EB_Configure_Table, false) ? EB_Configure_Table : null;


                EbMStream.Initialization();
            }
            UpdateDataTextNew((object)3);
            #region 删除记录 
            foreach (TimeService_ item in listTS)
            {
                _EBMConfigureGlobal.ListTimeService.Remove(item);
            }
            #endregion
        }

        public bool GetConfigureTable(ref EBConfigureTable oldTable, bool isTimeSend)
        {
            try
            {
                List<ConfigureCmd> configureCmd = isTimeSend ? GetSendTimeSerConfigureCmd() : GetSendConfigureCmd();
                if (configureCmd == null || configureCmd.Count == 0)
                {
                    if (oldTable != null) oldTable.list_configure_cmd = null;
                    return false;
                }
                if (oldTable == null)
                {
                    oldTable = new EBConfigureTable();
                    oldTable.Table_id = 0xfb;
                    oldTable.Table_id_extension = 0;
                }
                oldTable.list_configure_cmd = configureCmd;
                oldTable.Repeat_times = 1;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private List<ConfigureCmd> GetSendTimeSerConfigureCmd()
        {
            try
            {
                BindingCollection<Configure> TotalConfig_List = GetConfigureCollection(_EBMConfigureGlobal);
                List<ConfigureCmd> cmd = new List<ConfigureCmd>();
                foreach (var d in TotalConfig_List)
                {
                    //if (d.SendState && d.B_Daily_cmd_tag == Utils.ComboBoxHelper.ConfigureTimeServiceTag)
                    //{
                    //    cmd.Add((d as TimeService_).Configure.GetCmd());
                    //}
                }
                return cmd;
            }
            catch
            {
                return null;
            }
        }

        private BindingCollection<Configure> GetConfigureCollection(EBMConfigureGlobal_ _EBMConfigureGlobal)
        {
            BindingCollection<Configure> TotalConfig_List = new BindingCollection<Configure>();


            if (_EBMConfigureGlobal.ListTimeService != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListTimeService)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListSetAddress != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListSetAddress)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListWorkMode != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListWorkMode)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListMainFrequency != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListMainFrequency)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListReback != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListReback)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListDefaltVolume != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListDefaltVolume)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListRebackPeriod != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListRebackPeriod)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListContentMoniterRetback != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListContentMoniterRetback)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListContentRealMoniter != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListContentRealMoniter)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListContentRealMoniterGX != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListContentRealMoniterGX)
                {
                    TotalConfig_List.Add(item);
                }

            }

            if (_EBMConfigureGlobal.ListStatusRetback != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListStatusRetback)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListSoftwareUpGrade != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListSoftwareUpGrade)
                {
                    TotalConfig_List.Add(item);
                }
            }

            if (_EBMConfigureGlobal.ListRdsConfig != null)
            {
                foreach (var item in _EBMConfigureGlobal.ListRdsConfig)
                {
                    TotalConfig_List.Add(item);
                }
            }

            return TotalConfig_List;
        }

        private List<ConfigureCmd> GetSendConfigureCmd()
        {
            try
            {
                BindingCollection<Configure> TotalConfig_List;
                TotalConfig_List = GetConfigureCollection(_EBMConfigureGlobal);
                List<ConfigureCmd> cmd = new List<ConfigureCmd>();
                foreach (var d in TotalConfig_List)
                {
                    switch (d.B_Daily_cmd_tag)
                    {
                        case Utils.ComboBoxHelper.ConfigureTimeServiceTag:
                            cmd.Add((d as TimeService_).Configure.GetCmd());
                            break;
                        case Utils.ComboBoxHelper.ConfigureSetAddressTag:
                            if (SingletonInfo.GetInstance().IsGXProtocol)
                            {

                                cmd.Add((d as SetAddress_).Configure.GetCmdGX());
                            }
                            else
                            {
                                cmd.Add((d as SetAddress_).Configure.GetCmd());
                            }

                            break;
                        case Utils.ComboBoxHelper.ConfigureWorkModeTag:
                            if (SingletonInfo.GetInstance().IsGXProtocol)
                            {
                                cmd.Add((d as WorkMode_).Configure.GetCmdGX());
                            }
                            else
                            {
                                cmd.Add((d as WorkMode_).Configure.GetCmd());
                            }

                            break;
                        case Utils.ComboBoxHelper.ConfigureMainFrequencyTag:
                            if (SingletonInfo.GetInstance().IsGXProtocol)
                            {
                                cmd.Add((d as MainFrequency_).Configure.GetCmdGX());
                            }
                            else
                            {
                                cmd.Add((d as MainFrequency_).Configure.GetCmd());
                            }

                            break;
                        case Utils.ComboBoxHelper.ConfigureRebackTag:
                            cmd.Add((d as Reback_).Configure.GetCmd());
                            break;
                        case Utils.ComboBoxHelper.ConfigureDefaltVolumeTag:

                            if (SingletonInfo.GetInstance().IsGXProtocol)
                            {
                                cmd.Add((d as DefaltVolume_).Configure.GetCmdGX());
                            }
                            else
                            {
                                cmd.Add((d as DefaltVolume_).Configure.GetCmd());
                            }

                            break;
                        case Utils.ComboBoxHelper.ConfigureRebackPeriodTag:

                            if (SingletonInfo.GetInstance().IsGXProtocol)
                            {
                                cmd.Add((d as RebackPeriod_).Configure.GetCmdGX());
                            }
                            else
                            {
                                cmd.Add((d as RebackPeriod_).Configure.GetCmd());
                            }

                            break;
                        case Utils.ComboBoxHelper.ConfigureContentMoniterRetbackTag:
                            cmd.Add((d as ContentMoniterRetback_).Configure.GetCmd());
                            break;
                        case Utils.ComboBoxHelper.ConfigureContentRealMoniterTag:
                            if (SingletonInfo.GetInstance().IsGXProtocol)
                            {
                                cmd.Add((d as ContentRealMoniterGX_).Configure.GetCmd());

                            }
                            else
                            {
                                cmd.Add((d as ContentRealMoniter_).Configure.GetCmd());
                            }


                            break;
                        case Utils.ComboBoxHelper.ConfigureStatusRetbackTag:
                            cmd.Add((d as StatusRetback_).Configure.GetCmd());
                            break;
                        case Utils.ComboBoxHelper.ConfigureSoftwareUpGradeTag:
                            cmd.Add((d as SoftwareUpGrade_).Configure.GetCmd());
                            break;
                        case Utils.ComboBoxHelper.ConfigureRdsConfigTag:
                            cmd.Add((d as RdsConfig_).Configure.GetCmd());
                            break;
                    }

                }
                return cmd;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void Loginjudge()
        {
            FmLogin fmLogin = new FmLogin();
            SingletonInfo.GetInstance().lockstatus = true;
            fmLogin.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            long i = getIdleTick();
            long Lockcycle = Convert.ToInt32(SingletonInfo.GetInstance().lockcycle) * 1000 * 60;
            if (i > Lockcycle)//目前判断是30秒就好了。超过一分钟是>=60000。十分钟600000
            {
                if (!SingletonInfo.GetInstance().lockstatus)
                {
                    FmLogin fmLogin = new FmLogin();
                    SingletonInfo.GetInstance().lockstatus = true;
                    fmLogin.Show();
                }
            }

            if (SingletonInfo.GetInstance().UpgradeFlag == "0")
            {
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    if (SecondCount < 30)
                    {
                        SecondCount++;
                    }
                    else
                    {
                        SecondCount = 0;
                        //获取版本升级信息

                        UpgradInfo versionsponse;
                        object pptmp = SingletonInfo.GetInstance().post.PostCommnand(null, "版本信息");
                        if (pptmp == null)
                        {
                            return;
                        }
                        else
                        {
                            versionsponse = (UpgradInfo)pptmp;
                            string versiontmp = versionsponse.version;
                            if (versiontmp != Application.ProductVersion && versiontmp != "")
                            {
                                bool flag = false;
                                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                                DirectoryInfo folder = new DirectoryInfo(path);
                                foreach (FileInfo file in folder.GetFiles("TownsEBMSystem_V" + versiontmp + ".zip"))
                                {
                                    if (file != null)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }

                                if (!flag && !SingletonInfo.GetInstance().downloading)
                                {
                                    //开始下载文件
                                    DownloadFile("TownsEBMSystem_V" + versiontmp + ".zip");
                                    SingletonInfo.GetInstance().downloading = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="num">下载文件序号</param>
        private void DownloadFile(string Filename)
        {
            try
            {
                string strTag = Application.StartupPath + "\\" + Filename;
                this.downWebClient.DownloadFileAsync(new Uri(SingletonInfo.GetInstance().HttpServer+ "upload/" + Filename), strTag);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            SingletonInfo.GetInstance().lockstatus = false;
            pictureBox_Login_Click(null,null);
            ProcessBegin();
            timer1.Enabled = true;
            timer2.Enabled = true;
            this.downWebClient.DownloadFileCompleted += delegate (object wcsender, AsyncCompletedEventArgs ex)
            {
                SingletonInfo.GetInstance().downloading = false;
                ini.WriteValue("SystemConfig", "UpgradeFlag","1");
            };

            if (SingletonInfo.GetInstance().loginstatus && SingletonInfo.GetInstance().SendCommandMode)
            {
                skinButton2.Visible = true;
            }
        }

        private void InitTable()
        {
            EB_Index_Table.Table_id = 0xfd;
            EB_Index_Table.Table_id_extension = 0;
            EB_Content_Table.Table_id = 0xfe;
            EB_Content_Table.Table_id_extension = 0;
            Daily_Broadcast_Table.Table_id = 0xfa;
            Daily_Broadcast_Table.Table_id_extension = 0;
            EB_CertAuth_Table.Table_id = 0xfc;
            EB_CertAuth_Table.Table_id_extension = 0;
            EB_Configure_Table.Table_id = 0xfb;
            EB_Configure_Table.Table_id_extension = 0;
            EbmStream.EB_Index_Table = EB_Index_Table;

            #region 启用 广西还是国标协议

            EbmStream.EB_Index_Table.ProtocolGX = SingletonInfo.GetInstance().IsGXProtocol;
            #endregion 
        }

        private void GlobalDataDeal(object obj)
        {
            try
            {
                ParamObject ReceiveObject = (ParamObject)obj;
                switch (ReceiveObject.commandcode)
                {
                    case 0x18: //任务上报

                        TaskUploadBegin op = (TaskUploadBegin)ReceiveObject.paramobj;
                        if (SingletonInfo.GetInstance().loginstatus)
                        {
                            if (op.ebm_class == "6")
                            {
                                //关
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线优先的情况
                                    OfflineAllStop();
                                }
                                else
                                {
                                    //正常情况
                                    OnlineAllStop();
                                }
                               
                                this.Invoke(new Action(() =>
                                {
                                    btn_Emergency_Main.Enabled = true;
                                    btn_Organization.Enabled = true;
                                    btn_Daily_Main.Enabled = true;

                                    btn_Daily_Main.Text = "日常广播";
                                    btn_Emergency_Main.Text = "应急广播";
                                    btn_Daily_Main.BaseColor = System.Drawing.Color.DarkGreen;
                                    btn_Emergency_Main.BaseColor = System.Drawing.Color.Maroon;
                                }));
                            }
                            else
                            {
                                //开
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线优先
                                    OfflineAllStart("应急");
                                }
                                else
                                {
                                    //正常情况
                                    OnlineAllStart("1");
                                }
                              
                                this.Invoke(new Action(() =>
                                {
                                    btn_Emergency_Main.Enabled = true;
                                    btn_Organization.Enabled = false;
                                    btn_Daily_Main.Enabled = false;


                                    btn_Daily_Main.Text = "日常广播";
                                    btn_Emergency_Main.Text = "应急停播";
                                    btn_Emergency_Main.BaseColor = System.Drawing.Color.Red;

                                }));

                            }
                        }
                        else
                        {
                            //未登录的情况下
                            if (op.ebm_class == "6")
                            {
                                //关闭指令
                                OfflineAllStop();
                                this.Invoke(new Action(() =>
                                {
                                    btn_Emergency_Main.Enabled = true;
                                    btn_Organization.Enabled = true;
                                    btn_Daily_Main.Enabled = true;

                                    btn_Daily_Main.Text = "日常广播";
                                    btn_Emergency_Main.Text = "应急广播";
                                    btn_Daily_Main.BaseColor = System.Drawing.Color.DarkGreen;
                                    btn_Emergency_Main.BaseColor = System.Drawing.Color.Maroon;

                                }));
                            }
                            else
                            {
                                //开启指令
                                OfflineAllStart("应急");
                                this.Invoke(new Action(() =>
                                {
                                    btn_Emergency_Main.Enabled = true;
                                    btn_Organization.Enabled = false;
                                    btn_Daily_Main.Enabled = false;
                                    btn_Daily_Main.Text = "日常广播";
                                    btn_Emergency_Main.Text = "应急停播";
                                    btn_Emergency_Main.BaseColor = System.Drawing.Color.Red;

                                }));
                            }

                        }
                        break;
                    case 0x20:

                        RecvHeartBeat receiveheartbeat = (RecvHeartBeat)ReceiveObject.paramobj;
                        if (receiveheartbeat.auxiliarydata != null)
                        {
                            switch (receiveheartbeat.auxiliarydata)
                            {
                                case "1":
                                    //话筒
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.Lime;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));
                                    break;
                                case "2":
                                    //USB
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.Lime;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));

                                    break;
                                case "3":
                                    //DVB
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.Lime;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));
                                    break;
                                case "4":
                                    //DTMB
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.Lime;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));
                                    break;
                                case "5":
                                    //线路一
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.Lime;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));
                                    break;
                                case "6":
                                    //线路二
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.Lime;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));
                                    break;
                                case "7":
                                    //调频一
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.Lime;
                                        skinButton6.ForeColor = System.Drawing.Color.White;

                                    }));
                                    break;
                                case "8":
                                    //调频二
                                    this.Invoke(new Action(() =>
                                    {
                                        skinButton1.ForeColor = System.Drawing.Color.White;
                                        skinButton3.ForeColor = System.Drawing.Color.White;
                                        skinButton4.ForeColor = System.Drawing.Color.White;
                                        skinButton5.ForeColor = System.Drawing.Color.White;

                                        skinButton9.ForeColor = System.Drawing.Color.White;
                                        skinButton8.ForeColor = System.Drawing.Color.White;
                                        skinButton7.ForeColor = System.Drawing.Color.White;
                                        skinButton6.ForeColor = System.Drawing.Color.Lime;

                                    }));
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog(typeof(MainForm), ex.ToString());
            }
        }

        private void InitConfig()
        {
            try
            {
                SingletonInfo.GetInstance().username = ini.ReadValue("LoginInfo", "username");
                SingletonInfo.GetInstance().password = ini.ReadValue("LoginInfo", "password");
                SingletonInfo.GetInstance().licenseCode = ini.ReadValue("LoginInfo", "licenseCode");
                SingletonInfo.GetInstance().HttpServer = ini.ReadValue("HttpURL", "HttpServer");
                SingletonInfo.GetInstance().pid = ini.ReadValue("PlayInfo", "playPID");
                SingletonInfo.GetInstance().SendfaileTime = Convert.ToInt32(ini.ReadValue("HeartBeat", "sendfailetimes"));
                t = new System.Timers.Timer(Convert.ToInt32(ini.ReadValue("Timers", "HeartBeatIntreval")));//实例化Timer类
                timer_organization = new System.Timers.Timer(Convert.ToInt32(ini.ReadValue("Timers", "GetOrganizationInterval")));//实例化Timer类 
                TcpReceivePort = Convert.ToInt32(ini.ReadValue("TCP", "ReceivePort"));
                SingletonInfo.GetInstance().SendTCPdataIP= ini.ReadValue("TCP", "SenddataIP");
                SingletonInfo.GetInstance().SendTCPdataPORT = Convert.ToInt32(ini.ReadValue("TCP", "SenddataPORT"));
                SingletonInfo.GetInstance().ebm_id_front= ini.ReadValue("EBM", "ebm_id_front");
                SingletonInfo.GetInstance().ebm_id_behind = ini.ReadValue("EBM", "ebm_id_behind");
                SingletonInfo.GetInstance().ebm_id_count = Convert.ToInt32(ini.ReadValue("EBM", "ebm_id_count"));

                SingletonInfo.GetInstance().cramblertype = ini.ReadValue("Scrambler", "ScramblerType");
                SingletonInfo.GetInstance().IsGXProtocol = ini.ReadValue("ProtocolType", "ProtocolType") == "1" ? true : false;//“1”表示广西协议 2表示国标
                #region AddCertInfo
                SingletonInfo.GetInstance().IsUseAddCert = ini.ReadValue("AddCertInfo", "IsUseAddCert") == "1" ? true : false;//“1”表示使用增加的证书 2表示不使用增加证书信息
                SingletonInfo.GetInstance().Cert_SN = ini.ReadValue("AddCertInfo", "Cert_SN");
                SingletonInfo.GetInstance().PriKey = ini.ReadValue("AddCertInfo", "PriKey");
                SingletonInfo.GetInstance().PubKey = ini.ReadValue("AddCertInfo", "PubKey");

                EBCert tmp = new EBCert();
                tmp.Cert_sn = SingletonInfo.GetInstance().Cert_SN;
                tmp.PriKey = SingletonInfo.GetInstance().PriKey;
                tmp.PubKey = SingletonInfo.GetInstance().PubKey;
                SingletonInfo.GetInstance().Cert_Index = SingletonInfo.GetInstance().InlayCA.AddEBCert(tmp);
                #endregion
                SingletonInfo.GetInstance().input_channel_id= ini.ReadValue("EBM", "input_channel_id");

                SingletonInfo.GetInstance().S_details_channel_transport_stream_id= ini.ReadValue("EBM", "S_details_channel_transport_stream_id");
                SingletonInfo.GetInstance().S_details_channel_program_number= ini.ReadValue("EBM", "S_details_channel_program_number");
                SingletonInfo.GetInstance().S_details_channel_PCR_PID = ini.ReadValue("EBM", "S_details_channel_PCR_PID");

                SingletonInfo.GetInstance().ts_pid= ini.ReadValue("EBM", "pid");

                SingletonInfo.GetInstance().IndexItemID = Convert.ToInt32(ini.ReadValue("EBM", "IndexItemID"));


                var jo = TableDataHelper.ReadTable(Enums.TableType.WhiteList);
                SingletonInfo.GetInstance().WhiteListRecordList = JsonConvert.DeserializeObject<List<WhiteListRecord>>(jo["0"].ToString());
                SingletonInfo.GetInstance().EndtimeDelay = ini.ReadValue("EBM", "EndtimeDelay");
                SingletonInfo.GetInstance().LocalHost = ini.ReadValue("LocalHost", "IP");

                #region    UI部分
                SingletonInfo.GetInstance().logincode = cf["LoginCode"].ToString();
                skinButton1.Text = cf["LeftBtuuon1"].ToString();
                skinButton9.Text = cf["LeftBtuuon2"].ToString();
                skinButton8.Text = cf["LeftBtuuon3"].ToString();
                skinButton3.Text = cf["RightBtuuon1"].ToString();
                skinButton7.Text = cf["RightBtuuon2"].ToString();
                skinButton6.Text = cf["RightBtuuon3"].ToString();
                SingletonInfo.GetInstance().lockcycle = cf["Lockcycle"].ToString();

                SingletonInfo.GetInstance().mark = cf["mark"].ToString();
                #endregion

                #region 授时指令周期
              //  SingletonInfo.GetInstance().TimeServiceInterval = Convert.ToInt32(ini.ReadValue("Instructions", "TimeServiceInterval"));
                #endregion

                SingletonInfo.GetInstance().IsLogoutWin = ini.ReadValue("SystemConfig", "LogoutWin") == "1" ? true : false;
                SingletonInfo.GetInstance().UpgradeFlag = ini.ReadValue("SystemConfig", "UpgradeFlag");
                SingletonInfo.GetInstance().SendCommandMode = ini2.ReadValue("SendCommandMode", "SendCommandMode") =="1"?true:false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "获取配置文件失败");
            }
        }

        /// <summary>
        /// 启动发送
        /// </summary>
        private void ProcessBegin()
        {
            try
            {
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    if (!SingletonInfo.GetInstance().SendCommandMode)
                    {
                        //离线模式优先
                        if (EbmStream != null && isInitStream)
                        {
                            //发送数据
                           // EbmStream.StartStreaming();//测试放开
                          //  IsStartStream = true;
                         //   SingletonInfo.GetInstance().IsStartSend = true;
                        }
                    }
                }
                else
                {
                    if (EbmStream != null && isInitStream)
                    {
                        //发送数据
                      //  EbmStream.StartStreaming();
                      //  IsStartStream = true;
                      //  SingletonInfo.GetInstance().IsStartSend = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "启动发送失败：" + ex.ToString());
            }
        }

        public bool InitEBStream()
        {
            try
            {
                EbmStream.ElementaryPid = Convert.ToInt32(ini.ReadValue("TSSendInfo", "ElementaryPid").ToString());
                EbmStream.Stream_id = Convert.ToInt32(ini.ReadValue("TSSendInfo", "Stream_id").ToString());
                EbmStream.Program_id = Convert.ToInt32(ini.ReadValue("TSSendInfo", "Program_id").ToString());
                EbmStream.PMT_Pid = Convert.ToInt32(ini.ReadValue("TSSendInfo", "PMT_Pid").ToString());
                EbmStream.Section_length = Convert.ToInt32(ini.ReadValue("TSSendInfo", "Section_length").ToString());
                EbmStream.sDestSockAddress = ini.ReadValue("TSSendInfo", "sDestSockAddress").ToString();
                EbmStream.sLocalSockAddress = ini.ReadValue("TSSendInfo", "sLocalSockAddress").ToString();
                EbmStream.Stream_BitRate = Convert.ToInt32(ini.ReadValue("TSSendInfo", "Stream_BitRate").ToString());
                InitStreamTableNew();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "TS流参数设置失败！");
                return false;
            }
        }


        public void InitStreamTableNew()
        {
            //设置需要发送的表
            GetEBIndexTable(ref EB_Index_Table);
            EbmStream.EB_Index_Table = EB_Index_Table;

            if (SingletonInfo.GetInstance().IsUseCAInfo)
            {
                EbmStream.SignatureCallbackRef = new EBMStream.SignatureCallBackDelegateRef(calcel.SignatureFunc);//每次在 Initialization()之前调用
            }
            else
            {
                EbmStream.SignatureCallbackRef = null;
            }
            EbmStream.Initialization();
            isInitStream = true;
        }


        private bool CheckIniConfig()
        {
            try
            {
                string iniPath = Path.Combine(Application.StartupPath, "TownsEBMSystem.ini");
                ini = new IniFiles(iniPath);

                string iniPath2 = Path.Combine(Application.StartupPath, "SendCommandMode.ini");
                ini2 = new IniFiles(iniPath2);


                string path = AppDomain.CurrentDomain.BaseDirectory ;
                ConfigFile.Instanse.fileName = @path + "config\\LocalConfiguration.cfg";
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "配置文件打开失败");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 启动TCP服务
        /// </summary>
        private void InitTCPServer()
        {
            iocp.Start(TcpReceivePort);
            iocp.mainForm = this;
        }
        
        private void ShowtreeViewOrganization(List<organizationdata> inputdata)
        {
            this.Invoke(new Action(() =>
            {
                treeViewOrganization.Nodes.Clear();
                foreach (organizationdata item in inputdata)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization.Nodes.Add(node);
                }
            }));
        }

        private void ShowtreeViewOrganization_WhiteList(List<organizationdata> inputdata)
        {
            this.Invoke(new Action(() =>
            {
                treeViewOrganization_WhiteList.Nodes.Clear();
                foreach (organizationdata item in inputdata)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization_WhiteList.Nodes.Add(node);
                }
            }));
        }

        private void ShowtreeViewOrganization_RebackCycle(List<organizationdata> inputdata)
        {
            this.Invoke(new Action(() =>
            {
                treeViewOrganization_RebackCycle.Nodes.Clear();
                foreach (organizationdata item in inputdata)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization_RebackCycle.Nodes.Add(node);
                }
            }));
        }
        private void ShowtreeViewOrganization_RebackParam(List<organizationdata> inputdata)
        {
            this.Invoke(new Action(() =>
            {
                treeViewOrganization_RebackParam.Nodes.Clear();
                foreach (organizationdata item in inputdata)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization_RebackParam.Nodes.Add(node);
                }
            }));
        }

        private void ShowtreeViewOrganization_volumn(List<organizationdata> inputdata)
        {
            this.Invoke(new Action(() =>
            {
                treeViewOrganization_volumn.Nodes.Clear();
                foreach (organizationdata item in inputdata)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization_volumn.Nodes.Add(node);
                }
            }));
        }

        private void ShowtreeViewOrganization_SwitchAmplifier(List<organizationdata> inputdata)
        {
            this.Invoke(new Action(() =>
            {
                treeViewOrganization_SwitchAmplifier.Nodes.Clear();
                foreach (organizationdata item in inputdata)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization_SwitchAmplifier.Nodes.Add(node);
                }
            }));
        }

        private void InitTimer()
        {
             t.Elapsed += new System.Timers.ElapsedEventHandler(SendHeartBeat);//到达时间的时候执行事件；

          
            t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；


            timer_organization.Elapsed += new System.Timers.ElapsedEventHandler(GetOrganizationtData);//到达时间的时候执行事件；


            timer_organization.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            timer_organization.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；


        }

        private void GetOrganizationtData(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
               SingletonInfo.GetInstance().Organization =((organizationInfo)SingletonInfo.GetInstance().post.PostCommnand(null, "获取区域")).data;
                ShowtreeViewOrganization(SingletonInfo.GetInstance().Organization);
                ShowtreeViewOrganization_WhiteList(SingletonInfo.GetInstance().Organization);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "区域数据显示失败");
            }
        }

        private void SendHeartBeat(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                HeartBeatResponse heartbeatresponse = (HeartBeatResponse)SingletonInfo.GetInstance().post.PostCommnand(null, "心跳");
             //   JavaScriptSerializer Serializer = new JavaScriptSerializer();
                LocalParam param = heartbeatresponse.extend;
               
                if (param.mark != SingletonInfo.GetInstance().mark)
                {
                    //有变化
                    SingletonInfo.GetInstance().mark = param.mark;
                    cf["mark"] = param.mark;

                    if (SingletonInfo.GetInstance().LeftBtn1txt != param.btn_one)
                    {
                        SingletonInfo.GetInstance().LeftBtn1txt = param.btn_one;
                        cf["LeftBtuuon1"] = param.btn_one;
                        this.Invoke(new Action(() =>
                        {
                            skinButton1.Text = param.btn_one;
                        }));
                    }


                    if (SingletonInfo.GetInstance().LeftBtn2txt != param.btn_two)
                    {
                        SingletonInfo.GetInstance().LeftBtn2txt = param.btn_two;
                        cf["LeftBtuuon2"] = param.btn_two;
                        this.Invoke(new Action(() =>
                        {
                            skinButton9.Text = param.btn_two;
                        }));
                    }

                    if (SingletonInfo.GetInstance().LeftBtn3txt != param.btn_three)
                    {
                        SingletonInfo.GetInstance().LeftBtn3txt = param.btn_three;
                        cf["LeftBtuuon3"] = param.btn_three;
                        this.Invoke(new Action(() =>
                        {
                            skinButton8.Text = param.btn_three;
                        }));
                    }


                    if (SingletonInfo.GetInstance().RightBtn1txt != param.btn_four)
                    {
                        SingletonInfo.GetInstance().RightBtn1txt = param.btn_four;
                        cf["RightBtuuon1"] = param.btn_four;

                        this.Invoke(new Action(() =>
                        {
                            skinButton3.Text = param.btn_four;
                        }));
                    }

                    if (SingletonInfo.GetInstance().RightBtn2txt != param.btn_five)
                    {
                        SingletonInfo.GetInstance().RightBtn2txt = param.btn_five;
                        cf["RightBtuuon2"] = param.btn_five;
                        this.Invoke(new Action(() =>
                        {
                            skinButton7.Text = param.btn_five;
                        }));
                    }

                    if (SingletonInfo.GetInstance().RightBtn3txt != param.btn_six)
                    {
                        SingletonInfo.GetInstance().RightBtn3txt = param.btn_six;
                        cf["RightBtuuon3"] = param.btn_six;
                        this.Invoke(new Action(() =>
                        {
                            skinButton6.Text = param.btn_six;
                        }));
                    }
                    
                    if (SingletonInfo.GetInstance().logincode != param.lock_pwd)
                    {
                        SingletonInfo.GetInstance().logincode = param.lock_pwd;
                        cf["LoginCode"] = param.lock_pwd;
                    }


                    if (SingletonInfo.GetInstance().lockcycle != param.lock_cycle)
                    {
                        SingletonInfo.GetInstance().lockcycle = param.lock_cycle;
                        cf["Lockcycle"] = param.lock_cycle;
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "心跳发送失败");
            }
            finally
            {
                //暂时先不处理吧
            }
        }
        
        protected void subnode(TreeNode pnode, List<organizationdata> org)
        {
            TreeNode node;

            foreach (organizationdata item in org)
            {
                node = new TreeNode();
                node.Text = item.name;
                node.Tag = item;
                subnode(node, item.children);
                pnode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// 显示列表  
        /// </summary>
        /// <param name="dgvMainData"></param>
        /// <param name="showtype">显示类型 0表示没有广播的显示情况 1表示有广播在播放的显示情况，此时不能勾选操作，只能取消广播</param>
        private void ShowskinDataGridView_Main(List<Datagridviewmainitem> dgvMainData,int showtype=0)
        {
            this.Invoke(new Action(() =>
            {
                skinDataGridView_Main.Rows.Clear();
               
                foreach (Datagridviewmainitem dataRow in dgvMainData)
                {
                    DataGridViewRow dgvR = new DataGridViewRow();
                    dgvR.CreateCells(skinDataGridView_Main);

                    if (dataRow.checkstate)
                    {
                        dgvR.Cells[0].Value = imageList1.Images[0];
                    }
                    else
                    {
                        dgvR.Cells[0].Value = imageList1.Images[1];
                    }

                    
                    dgvR.Cells[1].Value = dataRow.areadata.name; //播放区域

                    switch (dataRow.prEvnType)
                    {
                        case "日常":

                            dgvR.Cells[2].Value = "日常广播播放中...";
                            dgvR.Cells[3].Value = "空闲";
                            break;                                   

                        case "应急":
                            dgvR.Cells[2].Value = "空闲";
                            dgvR.Cells[3].Value = "应急广播播放中...";
                            break;

                        case "未播放":
                            dgvR.Cells[2].Value ="空闲";
                            dgvR.Cells[3].Value = "空闲";
                            break;
                    
                    }
                    dgvR.Height =60;
                    dgvR.Tag = dataRow;
                    skinDataGridView_Main.Rows.Add(dgvR);
                    if (showtype==1)
                    {
                        dgvR.ReadOnly = true;
                    }
                    Application.DoEvents();    
                }
               
            }));
        }

        private void Showdgv_broadcastrecord(List<broadcastrecorddata> Listdata)
        {
            this.Invoke(new Action(() =>
            {
                BindingList<broadcastrecorddata> tmpList = new BindingList<broadcastrecorddata>();
                foreach (var item in Listdata)
                {
                    tmpList.Add(item);
                }
                dgv_broadcastrecord.DataSource = tmpList;
                dgv_broadcastrecord.Columns[1].Visible = false;
                dgv_broadcastrecord.Columns[2].HeaderText= "播出区域";
                dgv_broadcastrecord.Columns[3].Visible = false;
                dgv_broadcastrecord.Columns[4].HeaderText = "播放类型";
                dgv_broadcastrecord.Columns[5].Visible = false;
                dgv_broadcastrecord.Columns[6].Visible = false;
            }));
        }

        private void Showdgv_broadcastrecord(List<PlayRecord_tcp_ts> Listdata)
        {
            this.Invoke(new Action(() =>
            {
                BindingList<PlayRecord_tcp_ts> tmpList = new BindingList<PlayRecord_tcp_ts>();
                foreach (var item in Listdata)
                {
                    tmpList.Add(item);
                }
                dgv_broadcastrecord.DataSource = tmpList;
                dgv_broadcastrecord.Columns[1].Visible = false;
                dgv_broadcastrecord.Columns[2].Visible = false;
                dgv_broadcastrecord.Columns[3].HeaderText = "播出区域";
                dgv_broadcastrecord.Columns[4].HeaderText = "播放类型";
            }));
        }


        private void Showdgv_WhiteList(List<WhiteListRecord> Listdata)
        {
            this.Invoke(new Action(() =>
            {
                BindingList<WhiteListRecord> tmpList = new BindingList<WhiteListRecord>();
                foreach (var item in Listdata)
                {
                    tmpList.Add(item);
                }
                dgv_WhiteList.DataSource = tmpList;
            }));
        }

        /// <summary>
        /// 遍历树 找到勾选节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="checkednodes"></param>
        /// <returns></returns>
        public List<organizationdata> CheckedNodes(TreeNode parent, List<organizationdata> checkednodes)
        {
            TreeNode node = parent;
            if (node != null)
            {

                if (node.Checked == true && node.Parent == null)///首节点
                    checkednodes.Add(((organizationdata)node.Tag));
                if (node.Checked == true && node.FirstNode == null)
                    checkednodes.Add(((organizationdata)node.Tag));

                if (node.FirstNode != null)////如果node节点还有子节点则进入遍历
                {
                    CheckedNodes(node.FirstNode, checkednodes);
                }
                if (node.NextNode != null)////如果node节点后面有同级节点则进入遍历
                {
                    CheckedNodes(node.NextNode, checkednodes);
                }
            }
            return checkednodes;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (SingletonInfo.GetInstance().loginstatus)
                {

                    if (dgv_broadcastrecord.Rows.Count > 0)
                    {
                        List<string> IDList = new List<string>();
                        for (int i = 0; i < dgv_broadcastrecord.Rows.Count; i++)
                        {
                            if ((bool)dgv_broadcastrecord.Rows[i].Cells[0].EditedFormattedValue == true)
                            {
                                IDList.Add(dgv_broadcastrecord.Rows[i].Cells["prlId"].Value.ToString());
                            }
                        }
                        if (IDList.Count > 0)
                        {
                            Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(IDList, "停止");
                            if (stopresponse.code == 0)
                            {
                                List<broadcastrecorddata> Listdata = (List<broadcastrecorddata>)dgv_broadcastrecord.DataSource;
                                foreach (string item in IDList)
                                {
                                    broadcastrecorddata tmp = Listdata.Find(s => s.prlId.Equals(Convert.ToInt32(item)));
                                    Listdata.Remove(tmp);
                                }
                                Showdgv_broadcastrecord(Listdata);
                            }
                        }
                    }
                }
                else
                {
                    if (dgv_broadcastrecord.Rows.Count > 0)
                    {
                        List<PlayRecord_tcp_ts> IDList = new List<PlayRecord_tcp_ts>();
                        string IndexItemIDstr = "";
                        for (int i = 0; i < dgv_broadcastrecord.Rows.Count; i++)
                        {
                            if ((bool)dgv_broadcastrecord.Rows[i].Cells[0].EditedFormattedValue == true)
                            {
                                PlayRecord_tcp_ts tmp = (PlayRecord_tcp_ts)dgv_broadcastrecord.Rows[i].DataBoundItem;

                                IDList.Add(tmp);
                                IndexItemIDstr += tmp.IndexItemID+",";
                            }
                        }
                        if (IDList.Count > 0)
                        {
                          //  OnorOFFResponse stopresponse = TCPBroadcastcommand(IDList,"2");  20180711经商议 停播不发前端协议 
                          //  if (stopresponse.result_code == 0)
                           // {

                                IndexItemIDstr = IndexItemIDstr.Substring(0, IndexItemIDstr.Length - 1);

                                DelEBMIndex2Global(IndexItemIDstr);
                            List<PlayRecord_tcp_ts> Listdata = new List<PlayRecord_tcp_ts>();

                            BindingList<PlayRecord_tcp_ts>  dgvdatasouceList= (BindingList<PlayRecord_tcp_ts>)dgv_broadcastrecord.DataSource;
                            foreach (PlayRecord_tcp_ts item in IDList)
                            {
                                dgvdatasouceList.Remove(item);
                            }

                            foreach (var item in dgvdatasouceList)
                            {
                                Listdata.Add(item);
                            }
                                Showdgv_broadcastrecord(Listdata);

                           // }

                        }

                    }
                }
          
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "停止播放失败");
            }
        }
        
        public void UpdateDataTextNew(object tag)
        {
            try
            {
                if (IsStartStream)
                {

                    int type = (int)tag;
                    switch (type)
                    {
                        case 0:
                          //  EB_IndexScreenPrint();
                         //   EB_CertAuthScreenPrint();
                            break;
                        case 1:
                            EB_IndexScreenPrint();
                            break;
                        case 2:
                       //     EB_CertAuthScreenPrint();
                            break;
                        case 3:
                        //    EB_ConfigureScreenPrint();
                            break;
                        case 4:
                         //   EB_DailyBroadcastScreenPrint();
                            break;
                        case 5:
                         //   EB_ContentScreenPrint();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), ex.ToString());
            }
        }

        private void EB_IndexScreenPrint()
        {
            if (EbmStream.EB_Index_Table != null)
            {
                StringBuilder sb = new StringBuilder(DateTime.Now.ToString() + "\n");
                int num = 0;
                EbmStream.EB_Index_Table.BuildEbIndexSection();
                byte[] body = new byte[] { };
                do
                {
                    Thread.Sleep(800);
                    body = EbmStream.EB_Index_Table.GetEbIndexSection(ref num);
                }

                while (EbmStream.EB_Index_Table.Completed == false);

                if (body != null)
                {
                    for (int i = 0; i < body.Length; i++)
                    {
                        if (i != 0 && i % 16 == 0) sb.Append("\n");
                        sb.Append(body[i].ToString("X2").PadLeft(2, '0').ToUpper() + " ");
                    }
                    sb.Append("\n\n");
                }

                LogHelper.WriteLog(typeof(MainForm),"TS发送指令:(开机/停机指令):" + sb.ToString());
            }

        }
        
        private void pictureBox_Login_Click(object sender, EventArgs e)
        {
            bool loginflag = false;
            try
            {
                LoginInfo lginfo = new LoginInfo();
                lginfo.username = SingletonInfo.GetInstance().username;
                lginfo.password = SingletonInfo.GetInstance().password;
                lginfo.licenseCode = SingletonInfo.GetInstance().licenseCode;
                lginfo.localParam = new LocalParam();


                lginfo.localParam.btn_one = skinButton1.Text.Trim();
                lginfo.localParam.btn_two = skinButton9.Text.Trim();
                lginfo.localParam.btn_three = skinButton8.Text.Trim();

                lginfo.localParam.btn_four = skinButton3.Text.Trim();
                lginfo.localParam.btn_five = skinButton7.Text.Trim();
                lginfo.localParam.btn_six = skinButton6.Text.Trim();

                lginfo.localParam.lock_pwd = SingletonInfo.GetInstance().logincode;
                lginfo.localParam.lock_cycle = SingletonInfo.GetInstance().lockcycle;

                if (lginfo != null)
                {
                    LoginInfoReback reback = (LoginInfoReback)SingletonInfo.GetInstance().post.PostCommnand(lginfo, "登录");
                    if (reback != null)
                    {
                        if (reback.code == 0)
                        {
                            loginflag = true;
                            SingletonInfo.GetInstance().creditCode = reback.extend.creditCode;
                            SingletonInfo.GetInstance().loginstatus = true;//表示系统登录到县平台


                            #region 获取区域信息
                            organizationInfo reback1 = (organizationInfo)SingletonInfo.GetInstance().post.PostCommnand(null, "获取区域");

                            if (reback1 != null)
                            {
                                SingletonInfo.GetInstance().Organization = reback1.data;
                                #region 保存区域信息 
                                TableDataHelper.WriteTable(Enums.TableType.Organization, reback1.data);
                                #endregion

                                ShowtreeViewOrganization(reback1.data);
                                ShowtreeViewOrganization_WhiteList(reback1.data);
                                ShowtreeViewOrganization_RebackCycle(reback1.data);
                                ShowtreeViewOrganization_RebackParam(reback1.data);
                                ShowtreeViewOrganization_SwitchAmplifier(reback1.data);
                                ShowtreeViewOrganization_volumn(reback1.data);

                            }
                            #endregion

                            #region   生成显示信息
                            //获取HTTP播放列表
                            broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                            //生成显示数据
                            foreach (var item in SingletonInfo.GetInstance().Organization)
                            {
                                if (item.children.Count>0)
                                {
                                    foreach (var ite in item.children)
                                    {
                                        //非镇级                  
                                        Datagridviewmainitem addone = new Datagridviewmainitem();
                                        addone.areadata = ite;
                                        addone.checkstate = true;//其实默认勾选
                                        broadcastrecorddata playrecord = broadcastrecordresponse.data.Find(s => s.prAreaName.Equals(ite.name));
                                        if (playrecord != null)
                                        {
                                            addone.deviceoperate = "1";
                                            addone.prEvnType = playrecord.prEvnType;
                                            addone.prlId = playrecord.prlId.ToString();
                                        }
                                        else
                                        {
                                            addone.deviceoperate = "0";
                                            addone.prEvnType = "未播放";
                                            addone.prlId = "-1";
                                        }
                                        addone.IndexItemID = "-1";

                                        SingletonInfo.GetInstance().dgvMainData.Add(addone); 
                                    }
                                 
                                }
                            }

                            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
                            #endregion

                            #region 启动心跳及其他线程
                            threadHeart = new Thread(InitTimer);
                            threadHeart.IsBackground = true;
                            threadHeart.Start();
                            #endregion

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "登录到县平台失败！");
            }
            finally
            {
                pictureBox_Login.Visible = false;
                if (loginflag)
                {
                    pictureBox_online.Visible = true;
                    if (!SingletonInfo.GetInstance().SendCommandMode && SingletonInfo.GetInstance().loginstatus)
                    {
                      //  InitTimerServerTimer();
                    }
                }
                else
                {
                    pictureBox_offline.Visible = true;
                    var jo = TableDataHelper.ReadTable(Enums.TableType.Organization);
                    if (jo != null)
                    {
                        SingletonInfo.GetInstance().Organization = JsonConvert.DeserializeObject<List<organizationdata>>(jo["0"].ToString());
                    }

                    #region   生成skinDataGridView_Main的显示数据
                    //生成显示数据
                    foreach (var item in SingletonInfo.GetInstance().Organization)
                    {
                        if (item.children.Count > 0)
                        {
                            foreach (var ite in item.children)
                            {
                                //非镇级
                                Datagridviewmainitem addone = new Datagridviewmainitem();
                                addone.areadata = ite;
                                addone.checkstate = true;//起始默认勾选
                                //先默认都没有播放
                                addone.deviceoperate = "0";
                                addone.prEvnType = "未播放";
                                addone.prlId = "-1";
                                addone.IndexItemID = "-1";
                                SingletonInfo.GetInstance().dgvMainData.Add(addone);  //不包含镇信息
                            }
                        }
                    }
                    #endregion

                    ShowtreeViewOrganization(SingletonInfo.GetInstance().Organization);
                    ShowtreeViewOrganization_WhiteList(SingletonInfo.GetInstance().Organization);
                    ShowtreeViewOrganization_RebackCycle(SingletonInfo.GetInstance().Organization);
                    ShowtreeViewOrganization_RebackParam(SingletonInfo.GetInstance().Organization);
                    ShowtreeViewOrganization_SwitchAmplifier(SingletonInfo.GetInstance().Organization);
                    ShowtreeViewOrganization_volumn(SingletonInfo.GetInstance().Organization);
                    ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);

                  //  InitTimerServerTimer();
                }

                pictureBox_checkbox.BackgroundImage = imageList1.Images[0];

            }
        }

        private GeneralResponse TCPGeneralVolumn(List<organizationdata> organization_List, string volumn)
        {
            GeneralVolumn tt = new GeneralVolumn();

            tt.volume = volumn;
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            foreach (var item in organization_List)
            {
                tt.resource_codeList.Add(item.resource);
            }
            GeneralResponse resopnse = (GeneralResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt, 0x06);
            return resopnse;
        }

        private GeneralResponse TCPWhiteListUpdate(WhiteListUpdate whitelist)
        {
            GeneralResponse resopnse = (GeneralResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(whitelist, 0x0C);
            return resopnse;
        }

        private GeneralResponse TCPSwitchAmplifier(List<organizationdata> organization_List, string onoff)
        {
            SwitchAmplifier tt = new SwitchAmplifier();

            tt.switch_option = onoff;
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            foreach (var item in organization_List)
            {
                tt.resource_codeList.Add(item.resource);
            }
            GeneralResponse resopnse = (GeneralResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt, 0x3F);
            return resopnse;
        }

        private GeneralResponse TCPGeneralRebackParam(GeneralRebackParam tt)
        {
           
            GeneralResponse resopnse = (GeneralResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt, 0x07);
            return resopnse;
        }

        private GeneralResponse TCPRebackPeriod(GeneralRebackCycle tmp)
        {
            
            GeneralResponse resopnse = (GeneralResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tmp, 0x0B);
            return resopnse;
        }

        private OnorOFFResponse TCPBroadcastcommand(List<organizationdata> organization_List,string commandtype,string ebm_class)
        {
            OnorOFFBroadcast tt = new OnorOFFBroadcast();
            tt.ebm_class = ebm_class;
            tt.ebm_id = SingletonInfo.GetInstance().tcpsend.CreateEBM_ID();
            SingletonInfo.GetInstance().ebm_id = tt.ebm_id;
            tt.ebm_level = "2";//2级  重大
            tt.ebm_type = "00000";
            tt.end_time = DateTime.Now.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss");
            SingletonInfo.GetInstance().endtime = tt.end_time;


            tt.start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SingletonInfo.GetInstance().starttime = tt.start_time;
            tt.power_switch = commandtype;// 1开播  2停播   3切换通道
            tt.volume = "80";
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            foreach (var item in organization_List)
            {
                tt.resource_codeList.Add(item.resource);
            }
            tt.input_channel_id = Convert.ToInt32(SingletonInfo.GetInstance().input_channel_id); 
            OnorOFFResponse resopnse = (OnorOFFResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt, 0x04);
            return resopnse;
        }

        private OnorOFFResponse TCPBroadcastcommand(List<PlayRecord_tcp_ts> organization_List, string commandtype)
        {
            OnorOFFBroadcast tt = new OnorOFFBroadcast();
            tt.ebm_class = "4";
            tt.ebm_id = SingletonInfo.GetInstance().tcpsend.CreateEBM_ID();
            SingletonInfo.GetInstance().ebm_id = tt.ebm_id;
            tt.ebm_level = "2";//2级  重大
            tt.ebm_type = "00000";
            tt.end_time = DateTime.Now.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss");
            SingletonInfo.GetInstance().endtime = tt.end_time;


            tt.start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SingletonInfo.GetInstance().starttime = tt.start_time;
            tt.power_switch = commandtype;// 1开播  2停播   3切换通道
            tt.volume = "80";
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            foreach (var item in organization_List)
            {
                tt.resource_codeList.Add(item.resource_code);
            }
            tt.input_channel_id = Convert.ToInt32(SingletonInfo.GetInstance().input_channel_id);
            OnorOFFResponse resopnse = (OnorOFFResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt, 0x04);
            return resopnse;
        }

        private Dictionary<string, string> TSBroadcastcommand(List<string> organization_List, string pid, string ebm_class)
        {
            Dictionary<string, string> IndexItemIDic = new Dictionary<string, string>();

            foreach (var item in organization_List)
            {
                EBMIndexTmp tmp = new EBMIndexTmp();

                SingletonInfo.GetInstance().IndexItemID += 1;
                tmp.IndexItemID = SingletonInfo.GetInstance().IndexItemID.ToString();
                IndexItemIDic.Add(item,tmp.IndexItemID);
                tmp.S_EBM_class = ebm_class;//
                string ebm_id_tmp = SingletonInfo.GetInstance().tcpsend.CreateEBM_ID();
                tmp.S_EBM_id = ebm_id_tmp.Substring(5, ebm_id_tmp.Length - 5); 
                //应陈良要求 开始时间在当前时间的基础上减1小时 20190218
                tmp.S_EBM_start_time = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");
                int delay = Convert.ToInt32(SingletonInfo.GetInstance().EndtimeDelay);
                tmp.S_EBM_end_time = DateTime.Now.AddMinutes(delay).ToString("yyyy-MM-dd HH:mm:ss");
                tmp.S_EBM_level = "2";//
                tmp.S_EBM_original_network_id = "1";
                tmp.S_EBM_type = "00000";//
                tmp.List_EBM_resource_code = item;
                tmp.BL_details_channel_indicate = "true";
                tmp.S_details_channel_transport_stream_id = SingletonInfo.GetInstance().S_details_channel_transport_stream_id;
                tmp.S_details_channel_program_number = SingletonInfo.GetInstance().S_details_channel_program_number;
                tmp.S_details_channel_PCR_PID= SingletonInfo.GetInstance().S_details_channel_PCR_PID;
                tmp.List_ProgramStreamInfo = new List<ProgramStreamInfotmp>();
                ProgramStreamInfotmp pp = new ProgramStreamInfotmp();
                pp.B_stream_type = "4";
                pp.S_elementary_PID = SingletonInfo.GetInstance().ts_pid;
                tmp.List_ProgramStreamInfo.Add(pp);
                DealEBMIndex2Global(tmp);
            }
            return IndexItemIDic;
        }


        private List<string> TSBroadcastcommand(List<organizationdata> organization_List, string pid, string ebm_class)
        {
            List<string> IndexItemIDList = new List<string>();

            foreach (var item in organization_List)
            {
                EBMIndexTmp tmp = new EBMIndexTmp();

                SingletonInfo.GetInstance().IndexItemID += 1;
                tmp.IndexItemID = SingletonInfo.GetInstance().IndexItemID.ToString();
                IndexItemIDList.Add(tmp.IndexItemID);
                tmp.S_EBM_class = ebm_class;//
                string ebm_id_tmp = SingletonInfo.GetInstance().tcpsend.CreateEBM_ID();
                tmp.S_EBM_id = ebm_id_tmp.Substring(5, ebm_id_tmp.Length - 5);
                //应陈良要求 开始时间在当前时间的基础上增加1小时 20190218
                tmp.S_EBM_start_time = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                int delay = Convert.ToInt32(SingletonInfo.GetInstance().EndtimeDelay);
                tmp.S_EBM_end_time = DateTime.Now.AddMinutes(delay).ToString("yyyy-MM-dd HH:mm:ss");
                tmp.S_EBM_level = "2";//
                tmp.S_EBM_original_network_id = "1";
                // tmp.S_EBM_start_time = SingletonInfo.GetInstance().starttime;//
                tmp.S_EBM_type = "00000";//
                tmp.List_EBM_resource_code = item.resource;

                LogHelper.WriteLog(typeof(MainForm), "全镇开资源码："+ item.resource);


                tmp.BL_details_channel_indicate = "true";
                tmp.S_details_channel_transport_stream_id = SingletonInfo.GetInstance().S_details_channel_transport_stream_id;
                tmp.S_details_channel_program_number = SingletonInfo.GetInstance().S_details_channel_program_number;
                tmp.S_details_channel_PCR_PID = SingletonInfo.GetInstance().S_details_channel_PCR_PID;
                tmp.List_ProgramStreamInfo = new List<ProgramStreamInfotmp>();
                ProgramStreamInfotmp pp = new ProgramStreamInfotmp();
                pp.B_stream_type = "4";
                pp.S_elementary_PID = SingletonInfo.GetInstance().ts_pid;
                tmp.List_ProgramStreamInfo.Add(pp);
                DealEBMIndex2Global(tmp);
            }
            return IndexItemIDList;
        }


        /// <summary>
        /// 应急广播播发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_emergentbrd_Click(object sender, EventArgs e)
        {
            try
            {
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization.TopNode, organization_List);
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    if (organization_List.Count > 0)
                    {
                        SendPlayInfo palyinfo = new SendPlayInfo();
                        palyinfo.organization_List = new List<organizationdata>();
                        palyinfo.organization_List = organization_List;
                        palyinfo.broadcastType = "1";//表示应急

                        Generalresponse response = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(palyinfo, "播放");
                        if (response.code == 0)
                        {
                            broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                            Showdgv_broadcastrecord(broadcastrecordresponse.data);

                        }
                        else
                        {
                            MessageBox.Show("播放失败：" + response.msg);
                        }
                    }
                }
                else
                {
                    //离线状态 发送TS数据  同时也要发前端协议  内容和TS一样   资源码"0612"+12位区域码+"00"
                    if (organization_List.Count > 0)
                    {
                        #region 前端协议播发   20180711  商议结果  播放停止的时候  前端协议不发
                      // OnorOFFResponse res = TCPBroadcastcommand(organization_List, "1", "4");//"1"表示开播

                      //  SingletonInfo.GetInstance().ts_pid = res.result_desc;
                        #endregion

                        #region  TS指令 播发
                        List<string> IndexItemIDList = TSBroadcastcommand(organization_List, SingletonInfo.GetInstance().ts_pid, "0100");//此时的res.result_desc中存放的是pid数据

                        BindingList<PlayRecord_tcp_ts> datasource_BindList = (BindingList<PlayRecord_tcp_ts>)dgv_broadcastrecord.DataSource;

                        if (datasource_BindList == null)
                        {
                            datasource_BindList = new BindingList<PlayRecord_tcp_ts>();
                        }
                        for (int i = 0; i < organization_List.Count; i++)
                        {
                            PlayRecord_tcp_ts pp = new PlayRecord_tcp_ts();
                            pp.IndexItemID = IndexItemIDList[i];
                            pp.prAreaName = organization_List[i].name;
                            pp.prEvnType = "应急";
                            pp.resource_code = organization_List[i].resource;
                            datasource_BindList.Add(pp);
                        }
                        List<PlayRecord_tcp_ts> datasource = new List<PlayRecord_tcp_ts>();
                        foreach (var item in datasource_BindList)
                        {
                            datasource.Add(item);
                        }
                          Showdgv_broadcastrecord(datasource); 
                        #endregion
                        //if (res.result_code == 1)
                        //{
                        //    MessageBox.Show("播放失败：");
                        //}

                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog(typeof(MainForm), "播放失败");
            }
        }

        /// <summary>
        /// 日常广播播发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Dailybrd_Click(object sender, EventArgs e)
        {
            try
            {
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization.TopNode, organization_List);
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    if (organization_List.Count > 0)
                    {
                        SendPlayInfo palyinfo = new SendPlayInfo();
                        palyinfo.organization_List = new List<organizationdata>();
                        palyinfo.organization_List = organization_List;
                        palyinfo.broadcastType = "0";//1表示应急  0表示日常

                        Generalresponse response = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(palyinfo, "播放");
                        if (response.code == 0)
                        {
                            broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                            Showdgv_broadcastrecord(broadcastrecordresponse.data);

                        }
                        else
                        {
                            MessageBox.Show("播放失败：" + response.msg);
                        }
                    }
                }
                else
                {
                    //离线状态 发送TS数据  同时也要发前端协议  内容和TS一样   资源码"0612"+12位区域码+"00"
                    if (organization_List.Count > 0)
                    {
                        #region 前端协议播发   20180711  商议结果  播放停止的时候  前端协议不发
                        // OnorOFFResponse res = TCPBroadcastcommand(organization_List, "1", "4");//"1"表示开播

                        //  SingletonInfo.GetInstance().ts_pid = res.result_desc;
                        #endregion

                        #region  TS指令 播发
                        List<string> IndexItemIDList = TSBroadcastcommand(organization_List, SingletonInfo.GetInstance().ts_pid, "0101");//此时的res.result_desc中存放的是pid数据

                        BindingList<PlayRecord_tcp_ts> datasource_BindList = (BindingList<PlayRecord_tcp_ts>)dgv_broadcastrecord.DataSource;

                        if (datasource_BindList == null)
                        {
                            datasource_BindList = new BindingList<PlayRecord_tcp_ts>();
                        }
                        for (int i = 0; i < organization_List.Count; i++)
                        {
                            PlayRecord_tcp_ts pp = new PlayRecord_tcp_ts();
                            pp.IndexItemID = IndexItemIDList[i];
                            pp.prAreaName = organization_List[i].name;
                            pp.prEvnType = "日常";
                            pp.resource_code = organization_List[i].resource;
                            datasource_BindList.Add(pp);
                        }
                        List<PlayRecord_tcp_ts> datasource = new List<PlayRecord_tcp_ts>();
                        foreach (var item in datasource_BindList)
                        {
                            datasource.Add(item);
                        }
                        Showdgv_broadcastrecord(datasource);
                        #endregion
                        //if (res.result_code == 1)
                        //{
                        //    MessageBox.Show("播放失败：");
                        //}

                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog(typeof(MainForm), "播放失败");
            }
        }

        private void btn_Organization_Click(object sender, EventArgs e)
        {
            try
            {
                skinDataGridView_Main.Tag = (bool)true;
                skinDataGridView_Main.Location = new System.Drawing.Point(453, 214);
                skinDataGridView_Main.Size = new System.Drawing.Size(1030, 589);
                skinDataGridView_Main.Visible = true;
               
                //skinTabControl_Organization.Location = new System.Drawing.Point(436, 179);
                //skinTabControl_Organization.Size = new System.Drawing.Size(1038, 659);
                //skinTabControl_Organization.Visible = false;
                skinTabControl_parameterset.Visible = false;
                btn_Home1.Visible = true;
                pictureBox_checkbox.Visible = true;
                ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);                         

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void btn_parameterset_Click(object sender, EventArgs e)
        {
            skinTabControl_parameterset.Location = new System.Drawing.Point(436, 179);
            skinTabControl_parameterset.Size = new System.Drawing.Size(1038, 659);
            skinTabControl_parameterset.Visible = true;
            skinTabControl_Organization.Visible = false;
            btn_Home1.Visible = true;

            Showdgv_WhiteList(SingletonInfo.GetInstance().WhiteListRecordList);
        }

        private void btn_Home1_Click(object sender, EventArgs e)
        {
            btn_Home1.Visible = false;
            skinTabControl_Organization.Visible = false;
            skinTabControl_parameterset.Visible = false;
            skinDataGridView_Main.Visible = false;
            pictureBox_checkbox.Visible = false;

        }

        private void pictureBox_offline_DoubleClick(object sender, EventArgs e)
        {
            MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定关闭？" } };
            MessageShowDlg.ShowDialog();
            if (MessageShowDlg.IsSure)
            {
                  ini.WriteValue("EBM", "ebm_id_behind", SingletonInfo.GetInstance().ebm_id_behind);
                  ini.WriteValue("EBM", "ebm_id_count", SingletonInfo.GetInstance().ebm_id_count.ToString());
                  ini.WriteValue("EBM", "input_channel_id", SingletonInfo.GetInstance().input_channel_id);
                  ini.WriteValue("EBM", "IndexItemID", SingletonInfo.GetInstance().IndexItemID.ToString());


                  TableDataHelper.WriteTable(Enums.TableType.WhiteList, SingletonInfo.GetInstance().WhiteListRecordList);
                  if (EbmStream != null && IsStartStream)
                  {
                      EbmStream.StopStreaming();
                      IsStartStream = false;
                  }
                ShutdownWin();
                  Close();
              }
              GC.Collect();
        }

        private void ShutdownWin()
        {
            if (SingletonInfo.GetInstance().IsLogoutWin)
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardInput.WriteLine("shutdown -s -t 1");
                process.StandardInput.WriteLine("exit");
              //  Shuttime = System.Int32.Parse(Intime.Text);
                process.WaitForExit();
                process.Close();

            }
        }


        private OnorOFFResponse SwitchChannel(int channelID)
        {
        
            OnorOFFBroadcast tt = new OnorOFFBroadcast();
            tt.ebm_class = "4";
            tt.ebm_id = SingletonInfo.GetInstance().tcpsend.CreateEBM_ID();
            tt.ebm_level = "2";
            tt.ebm_type = "00000";
            tt.end_time = DateTime.Now.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss");
            tt.start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            tt.power_switch = "3";// 1开播  2停播   3切换通道
            tt.volume = "80";
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            tt.resource_codeList.Add("000000000000000000");
            tt.input_channel_id = channelID;
            OnorOFFResponse resopnse= (OnorOFFResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt, 0x04);
            return resopnse;

        }
        private void skinButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string btnmessage = skinButton1.Text;
                MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到"+btnmessage+"？" } };
                MessageShowDlg.ShowDialog();
                if (MessageShowDlg.IsSure)
                {
                    OnorOFFResponse res = SwitchChannel(1);//话筒目前定为1
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {

                        MessageBox.Show("切换到" + btnmessage + "失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "1";
                    }

                    skinButton1.ForeColor = System.Drawing.Color.Lime;
                    skinButton3.ForeColor = System.Drawing.Color.White;
                    skinButton4.ForeColor = System.Drawing.Color.White;
                    skinButton5.ForeColor = System.Drawing.Color.White;


                    skinButton9.ForeColor = System.Drawing.Color.White;
                    skinButton8.ForeColor = System.Drawing.Color.White;
                    skinButton7.ForeColor = System.Drawing.Color.White;
                    skinButton6.ForeColor = System.Drawing.Color.White;
                }

            }
            catch (Exception)
            {


            }
        }

        private void skinButton3_Click(object sender, EventArgs e)
        {
            try
            {
               
                    string btnmessage = skinButton3.Text;
                    MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到" + btnmessage + "？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                        OnorOFFResponse res = SwitchChannel(2);//U盘目前定为2
                        if (res.result_code == 1)  //0代表成功1代表失败
                        {

                            MessageBox.Show("切换到" + btnmessage + "失败");
                        }
                        else
                        {
                            SingletonInfo.GetInstance().input_channel_id = "2";
                        }

                        skinButton1.ForeColor = System.Drawing.Color.White;
                        skinButton3.ForeColor = System.Drawing.Color.Lime;
                        skinButton4.ForeColor = System.Drawing.Color.White;
                        skinButton5.ForeColor = System.Drawing.Color.White;

                        skinButton9.ForeColor = System.Drawing.Color.White;
                        skinButton8.ForeColor = System.Drawing.Color.White;
                        skinButton7.ForeColor = System.Drawing.Color.White;
                        skinButton6.ForeColor = System.Drawing.Color.White;
                    }
            }
            catch (Exception ex)
            {


            }
        }

        private void skinButton4_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton4.Text == "DVB")
                {
                    MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到DVC？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                        ///适配器暂不支持
                        //OnorOFFResponse res = SwitchChannel(3);//DVB-C目前定为3
                        //if (res.result_code == 1)  //0代表成功1代表失败
                        //{
                        //    MessageBox.Show("切换到DVB-C失败");
                        //}
                        //else
                        //{
                        //    SingletonInfo.GetInstance().input_channel_id = "3";
                        //}

                        skinButton1.ForeColor = System.Drawing.Color.White;
                        skinButton3.ForeColor = System.Drawing.Color.White;
                        skinButton4.ForeColor = System.Drawing.Color.Lime;
                        skinButton5.ForeColor = System.Drawing.Color.White;


                        skinButton9.ForeColor = System.Drawing.Color.White;
                        skinButton8.ForeColor = System.Drawing.Color.White;
                        skinButton7.ForeColor = System.Drawing.Color.White;
                        skinButton6.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    { 
                    
                    }
                }
                else
                {

                }
            }
            catch (Exception)
            {

              
            }
        }

        private void skinButton5_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton5.Text == "DTMB")
                {
                    //OnorOFFResponse res = SwitchChannel(4);//DTMB目前定为4
                    //if (res.result_code == 1)  //0代表成功1代表失败
                    //{
                    //    MessageBox.Show("切换到DVB-C失败");
                    //}
                    //else
                    //{
                    //    SingletonInfo.GetInstance().input_channel_id = "4";
                    //}

                      MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到DTMB？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                        skinButton1.ForeColor = System.Drawing.Color.White;
                        skinButton3.ForeColor = System.Drawing.Color.White;
                        skinButton4.ForeColor = System.Drawing.Color.White;
                        skinButton5.ForeColor = System.Drawing.Color.Lime;


                        skinButton9.ForeColor = System.Drawing.Color.White;
                        skinButton8.ForeColor = System.Drawing.Color.White;
                        skinButton7.ForeColor = System.Drawing.Color.White;
                        skinButton6.ForeColor = System.Drawing.Color.White;
                    }
                 
                }
                else
                {

                }
            }
            catch (Exception)
            {

               
            }
        }

        private void skinButton9_Click(object sender, EventArgs e)
        {
            try
            {

                string btnmessage = skinButton9.Text;
                MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到" + btnmessage + "？" } };
                MessageShowDlg.ShowDialog();
                if (MessageShowDlg.IsSure)
                {
                    OnorOFFResponse res = SwitchChannel(5);//线路一目前定为5
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到" + btnmessage + "失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "5";
                    }

                    skinButton1.ForeColor = System.Drawing.Color.White;
                    skinButton3.ForeColor = System.Drawing.Color.White;
                    skinButton4.ForeColor = System.Drawing.Color.White;
                    skinButton5.ForeColor = System.Drawing.Color.White;


                    skinButton9.ForeColor = System.Drawing.Color.Lime;
                    skinButton8.ForeColor = System.Drawing.Color.White;
                    skinButton7.ForeColor = System.Drawing.Color.White;
                    skinButton6.ForeColor = System.Drawing.Color.White;
                }
            }
            catch (Exception)
            {


            }
        }

        private void skinButton8_Click(object sender, EventArgs e)
        {
            try
            {
                string btnmessage = skinButton8.Text;
                MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到" + btnmessage + "？" } };
                MessageShowDlg.ShowDialog();
                if (MessageShowDlg.IsSure)
                {
                    OnorOFFResponse res = SwitchChannel(6);//线路二目前定为6
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到" + btnmessage + "失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "6";
                    }

                    skinButton1.ForeColor = System.Drawing.Color.White;
                    skinButton3.ForeColor = System.Drawing.Color.White;
                    skinButton4.ForeColor = System.Drawing.Color.White;
                    skinButton5.ForeColor = System.Drawing.Color.White;


                    skinButton9.ForeColor = System.Drawing.Color.White;
                    skinButton8.ForeColor = System.Drawing.Color.Lime;
                    skinButton7.ForeColor = System.Drawing.Color.White;
                    skinButton6.ForeColor = System.Drawing.Color.White;
                }
            }
            catch (Exception)
            {


            }
        }

        private void skinButton7_Click(object sender, EventArgs e)
        {
            try
            {

                string btnmessage = skinButton7.Text;
                MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到" + btnmessage + "？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                        OnorOFFResponse res = SwitchChannel(7);//调频一目前定为7
                        if (res.result_code == 1)  //0代表成功1代表失败
                        {
                            MessageBox.Show("切换到" + btnmessage + "失败");
                        }
                        else
                        {

                            SingletonInfo.GetInstance().input_channel_id = "7";
                        }

                        skinButton1.ForeColor = System.Drawing.Color.White;
                        skinButton3.ForeColor = System.Drawing.Color.White;
                        skinButton4.ForeColor = System.Drawing.Color.White;
                        skinButton5.ForeColor = System.Drawing.Color.White;


                        skinButton9.ForeColor = System.Drawing.Color.White;
                        skinButton8.ForeColor = System.Drawing.Color.White;
                        skinButton7.ForeColor = System.Drawing.Color.Lime;
                        skinButton6.ForeColor = System.Drawing.Color.White;
                    }
               
            }
            catch (Exception)
            {

             
            }
        }

        private void skinButton6_Click(object sender, EventArgs e)
        {
            try
            {
               
                    string btnmessage = skinButton6.Text;
                    MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定切换到" + btnmessage + "？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                        OnorOFFResponse res = SwitchChannel(8);//调频二目前定为8
                        if (res.result_code == 1)  //0代表成功1代表失败
                        {
                            MessageBox.Show("切换到" + btnmessage + "失败");
                        }
                        else
                        {
                            SingletonInfo.GetInstance().input_channel_id = "8";
                        }

                        skinButton1.ForeColor = System.Drawing.Color.White;
                        skinButton3.ForeColor = System.Drawing.Color.White;
                        skinButton4.ForeColor = System.Drawing.Color.White;
                        skinButton5.ForeColor = System.Drawing.Color.White;


                        skinButton9.ForeColor = System.Drawing.Color.White;
                        skinButton8.ForeColor = System.Drawing.Color.White;
                        skinButton7.ForeColor = System.Drawing.Color.White;
                        skinButton6.ForeColor = System.Drawing.Color.Lime;
                    }
            }
            catch (Exception)
            {

            
            }
        }

        private void DelEBMIndex2GlobalAll()
        {
            lock (Gtoken)
            {
                if (_EBMIndexGlobal.ListEbIndex != null)
                {

                    _EBMIndexGlobal.ListEbIndex.Clear();
                }
                EbmStream.EB_Index_Table = GetEBIndexTable(ref EB_Index_Table) ? EB_Index_Table : null;
                EbMStream.Initialization();

                if (SingletonInfo.GetInstance().IsStartSend)
                {
                    Thread.Sleep(2000);//测试数据  20190126
                    EbmStream.StopStreaming();
                    SingletonInfo.GetInstance().IsStartSend = false;
                }

            }
            //  UpdateDataTextNew((object)1);
            GC.Collect();
        }

        private void DelEBMIndex2Global(string IndexItemIDstr)
        {
            lock (Gtoken)
            {
                if (_EBMIndexGlobal.ListEbIndex != null)
                {
                    string[] IndexItemIDArray = IndexItemIDstr.Split(',');
                    foreach (string item in IndexItemIDArray)
                    {
                        List<EBMIndex_> tmp = _EBMIndexGlobal.ListEbIndex.FindAll(s => s.IndexItemID.StartsWith(item));
                        if (tmp != null)
                        {
                            foreach (var ite in tmp)
                            {
                                _EBMIndexGlobal.ListEbIndex.Remove(ite);
                            }
                        }
                    }
                }
                EbmStream.EB_Index_Table = GetEBIndexTable(ref EB_Index_Table) ? EB_Index_Table : null;
                EbMStream.Initialization();

                #region 判断是否关闭通讯库的流输出
                if (_EBMIndexGlobal.ListEbIndex.Count == 0 && SingletonInfo.GetInstance().IsStartSend)
                {

                    Thread.Sleep(2000);//测试数据   20190126
                    EbmStream.StopStreaming();
                    SingletonInfo.GetInstance().IsStartSend = false;
                }
                #endregion
            }
          //  UpdateDataTextNew((object)1);
            GC.Collect();
        }

        private void DealEBMIndex2Global(EBMIndexTmp EBMIndex)
        {

            try
            {
                lock (Gtoken)
                {
                    if (_EBMIndexGlobal.ListEbIndex != null)
                    {
                        //去同向
                        EBMIndex_ tmp = _EBMIndexGlobal.ListEbIndex.Find(s => s.IndexItemID.Equals(EBMIndex.IndexItemID));
                        if (tmp != null)
                        {
                            _EBMIndexGlobal.ListEbIndex.Remove(tmp);
                        }
                    }
                    //增加新项
                    EBMIndex_ index = new EBMIndex_();

                    index.SendState = true;
                    index.EBIndex = new EBIndex();
                    index.EBIndex.ProtocolGX = SingletonInfo.GetInstance().IsGXProtocol;
                    index.S_EBM_id = EBMIndex.S_EBM_id;
                    index.S_EBM_original_network_id = EBMIndex.S_EBM_original_network_id;
                    index.S_EBM_start_time = EBMIndex.S_EBM_start_time;
                    index.S_EBM_end_time = EBMIndex.S_EBM_end_time;
                    index.S_EBM_type = EBMIndex.S_EBM_type;
                    index.S_EBM_class = EBMIndex.S_EBM_class;
                    index.S_EBM_level = EBMIndex.S_EBM_level;
                    index.IndexItemID = EBMIndex.IndexItemID;
                    index.List_EBM_resource_code = new List<string>();

                    ///注：通讯库不支持 List的Add模式 
                    ///

                    string[] List_EBM_resource_codeArray = EBMIndex.List_EBM_resource_code.Split(',');
                    //gan
                    if (SingletonInfo.GetInstance().IsGXProtocol)
                    {
                        for (int i = 0; i < List_EBM_resource_codeArray.Length; i++)
                        {
                            int resource_code_length = List_EBM_resource_codeArray[i].Length;


                            //20180525 陈良要求修改特殊处理
                            switch (resource_code_length)
                            {
                                case 18:
                                    break;
                                case 23:
                                    string tt = List_EBM_resource_codeArray[i].Substring(1);
                                    string tt1 = tt.Substring(0, tt.Length - 4);
                                    List_EBM_resource_codeArray[i] = tt1;
                                    break;
                                case 12:
                                    List_EBM_resource_codeArray[i] = "0612" + List_EBM_resource_codeArray[i] + "00";
                                    break;
                            }
                        }
                    }

                    index.List_EBM_resource_code = new List<string>(List_EBM_resource_codeArray);
                    index.BL_details_channel_indicate = EBMIndex.BL_details_channel_indicate == "true" ? true : false;
                    index.DesFlag = EBMIndex.DesFlag == "true" ? true : false;
                    index.S_details_channel_transport_stream_id = EBMIndex.S_details_channel_transport_stream_id;
                    index.S_details_channel_program_number = EBMIndex.S_details_channel_program_number;
                    index.S_details_channel_PCR_PID = EBMIndex.S_details_channel_PCR_PID;

                    if (index.DesFlag)
                        index.DeliverySystemDescriptor = GetDataDSD(EBMIndex.DeliverySystemDescriptor, EBMIndex.descriptor_tag);
                    if (index.BL_details_channel_indicate)
                    {

                     //   List<ProgramStreamInfotmp> List_ProgramStreamInfotmp = new List<ProgramStreamInfotmp>();//S_elementary_PID 中有“，”时，临时加入项
                     //   int List_ProgramStreamInfoLength = EBMIndex.List_ProgramStreamInfo.Count;//详情频道节目流信息列表长度
                        //for (int i = 0; i < List_ProgramStreamInfoLength; i++)
                        //{
                        // //   string S_elementary_PID = EBMIndex.List_ProgramStreamInfo[i].S_elementary_PID;

                        //    //if (S_elementary_PID.Contains(","))
                        //    //{
                        //    //    string[] pidarray = S_elementary_PID.Split(',');


                        //    //    EBMIndex.List_ProgramStreamInfo[i].S_elementary_PID = pidarray[0];

                        //    //    EBMIndex.List_ProgramStreamInfo[i].B_stream_type = "3";



                        //    //    ProgramStreamInfotmp add = new ProgramStreamInfotmp();
                        //    //    add.B_stream_type = "1";
                        //    //    add.Descriptor2 = null;
                        //    //    //add.Descriptor2
                        //    //    add.S_elementary_PID = pidarray[1];
                        //    //    List_ProgramStreamInfotmp.Add(add);

                        //    //}测试注释  原先分为两路下来 一路音频 一路视频




                        //}



                      //  List_ProgramStreamInfotmp = EBMIndex.List_ProgramStreamInfo;
                        //if (List_ProgramStreamInfotmp.Count > 0)
                        //{
                        //    foreach (var item in List_ProgramStreamInfotmp)
                        //    {
                        //        EBMIndex.List_ProgramStreamInfo.Add(item);
                        //    }
                        //}
                        index.List_ProgramStreamInfo = GetDataPSI(EBMIndex.List_ProgramStreamInfo);
                    }

                    index.NickName = "";

                    if (_EBMIndexGlobal.ListEbIndex == null)
                    {
                        _EBMIndexGlobal.ListEbIndex = new List<EBMIndex_>();
                    }
                    _EBMIndexGlobal.ListEbIndex.Add(index);

                    #region 判断是否将TS通讯库的流输出打开
                    if (_EBMIndexGlobal.ListEbIndex.Count>0 && !SingletonInfo.GetInstance().IsStartSend)
                    {
                        EbmStream.StartStreaming();
                        SingletonInfo.GetInstance().IsStartSend = true;
                    }
                    #endregion

                    EbmStream.EB_Index_Table = GetEBIndexTable(ref EB_Index_Table) ? EB_Index_Table : null;
                    EbMStream.Initialization();
                }
              //  UpdateDataTextNew((object)1); // 暂时去除打印
                GC.Collect();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "TS播放/停止指令发送失败");
            }

        }

        public List<ProgramStreamInfo> GetDataPSI(List<ProgramStreamInfotmp> input)
        {
            List<ProgramStreamInfo> list = new List<ProgramStreamInfo>();

            foreach (ProgramStreamInfotmp item in input)
            {
                ProgramStreamInfo tmp = new ProgramStreamInfo();
                tmp.B_stream_type = (byte)Convert.ToInt32(item.B_stream_type);
                tmp.S_elementary_PID = item.S_elementary_PID;
                tmp.Descriptor2 = new StdDescriptor();

                //添加于20180531
                tmp.Descriptor2 = null;




                // Descriptor2 descriptor2 = new Descriptor2();
                //if (item.Descriptor2 == null)
                //{
                //    descriptor2.B_descriptor_tag = (byte)1;
                //    descriptor2.B_descriptor = new byte[] { 0 };
                //}
                //else
                //{
                //      dynamic a = item.Descriptor2;
                //      descriptor2.B_descriptor_tag = ((byte)Convert.ToInt32(a[0]["B_descriptor_tag"]));
                //      descriptor2.B_descriptor = new byte[] { ((byte)Convert.ToInt32(a[0]["B_descriptor"])) };
                //}
                //if (descriptor2!=null)
                //{
                //    if (descriptor2.B_descriptor_tag == null && descriptor2.B_descriptor == null)
                //    {
                //        descriptor2 = null;
                //    }
                //    else
                //    {
                //        tmp.Descriptor2.B_descriptor_tag = (byte)Convert.ToInt32(descriptor2.B_descriptor_tag);

                //        //string[] descriptors = descriptor2.B_descriptor.Split(' ');
                //        //List<byte> array = new List<byte>();
                //        //foreach (string ite in descriptors)
                //        //{
                //        //    array.Add((byte)Convert.ToInt32(ite, 16));
                //        //}
                //        //tmp.Descriptor2.Br_descriptor = array.ToArray();

                //        tmp.Descriptor2.Br_descriptor = descriptor2.B_descriptor;
                //    }


                //}




                list.Add(tmp);
            }

            return list;
        }

        private object GetDataDSD(object input, int type)
        {
            switch (type)
            {
                case 68://有线传送系统描述符
                    Cable_delivery_system_descriptor cdsd = new Cable_delivery_system_descriptor();
                    CableDeliverySystemDescriptortmp tmp = (CableDeliverySystemDescriptortmp)input;
                    cdsd.B_FEC_inner = (byte)Convert.ToInt32(tmp.B_FEC_inner);
                    cdsd.B_FEC_outer = (byte)Convert.ToInt32(tmp.B_FEC_outer);
                    cdsd.B_Modulation = (byte)Convert.ToInt32(tmp.B_Modulation);
                    cdsd.D_frequency = Convert.ToDouble(tmp.D_frequency);
                    cdsd.D_Symbol_rate = Convert.ToDouble(tmp.D_Symbol_rate);
                    return cdsd;
                case 90://地面传送系统描述符
                    Terristrial_delivery_system_descriptor tdsd = new Terristrial_delivery_system_descriptor();
                    TerristrialDeliverySystemDescriptortmp tmp1 = (TerristrialDeliverySystemDescriptortmp)input;
                    tdsd.B_FEC = (byte)Convert.ToInt32(tmp1.B_FEC);
                    tdsd.B_Frame_header_mode = (byte)Convert.ToInt32(tmp1.B_Frame_header_mode);
                    tdsd.B_Interleaveing_mode = (byte)Convert.ToInt32(tmp1.B_Interleaveing_mode);
                    tdsd.B_Modulation = (byte)Convert.ToInt32(tmp1.B_Modulation);
                    tdsd.B_Number_of_subcarrier = (byte)Convert.ToInt32(tmp1.B_Number_of_subcarrier); ;
                    tdsd.D_Centre_frequency = Convert.ToDouble(tmp1.D_Centre_frequency);
                    tdsd.L_Other_frequency_flag = tmp1.L_Other_frequency_flag == "true" ? true : false;
                    tdsd.L_Sfn_mfn_flag = tmp1.L_Sfn_mfn_flag == "true" ? true : false;
                    return tdsd;
            }
            return null;
        }

        public bool GetEBIndexTable(ref EBIndexTable oldTable)
        {
            try
            {
                List<EBIndex> listEbIndex = dataDealHelper.GetSendEBMIndex(_EBMIndexGlobal.ListEbIndex);
                oldTable.ListEbIndex = listEbIndex;
                oldTable.Repeat_times = 0;//重复发送
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        
        private void btn_AddWhiteList_Click(object sender, EventArgs e)
        {
            try
            {
                btn_AddWhiteList.Visible = false;
                btn_DelWhiteList.Visible = false;
                dgv_WhiteList.Visible = false;
            }
            catch (Exception)
            {

               
            }
        }

        private void btn_OK_AddWhiteList_Click(object sender, EventArgs e)
        {
            try
            {

                if (txt_username.Text == "")
                {
                    MessageBox.Show("请输入用户名！");
                    txt_username.Focus();
                    return;
                }

                if (txt_phonenumber.Text == "")
                {
                    MessageBox.Show("请输入手机号码！");
                    txt_phonenumber.Focus();
                    return;
                }
                btn_AddWhiteList.Visible = true;
                btn_DelWhiteList.Visible = true;
                dgv_WhiteList.Visible = true;

                WhiteListRecord tmpAdd = new WhiteListRecord();
                tmpAdd.username = txt_username.Text;
                tmpAdd.phone_number = txt_phonenumber.Text;



                WhiteListUpdate senddata = new WhiteListUpdate();
                senddata.white_list = new List<WhiteListInfo>();
                WhiteListInfo pp = new WhiteListInfo();
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization_WhiteList.TopNode, organization_List);

                pp.oper_type = "1"; //操纵类型 1：增加 2：修改 3：删除
                pp.phone_number = txt_phonenumber.Text;
                pp.user_name = txt_username.Text;
                pp.permission_type = "2";//许可类型1:代表短信;2:代表电话;3代表短信和电话
                pp.permission_area_codeList = new List<string>();


                if (organization_List.Count > 0)
                {
                    foreach (var item in organization_List)
                    {
                        tmpAdd.Organizations += item.name + ",";
                        tmpAdd.gb_codes += item.gb_code + ",";
                        pp.permission_area_codeList.Add(item.gb_code);
                    }
                    senddata.white_list.Add(pp);
                    tmpAdd.gb_codes = tmpAdd.gb_codes.Substring(0, tmpAdd.gb_codes.Length - 1);
                    tmpAdd.Organizations = tmpAdd.Organizations.Substring(0, tmpAdd.Organizations.Length - 1);

                    SingletonInfo.GetInstance().WhiteListRecordList.Add(tmpAdd);
                    Showdgv_WhiteList(SingletonInfo.GetInstance().WhiteListRecordList);
              //      GeneralResponse res = TCPWhiteListUpdate(senddata); // 暂时先注释20180711

                }
                txt_username.Text = "";
                txt_phonenumber.Text = "";
            }
            catch (Exception)
            {
            }
        } 

        private void btn_RebackCycle_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_RebackCycle.Text == "")
                {
                    MessageBox.Show("请输入周期！");
                    txt_RebackCycle.Focus();
                    return;
                }
               
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization_RebackCycle.TopNode, organization_List);
                GeneralRebackCycle Addtmp = new GeneralRebackCycle();
                Addtmp.reback_cycle = txt_RebackCycle.Text.Trim();
                Addtmp.resource_code_type = "1";
                Addtmp.resource_codeList = new List<string>();
                foreach (var item in organization_List)
                {
                    Addtmp.resource_codeList.Add(item.resource);
                }
                  GeneralResponse res = TCPRebackPeriod(Addtmp); //暂时先注释20180711

                MessageBox.Show("设置成功");

                
            }
            catch (Exception ex)
            {

                throw;
            }
        }
     
        private void btn_DelWhiteList_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_WhiteList.Rows.Count > 0)
                {
                    List<WhiteListRecord> IDList = new List<WhiteListRecord>();
                   
                    for (int i = 0; i < dgv_WhiteList.Rows.Count; i++)
                    {
                        if ((bool)dgv_WhiteList.Rows[i].Cells[0].EditedFormattedValue == true)
                        {
                            WhiteListRecord tmp = (WhiteListRecord)dgv_WhiteList.Rows[i].DataBoundItem;

                            IDList.Add(tmp);
                        }
                    }
                    if (IDList.Count > 0)
                    {

                        BindingList<WhiteListRecord> dgvwhiteList = (BindingList<WhiteListRecord>)dgv_WhiteList.DataSource;
                        List<WhiteListRecord> Listdata = new List<WhiteListRecord>();

                        WhiteListUpdate DelWhiteList = new WhiteListUpdate();
                        DelWhiteList.white_list = new List<WhiteListInfo>();

                        foreach (var item in IDList)
                        {
                            WhiteListInfo selctone = new WhiteListInfo();
                            selctone.user_name = item.username;
                            selctone.oper_type = "3";//表示删除
                            selctone.phone_number = item.phone_number;
                            selctone.permission_type = "2";//当前只能当做电话
                            selctone.permission_area_codeList = new List<string>();
                            string[] gb_code = item.gb_codes.Split(',');
                            foreach (var code in gb_code)
                            {
                                selctone.permission_area_codeList.Add(code);
                            }
                            DelWhiteList.white_list.Add(selctone);

                            dgvwhiteList.Remove(item);
                        }

                      //  GeneralResponse res = TCPWhiteListUpdate(DelWhiteList); // 暂时先注释20180711
                        foreach (WhiteListRecord item in dgvwhiteList)
                        {
                            Listdata.Add(item);
                        }

                        SingletonInfo.GetInstance().WhiteListRecordList = Listdata;
                        Showdgv_WhiteList(Listdata);

                        // }

                    }

                }
            }
            catch (Exception)
            { 
            }
        }

        private void btn_RebackParam_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_reback_address.Text == "")
                {
                    MessageBox.Show("请输入地址信息！");
                    txt_reback_address.Focus();
                    return;
                }

                GeneralRebackParam addtmp = new GeneralRebackParam();
                addtmp.reback_type = "";
                if (radioBtn_Message.Checked)
                {
                    addtmp.reback_type = "1";
                }
                if (radioBtn_IPandPort.Checked)
                {
                    addtmp.reback_type = "2";
                }

                if (radioBtn_DomainandPort.Checked)
                {
                    addtmp.reback_type = "3";
                }

                addtmp.reback_address = txt_reback_address.Text.Trim();

                addtmp.resource_code_type = "1";
                addtmp.resource_codeList = new List<string>();
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization_RebackCycle.TopNode, organization_List);
                foreach (var item in organization_List)
                {
                    addtmp.resource_codeList.Add(item.resource);
                }

                GeneralResponse res = TCPGeneralRebackParam(addtmp);// 暂时先注释20180711

                MessageBox.Show("设置成功");
            }
            catch (Exception)
            {
            }
        }

        private void btn_Cancel_AddWhiteList_Click(object sender, EventArgs e)
        {
            try
            {
                btn_AddWhiteList.Visible = true;
                btn_DelWhiteList.Visible = true;
                dgv_WhiteList.Visible = true;
                txt_username.Text = "";
                txt_phonenumber.Text = "";
            }
            catch (Exception)
            {
            }
        }

        private void btn_SwitchAmplifier_Click(object sender, EventArgs e)
        {
            try
            {
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization_SwitchAmplifier.TopNode, organization_List);
                string switchstatus = "";
                if (radioButton_Amplifier_On.Checked)
                {
                    switchstatus = "2";
                }

                if (radioButton_Amplifier_Off.Checked)
                {
                    switchstatus = "1";
                }
               // GeneralResponse res = TCPSwitchAmplifier(organization_List, switchstatus);//1表示关闭喇叭  2表示打开喇叭

                MessageBox.Show("设置成功");
            }
            catch (Exception)
            {
            }
        }

        private void btn_volumn_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_volumn.Text=="")
                {
                    MessageBox.Show("请输入音量值");
                    txt_volumn.Focus();
                    return;
                }
                List<organizationdata> organization_List = new List<organizationdata>();
                organization_List = CheckedNodes(treeViewOrganization_volumn.TopNode, organization_List);
                string volumn = txt_volumn.Text.Trim();
          
               // GeneralResponse res = TCPGeneralVolumn(organization_List, volumn);

                MessageBox.Show("设置成功");
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 操作主表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinDataGridView_Main_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == RecordSelect.Index && e.RowIndex >= 0 && !skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
            {
                CheckboxSelected(e);
            }
         //   if (e.ColumnIndex == Emergency.Index && e.RowIndex >= 0 &&
         //       !skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
         //   {
         //       OperatesingleDevice(e,"应急");
         //   }

         //   if (e.ColumnIndex == Daily.Index && e.RowIndex >= 0 &&
         //!skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
         //   {
         //       OperatesingleDevice(e,"日常");
         //   }
        }

        private void CheckboxSelected(DataGridViewCellEventArgs e)
        {
            DataGridViewRow selectedRow = skinDataGridView_Main.Rows[e.RowIndex];
            Datagridviewmainitem Item = (Datagridviewmainitem)selectedRow.Tag;
            if (Item.checkstate)
            {
                Item.checkstate = false;
            }
            else
            {
                Item.checkstate = true;
            }

            selectedRow.Tag = Item;

            Datagridviewmainitem Item_Global = SingletonInfo.GetInstance().dgvMainData.Find(s => s.areaname.Equals(Item.areaname));
            Item_Global.checkstate = Item.checkstate;
            if (Item.checkstate)
            {
                skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].Value=imageList1.Images[0];
            }
            else
            {
                skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = imageList1.Images[1];
            }
           
        }

        private void OperatesingleDevice(DataGridViewCellEventArgs e, string broadcastType)
        {
            DataGridViewRow selectedRow = skinDataGridView_Main.Rows[e.RowIndex];
            Datagridviewmainitem Item = (Datagridviewmainitem)selectedRow.Tag;

            if (Item.deviceoperate == "0")  // 1播放  0停止
            {
                #region  发开机指令
                //判断与县平台是否联通
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    string id = Item.areadata.id.ToString();

                    SendPlayInfoNew SendPlayInfosingle = new SendPlayInfoNew();
                    SendPlayInfosingle.broadcastType = broadcastType == "日常" ? "0" : "1";     // 0：日常      //1：应急
                    SendPlayInfosingle.Id_List = new List<string>();
                    SendPlayInfosingle.Id_List.Add(id);
                    Generalresponse response = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(SendPlayInfosingle, "图标播放");

                    Thread.Sleep(3000);
                    broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                    broadcastrecorddata selectedone = broadcastrecordresponse.data.Find(s => s.prAreaName.Equals(Item.areaname));
                    Item.prlId = selectedone.prlId.ToString();
                    Item.prEvnType = selectedone.prEvnType;
                }
                else
                {
                    #region  TS指令 播发
                    List<string> ResourceList = new List<string>();
                    ResourceList.Add(Item.areadata.resource);
                    string ebm_class = broadcastType == "日常" ? "0101" : "0100";
                    Dictionary<string, string> IndexItemIDic = TSBroadcastcommand(ResourceList, SingletonInfo.GetInstance().ts_pid, ebm_class);//此时的res.result_desc中存放的是pid数据
                    Item.IndexItemID = IndexItemIDic[ResourceList[0]];
                    Item.prEvnType = broadcastType;
                    #endregion
                }
                Item.deviceoperate = "1";
                selectedRow.Tag = Item;
                Datagridviewmainitem Item_Global = SingletonInfo.GetInstance().dgvMainData.Find(s => s.areaname.Equals(Item.areaname));
                Item_Global = Item;
                if (Item.prEvnType=="日常")
                {
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = imageList1.Images[2];
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].ReadOnly = true;
                }
                if (Item.prEvnType == "应急")
                {
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = imageList1.Images[2];
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].ReadOnly = true;
                }
                #endregion
            }
            else
            {
                #region  发关机指令
                //判断与县平台是否联通
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    List<string> IDList = new List<string>();
                    IDList.Add(Item.prlId);
                    Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(IDList, "停止");
                    Item.prlId = "-1";
                }
                else
                {
                    #region  TS指令 停止
                    string IndexItemIDstr = Item.IndexItemID;
                    DelEBMIndex2Global(IndexItemIDstr);
                    Item.IndexItemID = "-1";
                    #endregion
                }
                Item.deviceoperate = "0";
                Item.prEvnType="未播放";
                selectedRow.Tag = Item;
                Datagridviewmainitem Item_Global = SingletonInfo.GetInstance().dgvMainData.Find(s => s.areaname.Equals(Item.areaname));
                Item_Global = Item;
                if (Item.prEvnType == "未播放")
                {
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[2].Value = imageList1.Images[3];
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[3].Value = imageList1.Images[3];
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[2].ReadOnly = false;
                    skinDataGridView_Main.Rows[e.RowIndex].Cells[3].ReadOnly = false;
                }
                #endregion
            }
        }

        private void skinDataGridView_Main_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (e.Value.ToString() == "日常广播播放中...")
            {
                e.CellStyle.ForeColor = Color.Green;
                Application.DoEvents();

            }
            if (e.Value.ToString() == "应急广播播放中...")
            {
                e.CellStyle.ForeColor = Color.Red;
                Application.DoEvents();
            }
        }

        private void btn_input_channel_Update_Click(object sender, EventArgs e)
        {
            try
            {

                if (btn_input_channel_Update.Text=="线路切换")
                {
                    MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定更新线路？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                     

                    }

                }
           

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void btn_Organization_Update_Click(object sender, EventArgs e)
        {
            try
            {
                if (SingletonInfo.GetInstance().loginstatus)
                {
                    MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定区域信息？" } };
                    MessageShowDlg.ShowDialog();
                    if (MessageShowDlg.IsSure)
                    {
                        #region 获取区域信息
                        organizationInfo reback1 = (organizationInfo)SingletonInfo.GetInstance().post.PostCommnand(null, "获取区域");

                        if (reback1 != null)
                        {
                            SingletonInfo.GetInstance().Organization = reback1.data;
                            #region 保存区域信息
                            TableDataHelper.WriteTable(Enums.TableType.Organization, reback1.data);
                            #endregion

                            ShowtreeViewOrganization(reback1.data);
                            ShowtreeViewOrganization_WhiteList(reback1.data);
                            ShowtreeViewOrganization_RebackCycle(reback1.data);
                            ShowtreeViewOrganization_RebackParam(reback1.data);
                            ShowtreeViewOrganization_SwitchAmplifier(reback1.data);
                            ShowtreeViewOrganization_volumn(reback1.data);

                        }
                        #endregion

                        #region   生成显示信息
                        //获取HTTP播放列表
                        broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                        //生成显示数据
                        foreach (var item in SingletonInfo.GetInstance().Organization)
                        {
                            if (item.children.Count > 0)
                            {
                                foreach (var ite in item.children)
                                {
                                    //非镇级                  
                                    Datagridviewmainitem addone = new Datagridviewmainitem();
                                    addone.areadata = ite;
                                    addone.checkstate = true;//其实默认勾选
                                    broadcastrecorddata playrecord = broadcastrecordresponse.data.Find(s => s.prAreaName.Equals(ite.name));
                                    if (playrecord != null)
                                    {
                                        addone.deviceoperate = "1";
                                        addone.prEvnType = playrecord.prEvnType;
                                        addone.prlId = playrecord.prlId.ToString();
                                    }
                                    else
                                    {
                                        addone.deviceoperate = "0";
                                        addone.prEvnType = "未播放";
                                        addone.prlId = "-1";
                                    }
                                    addone.IndexItemID = "-1";

                                    SingletonInfo.GetInstance().dgvMainData.Add(addone);
                                }

                            }
                        }

                        ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
                        #endregion
                    
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                
                throw;
            }                                                            
        }


        private void OnlineAllStart(object obj)
        {
            string broadcastType = (string)obj;
            SendPlayInfo palyinfo = new SendPlayInfo();
            palyinfo.broadcastType = broadcastType;//0：日常  1：应急
            palyinfo.organization_List = new List<organizationdata>();
            List<string> STOP_ID_List = new List<string>();
            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                if (item.deviceoperate == "1")
                {
                    STOP_ID_List.Add(item.prlId);
                    item.deviceoperate = "0";
                    item.prEvnType = "未播放";
                    item.prlId = "-1";
                }
              //  palyinfo.organization_List.Add(item.areadata);
            }
            palyinfo.organization_List.Add(SingletonInfo.GetInstance().Organization[0]);

            if (STOP_ID_List.Count>0)
            {
                Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(STOP_ID_List, "停止");
            }

            Thread.Sleep(2500);
            Generalresponse response = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(palyinfo, "播放");
            if (response.code == 0)
            {
                Thread.Sleep(2500);
                broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                if (broadcastrecordresponse.data.Count > 0)
                {
                    //foreach (var item in broadcastrecordresponse.data)
                    //{
                    //    Datagridviewmainitem selected = SingletonInfo.GetInstance().dgvMainData.Find(s => s.areaname.Equals(item.prAreaName));
                    //    selected.checkstate = true;
                    //    selected.deviceoperate = "1";
                    //    selected.prEvnType = item.prEvnType;
                    //    selected.prlId = item.prlId.ToString();
                    //}
                    SingletonInfo.GetInstance().Interstitial_prlId = broadcastrecordresponse.data[0].prlId.ToString();
                }
            }
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData,1);
        }

        /// <summary>
        /// 开镇广播
        /// </summary>
        /// <param name="obj"></param>
        private void OnlineTownStart(object obj)
        {
            string broadcastType = (string)obj;
            SendPlayInfo palyinfo = new SendPlayInfo();
            palyinfo.broadcastType = broadcastType;//0：日常  1：应急
            palyinfo.organization_List = SingletonInfo.GetInstance().Organization;
            //Thread.Sleep(2000);
            Generalresponse response = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(palyinfo, "播放");
            if (response.code == 0)
            {
                Thread.Sleep(5000);
                broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                if (broadcastrecordresponse.data.Count > 0)
                {
                    SingletonInfo.GetInstance().TownHttpPlayID = broadcastrecordresponse.data[0].prlId.ToString();
                }
            }
          //  ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);


        }

        /// <summary>
        /// 如果日常开着  先把日常关闭  再开应急
        /// </summary>
        /// <param name="obj"></param>
        private void OnlineSelectedStart(object obj)
        {       
            string broadcastType = (string)obj;

            if (broadcastType == "1" && btn_Daily_Main.Text=="日常停播") //应急
            {
                 //先关日常

                List<string> STOP_ID_List = new List<string>();
                foreach (var item in SingletonInfo.GetInstance().dgvMainData)
                {
                    if (item.checkstate && item.deviceoperate=="1" && item.prEvnType=="日常" )
                    {
                        STOP_ID_List.Add(item.prlId);
                        item.deviceoperate = "0";
                        item.prEvnType = "未播放";
                        item.prlId = "-1";
                    }
                }

                if (STOP_ID_List.Count>0)
                {
                    Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(STOP_ID_List, "停止");
                    Thread.Sleep(5000);
                }
               
            }
          
            SendPlayInfo palyinfo = new SendPlayInfo();
            palyinfo.broadcastType = broadcastType;//0：日常  1：应急
            palyinfo.organization_List = new List<organizationdata>();

            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                if (item.checkstate && item.deviceoperate=="0"&&item.prEvnType=="未播放")
                {
                    palyinfo.organization_List.Add(item.areadata);
                }
            }
            if (palyinfo.organization_List.Count>0)
            {
                Generalresponse response = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(palyinfo, "播放");
                if (response.code == 0)
                {
                    Thread.Sleep(2000);
                    broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                    if (broadcastrecordresponse.data.Count > 0)
                    {
                        foreach (var item in palyinfo.organization_List)
                        {
                            Datagridviewmainitem selected = SingletonInfo.GetInstance().dgvMainData.Find(s => s.areaname.Equals(item.name));

                            broadcastrecorddata receiverecord = broadcastrecordresponse.data.Find(s => s.prAreaName.Equals(item.name));
                            selected.deviceoperate = "1";
                            selected.prEvnType = receiverecord.prEvnType;
                            selected.prlId = receiverecord.prlId.ToString();

                        }
                    }
                }
                ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData,1);
            }
        }

        private void OfflineSelectedStart(object obj)
        {
            #region  TS指令 播发
            string broadcasttype = (string)obj;
            string ebm_class = broadcasttype == "日常" ? "0101" : "0100";
            if (btn_Daily_Main.Text == "日常停播")
            {
                //关闭日常广播

                string IndexItemIDstr = "";
                foreach (var item in SingletonInfo.GetInstance().dgvMainData)
                {
                    if (item.checkstate && item.deviceoperate == "1" && item.IndexItemID != "-1" && item.prEvnType == "日常")
                    {
                        IndexItemIDstr += "," + item.IndexItemID;
                        // item.checkstate = true;
                        item.deviceoperate = "0";
                        item.IndexItemID = "-1";
                        item.prEvnType = "未播放";
                    }
                }
                if (IndexItemIDstr != "")
                {
                    IndexItemIDstr = IndexItemIDstr.Substring(1, IndexItemIDstr.Length - 1);
                    DelEBMIndex2Global(IndexItemIDstr);
                }
            }

            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                if (item.checkstate&&item.deviceoperate=="0"&&item.prEvnType=="未播放")
                {

                    List<string> ResourceList = new List<string>();
                    ResourceList.Add(item.areadata.resource);
                    Dictionary<string, string> dic = TSBroadcastcommand(ResourceList, SingletonInfo.GetInstance().ts_pid, ebm_class);
                    item.IndexItemID = dic[item.areadata.resource];
                    item.prEvnType = broadcasttype;
                    //item.checkstate = true;
                    item.deviceoperate = "1";
                } 
            }
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData,1);
            #endregion
        }

        private void OfflineAllStart(object obj)
        {
        
            //关闭 现有广播
            DelEBMIndex2GlobalAll();


            #region  TS指令 播发
            string broadcasttype = (string)obj;
 

            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                List<string> ResourceList = new List<string>();
                ResourceList.Add(item.areadata.resource);
                Dictionary<string, string> dic = TSBroadcastcommand(ResourceList, SingletonInfo.GetInstance().ts_pid, "0100");
                item.IndexItemID = dic[item.areadata.resource];
                item.prEvnType = broadcasttype;
                item.checkstate = true;
                item.deviceoperate = "1";
            }
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData,1);
            #endregion
        }

        private void OfflineTownStart(object obj)
        {
            //没有与县平台联通的情况下 全开日常广播
            #region  TS指令 播发
            string ebm_class = (string)obj == "日常" ? "0101" : "0100";
            List<string> ResourceList = new List<string>();
            ResourceList.Add(SingletonInfo.GetInstance().Organization[0].resource);
            Dictionary<string, string> dic = TSBroadcastcommand(ResourceList, SingletonInfo.GetInstance().ts_pid, ebm_class);
            SingletonInfo.GetInstance().TownTSItemIndexID = dic[SingletonInfo.GetInstance().Organization[0].resource];
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData,1);

            #endregion
        }

        private void OnlineAllStop()
        {
             List<string> STOP_ID_List=new List<string>();
             STOP_ID_List.Add(SingletonInfo.GetInstance().Interstitial_prlId);
            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                if (item.deviceoperate == "1")
                {
                   // item.checkstate = true;
                    item.deviceoperate = "0";
                    item.prEvnType = "未播放";
                    item.prlId = "-1";
                   
                }
               
            }
            Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(STOP_ID_List, "停止");
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
            
        }

        private void OnlineTownStop()
        {
            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                item.deviceoperate = "0";
                item.prEvnType = "未播放";
                item.prlId = "-1";
            }
            List<string> STOP_ID_List = new List<string>();
            STOP_ID_List.Add(SingletonInfo.GetInstance().TownHttpPlayID);
            Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(STOP_ID_List, "停止");
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
        }

        /// <summary>
        /// 实际把所有在播放的都关闭
        /// </summary>
        private void OnlineSelectedStop()
        {
            List<string> STOP_ID_List = new List<string>();
            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                if (item.deviceoperate=="1"&& item.prEvnType!="未播放")
                {
                    STOP_ID_List.Add(item.prlId);
                    item.deviceoperate = "0";
                    item.prEvnType = "未播放";
                    item.prlId = "-1";
                }
            }
            if (STOP_ID_List.Count>0)
            {
                Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(STOP_ID_List, "停止");
                ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
            }
        }

        private void OfflineAllStop()
        {
            string IndexItemIDstr = "";
            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                if (item.deviceoperate=="1")
                {
                    IndexItemIDstr += "," + item.IndexItemID;
                    //  item.checkstate = true;
                    item.deviceoperate = "0";
                    item.IndexItemID = "-1";
                    item.prEvnType = "未播放";
                }
            }
            IndexItemIDstr = IndexItemIDstr.Substring(1, IndexItemIDstr.Length-1);
            DelEBMIndex2Global(IndexItemIDstr);
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
        }

        private void OfflineTownStop()
        {
            string IndexItemIDstr = "";

            IndexItemIDstr += "," + SingletonInfo.GetInstance().TownTSItemIndexID;
           
            IndexItemIDstr = IndexItemIDstr.Substring(1, IndexItemIDstr.Length - 1);
            DelEBMIndex2Global(IndexItemIDstr);
            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                item.deviceoperate = "0";
                item.IndexItemID = "-1";
                item.prEvnType = "未播放";
            }
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
        }

        private void OfflineSelectedStop()
        {
            //string IndexItemIDstr = "";
            //foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            //{
            //    if (item.checkstate&&item.deviceoperate=="1"&&item.IndexItemID!="-1"&&item.prEvnType!="未播放")
            //    {
            //        IndexItemIDstr += "," + item.IndexItemID;
            //        item.checkstate = true;
            //        item.deviceoperate = "0";
            //        item.IndexItemID = "-1";
            //        item.prEvnType = "未播放";
            //    }
            //}
            //if (IndexItemIDstr != "")
            //{
            //    IndexItemIDstr = IndexItemIDstr.Substring(1, IndexItemIDstr.Length - 1);
            //    DelEBMIndex2Global(IndexItemIDstr);
            //    ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
            //}


            foreach (var item in SingletonInfo.GetInstance().dgvMainData)
            {
                item.deviceoperate = "0";
                item.IndexItemID = "-1";
                item.prEvnType = "未播放";

            }
            DelEBMIndex2GlobalAll();
            ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
        }

        private void btn_Daily_Main_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinDataGridView_Main.Visible)
                {
                    if (btn_Daily_Main.Text == "日常广播")
                    {
                        //发开播
                          MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定开启选中日常广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                            
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //如果离线模式优先的情况下  新增于20190109
                                    OfflineSelectedStart("日常");
                                }
                                else
                                {
                                    //正常模式下
                                    OnlineSelectedStart("0");
                                }
                            }
                            else
                            { 
                                //不在线情况
                                OfflineSelectedStart("日常");
                               
                            }
                            btn_Daily_Main.Text = "日常停播";
                            btn_Daily_Main.BaseColor = System.Drawing.Color.Lime;
                            btn_Emergency_Main.Enabled = false;
                            pictureBox_checkbox.Enabled = false;

                        }
                    }
                    else
                    { 
                         //发停播    与德芯商量后  采取全部关闭的处理
                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定关闭所有日常广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                       
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况下
                                    OfflineSelectedStop();
                                }
                                else
                                {
                                    //正常情况
                                    OnlineSelectedStop();
                                }
                            }
                            else
                            {
                                //不在线情况
                                OfflineSelectedStop();
                            }

                            btn_Daily_Main.Text = "日常广播";
                            btn_Daily_Main.BaseColor = System.Drawing.Color.DarkGreen;
                            btn_Emergency_Main.Enabled = true;
                            pictureBox_checkbox.Enabled = true;
                        }
                    }
                }
                else
                {
                    if (btn_Daily_Main.Text == "日常广播")
                    {
                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定开启全部日常广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                            btn_Emergency_Main.Enabled = false;
                            //开
                            btn_Daily_Main.Text = "日常停播";
                            btn_Daily_Main.BaseColor = System.Drawing.Color.Lime;
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况
                                    Thread thread = new Thread(new ParameterizedThreadStart(OfflineTownStart));
                                    thread.IsBackground = true;
                                    thread.Start("日常");
                                }
                                else
                                {
                                    //正常情况
                                    Thread thread = new Thread(new ParameterizedThreadStart(OnlineTownStart));
                                    thread.IsBackground = true;
                                    thread.Start("0");
                                }
                            }
                            else
                            {
                                //不在线情况
                                Thread thread = new Thread(new ParameterizedThreadStart(OfflineTownStart));
                                thread.IsBackground = true;
                                thread.Start("日常");
                            }
                            btn_Organization.Enabled = false;
                        }
                       
                    }
                    else
                    {
                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定关闭所有日常广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                            btn_Emergency_Main.Enabled = true;
                            //关
                            btn_Daily_Main.Text = "日常广播";
                            btn_Daily_Main.BaseColor = System.Drawing.Color.DarkGreen;
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况下 
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况
                                    Thread thread = new Thread(new ThreadStart(OfflineTownStop));
                                    thread.IsBackground = true;
                                    thread.Start();
                                }
                                else
                                {
                                    //正常情况
                                    Thread thread = new Thread(new ThreadStart(OnlineTownStop));
                                    thread.IsBackground = true;
                                    thread.Start();
                                }
                              
                            }
                            else
                            {
                                //不在线情况
                                Thread thread = new Thread(new ThreadStart(OfflineTownStop));
                                thread.IsBackground = true;
                                thread.Start();
                            }
                            btn_Organization.Enabled = true;
                        }
                       
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void btn_Emergency_Main_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinDataGridView_Main.Visible)
                {
                    if (btn_Emergency_Main.Text == "应急广播")
                    {
                        //发开播
                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定开启选中应急广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                            btn_Emergency_Main.Text = "应急停播";
                            btn_Emergency_Main.BaseColor = System.Drawing.Color.Red;
                            btn_Daily_Main.Enabled = false;
                            pictureBox_checkbox.Enabled = false;
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况下
                                    OfflineSelectedStart("应急");
                                }
                                else
                                {
                                    //正常情况
                                    OnlineSelectedStart("1");
                                }
                            }
                            else
                            {
                                //不在线情况
                                OfflineSelectedStart("应急");
                            }
                        }
                    }
                    else
                    {
                        //发停播      与德芯商量后  采取全部关闭的处理
                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定关闭所有应急广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                            btn_Emergency_Main.Text = "应急广播";
                            btn_Emergency_Main.BaseColor = System.Drawing.Color.Maroon;
                            btn_Daily_Main.Enabled = true;
                            pictureBox_checkbox.Enabled = true;
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况
                                    OfflineSelectedStop();
                                }
                                else
                                {
                                    //正常情况
                                    OnlineSelectedStop();
                                }
                            }
                            else
                            {
                                //不在线情况
                                OfflineSelectedStop();
                            }
                        }
                    }
                }
                else
                {
                    if (btn_Emergency_Main.Text == "应急广播")
                    {

                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定开启全部应急广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {

                            btn_Daily_Main.Enabled = false;
                            //开
                            btn_Emergency_Main.Text = "应急停播";
                            btn_Emergency_Main.BaseColor = System.Drawing.Color.Red;
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况
                                    Thread thread = new Thread(new ParameterizedThreadStart(OfflineTownStart));
                                    thread.IsBackground = true;
                                    thread.Start("应急");
                                }
                                else
                                {
                                    //正常情况
                                    Thread thread = new Thread(new ParameterizedThreadStart(OnlineTownStart));
                                    thread.IsBackground = true;
                                    thread.Start("1");
                                }
                            }
                            else
                            {
                                //不在线情况
                                Thread thread = new Thread(new ParameterizedThreadStart(OfflineTownStart));
                                thread.IsBackground = true;
                                thread.Start("应急");
                            }

                            btn_Organization.Enabled = false;
                        }
                    }
                    else
                    {
                        MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定关闭所有应急广播？" } };
                        MessageShowDlg.ShowDialog();
                        if (MessageShowDlg.IsSure)
                        {
                            btn_Daily_Main.Enabled = true;
                            //关
                            btn_Emergency_Main.Text = "应急广播";
                            btn_Emergency_Main.BaseColor = System.Drawing.Color.Maroon;
                            if (SingletonInfo.GetInstance().loginstatus)
                            {
                                //在线情况
                                if (!SingletonInfo.GetInstance().SendCommandMode)
                                {
                                    //离线模式优先的情况
                                    Thread thread = new Thread(new ThreadStart(OfflineTownStop));
                                    thread.IsBackground = true;
                                    thread.Start();
                                }
                                else
                                {
                                    //正常情况
                                    Thread thread = new Thread(new ThreadStart(OnlineTownStop));
                                    thread.IsBackground = true;
                                    thread.Start();
                                }
                            }
                            else
                            {
                                //不在线情况
                                Thread thread = new Thread(new ThreadStart(OfflineTownStop));
                                thread.IsBackground = true;
                                thread.Start();
                            }
                            btn_Organization.Enabled = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void pictureBox_checkbox_Click(object sender, EventArgs e)
        {
            try
            {
                if ((bool)skinDataGridView_Main.Tag)
                {
                       //取消
                    foreach (var Datagridviewmainitem in SingletonInfo.GetInstance().dgvMainData)
                    {
                        Datagridviewmainitem.checkstate = false;
                    }
                    skinDataGridView_Main.Tag = (bool)false;
                    pictureBox_checkbox.BackgroundImage = imageList1.Images[1];
                }
                else
                { 
                      //勾选
                    foreach (var Datagridviewmainitem in SingletonInfo.GetInstance().dgvMainData)
                    {
                        Datagridviewmainitem.checkstate = true;
                    }
                    skinDataGridView_Main.Tag = (bool)true;
                    pictureBox_checkbox.BackgroundImage = imageList1.Images[0];
                }

                ShowskinDataGridView_Main(SingletonInfo.GetInstance().dgvMainData);
            }
            catch (Exception)
            {
                
                throw;
            }
        }


        /// <summary>
        /// 锁定界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox_Lock_Click(object sender, EventArgs e)
        {
            try
            {
                FmLogin fmLogin = new FmLogin();
                SingletonInfo.GetInstance().lockstatus = true;
                fmLogin.Show();
            }
            catch (Exception)
            {
                throw;
            }
        }
        

        private void pictureBox_online_DoubleClick(object sender, EventArgs e)
        {
            MessageShowDlg = new MessageShowForm { label1 = { Text = @"确定关闭？" } };
            MessageShowDlg.ShowDialog();
            if (MessageShowDlg.IsSure)
            {
                ini.WriteValue("EBM", "ebm_id_behind", SingletonInfo.GetInstance().ebm_id_behind);
                ini.WriteValue("EBM", "ebm_id_count", SingletonInfo.GetInstance().ebm_id_count.ToString());
                ini.WriteValue("EBM", "input_channel_id", SingletonInfo.GetInstance().input_channel_id);
                ini.WriteValue("EBM", "IndexItemID", SingletonInfo.GetInstance().IndexItemID.ToString());


                TableDataHelper.WriteTable(Enums.TableType.WhiteList, SingletonInfo.GetInstance().WhiteListRecordList);
                if (EbmStream != null && IsStartStream)
                {
                    EbmStream.StopStreaming();
                    IsStartStream = false;
                }
                ShutdownWin();
                Close();
            }
            GC.Collect();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (SingletonInfo.GetInstance().UpgradeFlag == "1")
            {
                timer2.Enabled = false;
                upgradeForm = new UpgradeForm { label1 = { Text = @"软件版本有更新，是否升级？" } };
                upgradeForm.Show();
            }
        }

        private void skinButton2_Click(object sender, EventArgs e)
        {
            try
            {

                if (!panel_map.Visible)
                {
                    panel_map.Location = new System.Drawing.Point(453, 214);
                    panel_map.Size = new System.Drawing.Size(1030, 589);
                    panel_map.Visible = true;

                    #region  调用火狐浏览器
                    Browser = new Gecko.GeckoWebBrowser();
                    Browser.Dock = DockStyle.Fill;
                    panel_map.Controls.Add(Browser);
                    panel_map.BringToFront();
                    LogHelper.WriteLog(typeof(MainForm), "ceeaditcode：" + SingletonInfo.GetInstance().creditCode);
                    testUrl = SingletonInfo.GetInstance().HttpServer + "/platform/eMap/"+ SingletonInfo.GetInstance().creditCode + "/monitor.htm";
                    Browser.Navigate(testUrl);
                    #endregion
                }
                else
                {
                    panel_map.Controls.Remove(Browser);
                    panel_map.Location = new System.Drawing.Point(294, 991);
                    panel_map.Size = new System.Drawing.Size(74, 81);
                    panel_map.Visible = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "运行总览打开失败："+ex.Message);
            }
        }
    }
}
