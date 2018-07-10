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

namespace TownsEBMSystem
{
    public partial class MainForm : Form
    {

        AutoSizeFormClass asc = new AutoSizeFormClass();
        public static IniFiles ini;
        private readonly string xulrunnerPath = Application.StartupPath + "/xulrunner";
        private  string testUrl = "http://192.168.21.105/";
        private Gecko.GeckoWebBrowser Browser;
        public string Pictxt;
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


        public EBMStream EbMStream
        {
            get { return EbmStream; }
            set { EbmStream = value; }
        }

        public static Calcle calcel;
        private Object Gtoken = null; //用于锁住

        public MainForm()
        {
           
            InitializeComponent();
            this.ShowInTaskbar = false;
            Load += MainForm_Load;
            Xpcom.Initialize(xulrunnerPath);
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
            dataDealHelper = new DataDealHelper();
            dataHelper = new DataHelper();
            ProcessBegin();
        }



        private void MainForm_Load(object sender, EventArgs e)
        {
            switchImage1.Font = new Font("微软雅黑", 11);
            //  switchImage1.ForeColor = Color.Transparent;
            string[] jpgFiles = Directory.GetFiles(System.Windows.Forms.Application.StartupPath + "\\image", "*.jpg");

            foreach (var item in jpgFiles)
            {
                switchImage1.AddImageItems(item, Pictxt, Color.Transparent);
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


        public void NetErrorDeal()
        {
      
        }

        private void InitConfig()
        {
            try
            {
                SingletonInfo.GetInstance().username = ini.ReadValue("LoginInfo", "username");
                SingletonInfo.GetInstance().password = ini.ReadValue("LoginInfo", "password");
                SingletonInfo.GetInstance().licenseCode = ini.ReadValue("LoginInfo", "licenseCode");
                Pictxt = "测试显示数据，联系人：杭州图南电子股份有限公司";
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
               // InitEBStream();
                if (EbmStream != null && isInitStream && !IsStartStream)
                {
                    //发送数据
                    EbmStream.StartStreaming();
                    IsStartStream = true;
                }

                SingletonInfo.GetInstance().IsStartSend = true;
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
                JObject jo = TableDataHelper.ReadConfig();
                if (jo != null)
                {
                    EbmStream.ElementaryPid = Convert.ToInt32(jo["ElementaryPid"].ToString());
                    EbmStream.Stream_id = Convert.ToInt32(jo["Stream_id"].ToString());
                    EbmStream.Program_id = Convert.ToInt32(jo["Program_id"].ToString());
                    EbmStream.PMT_Pid = Convert.ToInt32(jo["PMT_Pid"].ToString());
                    EbmStream.Section_length = Convert.ToInt32(jo["Section_length"].ToString());
                    EbmStream.sDestSockAddress = jo["sDestSockAddress"].ToString();
                    EbmStream.sLocalSockAddress = jo["sLocalSockAddress"].ToString();
                    EbmStream.Stream_BitRate = Convert.ToInt32(jo["Stream_BitRate"].ToString());
                }
                InitStreamTableNew();
                return true;
            }
            catch (Exception ex)
            {
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
                Generalresponse heartbeatresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(null, "心跳");
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


        private void Showdgv_broadcastrecord(List<broadcastrecorddata> Listdata)
        {
            this.Invoke(new Action(() =>
            {
                dgv_broadcastrecord.DataSource = null;
                if (Listdata.Count > 0)
                {
                    dgv_broadcastrecord.DataSource = Listdata;
                }
            }));
        }


        private void Showdgv_broadcastrecord(List<PlayRecord_tcp_ts> Listdata)
        {
            this.Invoke(new Action(() =>
            {

                if (Listdata.Count>0)
                {
                    dgv_broadcastrecord.DataSource = null;
                    if (Listdata.Count > 0)
                    {
                        dgv_broadcastrecord.DataSource = Listdata;
                    }
                }
              
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
                                dgv_broadcastrecord.DataSource = null;
                                dgv_broadcastrecord.DataSource = Listdata;

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
                            OnorOFFResponse stopresponse = TCPBroadcastcommand(IDList,"2");
                            if (stopresponse.result_code == 0)
                            {

                                IndexItemIDstr = IndexItemIDstr.Substring(0, IndexItemIDstr.Length - 1);

                                DelEBMIndex2Global(IndexItemIDstr);
                                List<PlayRecord_tcp_ts> Listdata = (List<PlayRecord_tcp_ts>)dgv_broadcastrecord.DataSource;
                                foreach (PlayRecord_tcp_ts item in IDList)
                                {
                                    Listdata.Remove(item);
                                }
                                dgv_broadcastrecord.DataSource = null;
                                dgv_broadcastrecord.DataSource = Listdata;

                            }

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

                            }
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
                LogHelper.WriteLog(typeof(MainForm), ex);
            }
            finally
            {
                pictureBox_Login.Visible = false;
                if (loginflag)
                {
                    pictureBox_online.Visible = true;
                }
                else
                {
                    pictureBox_offline.Visible = true;
                    var jo = TableDataHelper.ReadTable(Enums.TableType.Organization);
                    if (jo != null)
                    {
                        SingletonInfo.GetInstance().Organization = JsonConvert.DeserializeObject<List<organizationdata>>(jo["0"].ToString());
                    }
                    ShowtreeViewOrganization(SingletonInfo.GetInstance().Organization);

                }

            }
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


        private GeneralResponse TCPRebackPeriod(List<organizationdata> organization_List,string cycle)
        {
            RebackPeriod tt = new RebackPeriod();

            tt.reback_cycle = cycle;
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            foreach (var item in organization_List)
            {
                tt.resource_codeList.Add(item.resource);
            }
            GeneralResponse resopnse = (GeneralResponse)SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt,0x0B);
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





        private List<string> TSBroadcastcommand(List<organizationdata> organization_List,string broadcasttype,string pid,string ebm_class)
        {
            List<string> IndexItemIDList = new List<string>();

            foreach (var item in organization_List)
            {
                EBMIndexTmp tmp = new EBMIndexTmp();

                SingletonInfo.GetInstance().IndexItemID += 1;
                tmp.IndexItemID = SingletonInfo.GetInstance().IndexItemID.ToString();
                IndexItemIDList.Add(tmp.IndexItemID);
                tmp.S_EBM_class = ebm_class;//


                string ebm_id_tmp= SingletonInfo.GetInstance().ebm_id;//

                ebm_id_tmp = "5451423000000010301010120180706";

                tmp.S_EBM_id = ebm_id_tmp.Substring(2, ebm_id_tmp.Length - 2);

                tmp.S_EBM_start_time = SingletonInfo.GetInstance().starttime;
                tmp.S_EBM_end_time = SingletonInfo.GetInstance().endtime;
                tmp.S_EBM_level = "2";//
                tmp.S_EBM_original_network_id = "1";
                tmp.S_EBM_start_time = SingletonInfo.GetInstance().starttime;//
                tmp.S_EBM_type = "00000";//

                string List_EBM_resource_code = "";
                foreach (organizationdata org in organization_List)
                {
                    List_EBM_resource_code +=","+ org.resource;
                }
                List_EBM_resource_code = List_EBM_resource_code.Substring(1, List_EBM_resource_code.Length-1);
                tmp.List_EBM_resource_code = List_EBM_resource_code;

                tmp.BL_details_channel_indicate = "true";
                tmp.S_details_channel_transport_stream_id = SingletonInfo.GetInstance().S_details_channel_transport_stream_id;
                tmp.S_details_channel_program_number = SingletonInfo.GetInstance().S_details_channel_program_number;
                tmp.S_details_channel_PCR_PID= SingletonInfo.GetInstance().S_details_channel_PCR_PID;
                tmp.List_ProgramStreamInfo = new List<ProgramStreamInfotmp>();
                ProgramStreamInfotmp pp = new ProgramStreamInfotmp();
                pp.B_stream_type = "4";
                pp.S_elementary_PID = SingletonInfo.GetInstance().pid;
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
                        #region 前端协议播发
                        OnorOFFResponse res = TCPBroadcastcommand(organization_List, "1", "4");//"1"表示开播

                        SingletonInfo.GetInstance().ts_pid = res.result_desc;
                        #endregion

                        #region  TS指令 播发
                        List<string> IndexItemIDList = TSBroadcastcommand(organization_List, "应急", SingletonInfo.GetInstance().ts_pid, "0100");//此时的res.result_desc中存放的是pid数据

                        List<PlayRecord_tcp_ts> datasource = new List<PlayRecord_tcp_ts>();
                        for (int i = 0; i < organization_List.Count; i++)
                        {
                            PlayRecord_tcp_ts pp = new PlayRecord_tcp_ts();
                            pp.IndexItemID = IndexItemIDList[i];
                            pp.prAreaName = organization_List[i].name;
                            pp.prEvnType = "应急";
                            pp.resource_code = organization_List[i].resource;
                            datasource.Add(pp);
                        }


                          Showdgv_broadcastrecord(datasource); 
                        #endregion
                        if (res.result_code == 1)
                        {
                            MessageBox.Show("播放失败：");
                        }

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
                        #region 前端协议播发
                        OnorOFFResponse res = TCPBroadcastcommand(organization_List, "1","5");//"1"表示开播

                        SingletonInfo.GetInstance().ts_pid = res.result_desc;
                        #endregion

                        #region  TS指令 播发
                        TSBroadcastcommand(organization_List, "日常", SingletonInfo.GetInstance().ts_pid,"0101");//此时的res.result_desc中存放的是pid数据
                        #endregion
                        if (res.result_code == 1)
                        {
                            MessageBox.Show("播放失败：");
                        }

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

                skinTabControl_Organization.Location = new System.Drawing.Point(436, 179);
                skinTabControl_Organization.Size = new System.Drawing.Size(1038, 659);
                skinTabControl_Organization.Visible = true;
                skinTabControl_parameterset.Visible = false;
                btn_Home1.Visible = true;



                skinButton1.Text = "话筒";
                skinButton3.Text = "U盘";
                skinButton4.Text = "DVB-C";
                skinButton5.Text = "DTMB";

                skinButton9.Text = "线路一";
                skinButton8.Text = "线路二";
                skinButton7.Text = "调频一";
                skinButton6.Text = "调频二";


                if (SingletonInfo.GetInstance().loginstatus)
                {
                    Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(null, "地图");

                    Thread.Sleep(500);
                    testUrl = SingletonInfo.GetInstance().HttpServer + stopresponse.data;
                    #region  调用火狐浏览器
                    Browser = new Gecko.GeckoWebBrowser();
                    Browser.Dock = DockStyle.Fill;
                    skinTabControl_Organization.TabPages["skinTabPage3"].Controls.Add(Browser);
                    Browser.Navigate(testUrl);
                    #endregion
                }

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

            skinButton1.Text = "电话授权";
            skinButton3.Text = "Tuner设置";
            skinButton4.Text = "广播设置";
            skinButton5.Text = "定时广播";


            skinButton9.Text = "调频设置";
            skinButton8.Text = "网络设置";
            skinButton7.Text = "U盘播放";
            skinButton6.Text = "系统设置";
        }

        private void btn_Home1_Click(object sender, EventArgs e)
        {
            btn_Home1.Visible = false;
            skinTabControl_Organization.Visible = false;
            skinTabControl_parameterset.Visible = false;
            skinButton1.Text = "话筒";
            skinButton3.Text = "U盘";
            skinButton4.Text = "DVB-C";
            skinButton5.Text = "DTMB";

            skinButton9.Text = "线路一";
            skinButton8.Text = "线路二";
            skinButton7.Text = "调频一";
            skinButton6.Text = "调频二";
        }

        private void pictureBox_offline_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定关闭？", "确定关闭", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                ini.WriteValue("EBM", "ebm_id_behind", SingletonInfo.GetInstance().ebm_id_behind);
                ini.WriteValue("EBM", "ebm_id_count", SingletonInfo.GetInstance().ebm_id_count.ToString());
                ini.WriteValue("EBM", "input_channel_id", SingletonInfo.GetInstance().input_channel_id);
                ini.WriteValue("EBM", "IndexItemID", SingletonInfo.GetInstance().IndexItemID.ToString());
                if (EbmStream != null && IsStartStream)
                {
                    EbmStream.StopStreaming();
                    IsStartStream = false;
                }
                Close();
            }
        }

        private void pictureBox_online_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定关闭？", "确定关闭", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                ini.WriteValue("EBM", "ebm_id_behind", SingletonInfo.GetInstance().ebm_id_behind);
                ini.WriteValue("EBM", "ebm_id_count", SingletonInfo.GetInstance().ebm_id_count.ToString());
                ini.WriteValue("EBM", "input_channel_id", SingletonInfo.GetInstance().input_channel_id);
                ini.WriteValue("EBM", "IndexItemID", SingletonInfo.GetInstance().IndexItemID.ToString());

                if (EbmStream != null && IsStartStream)
                {
                    EbmStream.StopStreaming();
                    IsStartStream = false;
                }

                Close();
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
                if (skinButton1.Text == "话筒")
                {
                    OnorOFFResponse res= SwitchChannel(1);//话筒目前定为1
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        
                        MessageBox.Show("切换到话筒失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "1";
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

        private void skinButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton3.Text == "U盘")
                {
                    OnorOFFResponse res = SwitchChannel(2);//U盘目前定为2
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                      
                        MessageBox.Show("切换到U盘失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "2";
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

        private void skinButton4_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton4.Text == "DVB-C")
                {
                    OnorOFFResponse res = SwitchChannel(3);//DVB-C目前定为3
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到DVB-C失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "3";
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
                    OnorOFFResponse res = SwitchChannel(4);//DTMB目前定为4
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到DVB-C失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "4";
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
                if (skinButton9.Text == "线路一")
                {
                    OnorOFFResponse res = SwitchChannel(5);//线路一目前定为5
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到线路一失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "5";
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

        private void skinButton8_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton8.Text == "线路二")
                {
                    OnorOFFResponse res = SwitchChannel(6);//线路二目前定为6
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到线路二失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "6";
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

        private void skinButton7_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton7.Text == "调频一")
                {
                    OnorOFFResponse res = SwitchChannel(7);//调频一目前定为7
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到调频一失败");
                    }
                    else
                    {

                        SingletonInfo.GetInstance().input_channel_id = "7";
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

        private void skinButton6_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinButton6.Text == "调频二")
                {
                    OnorOFFResponse res = SwitchChannel(8);//调频二目前定为8
                    if (res.result_code == 1)  //0代表成功1代表失败
                    {
                        MessageBox.Show("切换到调频二失败");
                    }
                    else
                    {
                        SingletonInfo.GetInstance().input_channel_id = "8";
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
            }
            UpdateDataTextNew((object)1);
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
                    EbmStream.EB_Index_Table = GetEBIndexTable(ref EB_Index_Table) ? EB_Index_Table : null;
                    EbMStream.Initialization();
                }
                UpdateDataTextNew((object)1);
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

        private void button1_Click(object sender, EventArgs e)
        {
            List<organizationdata> organization_List = new List<organizationdata>();
            organization_List = CheckedNodes(treeViewOrganization.TopNode, organization_List);
            GeneralResponse res = TCPRebackPeriod(organization_List, "60");
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<organizationdata> organization_List = new List<organizationdata>();
            organization_List = CheckedNodes(treeViewOrganization.TopNode, organization_List);
            GeneralResponse res = TCPSwitchAmplifier(organization_List, "1");//1表示关闭喇叭  2表示打开喇叭
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<organizationdata> organization_List = new List<organizationdata>();
            organization_List = CheckedNodes(treeViewOrganization.TopNode, organization_List);

            WhiteListUpdate senddata = new WhiteListUpdate();
         

            senddata.white_list = new List<WhiteListInfo>();


            WhiteListInfo pp = new WhiteListInfo();

            pp.oper_type = "1"; //操纵类型 1：增加 2：修改 3：删除
            pp.phone_number = "15158108008";
            pp.user_name = "老司机";
            pp.permission_type = "3";//许可类型1:代表短信;2:代表电话;3代表短信和电话
            pp.permission_area_codeList = new List<string>();
            foreach (var item in organization_List)
            {
                pp.permission_area_codeList.Add(item.gb_code);
            }

            senddata.white_list.Add(pp);
           GeneralResponse res = TCPWhiteListUpdate(senddata);
        }
    }
}
