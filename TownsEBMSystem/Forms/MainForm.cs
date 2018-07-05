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
  



        public MainForm()
        {
           
            InitializeComponent();
            this.ShowInTaskbar = false;
            Load += MainForm_Load;
            Xpcom.Initialize(xulrunnerPath);
            CheckIniConfig();
            InitConfig();
            InitTCPServer();
            //记录初始尺寸
            //asc.controllInitializeSize(this);
            //WindowState= FormWindowState.Maximized;
            //任务栏不显示
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

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "获取配置文件失败");
            }
 
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
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //自适应
           // asc.controlAutoSize(this);
        }

        private void skinButton2_Click(object sender, EventArgs e)
        {

            ini.WriteValue("EBM", "ebm_id_behind", SingletonInfo.GetInstance().ebm_id_behind);
            ini.WriteValue("EBM", "ebm_id_count", SingletonInfo.GetInstance().ebm_id_count.ToString());
            Close();
        }

        /// <summary>
        /// 登录县平台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginBtn_Click(object sender, EventArgs e)
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
                if (loginflag)
                {
                  
                    LoginBtn.BaseColor= Color.Green;
                  
                }
                else
                {
                    LoginBtn.BaseColor = Color.Red;


                    var jo = TableDataHelper.ReadTable(Enums.TableType.Organization);
                    if (jo != null)
                    {
                        SingletonInfo.GetInstance().Organization = JsonConvert.DeserializeObject<List<organizationdata>>(jo["0"].ToString());
                    }
                    ShowtreeViewOrganization(SingletonInfo.GetInstance().Organization);

                }

            }
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
        
        private void skinButton14_Click(object sender, EventArgs e)
        {

              testUrl = "http://192.168.4.87/homeAction_index.action#googleMapAction_loadMap";
            SingletonInfo.GetInstance().post.PostCommnand(null, "显示地图");
            #region  调用火狐浏览器
            Browser = new Gecko.GeckoWebBrowser();
            Browser.Dock = DockStyle.Fill;
          //  panel2.Controls.Add(Browser);
            Browser.Navigate(testUrl);
          
            #endregion
        }

        private void skinButton15_Click(object sender, EventArgs e)
        {
            Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(null, "地图");

            testUrl = "http://192.168.4.87:8080/"+ stopresponse.data;
          
            #region  调用火狐浏览器
            Browser = new Gecko.GeckoWebBrowser();
            Browser.Dock = DockStyle.Fill;
            panel_map.Controls.Add(Browser);
            panel_map.Visible = true;
            panel_map.BringToFront();
            Browser.Navigate(testUrl);
            #endregion
        }

        private void skinButton16_Click(object sender, EventArgs e)
        {
            organizationInfo reback = (organizationInfo)SingletonInfo.GetInstance().post.PostCommnand(null, "获取区域");

            if (reback!=null)
            {
                SingletonInfo.GetInstance().Organization = reback.data;

                foreach (organizationdata item in SingletonInfo.GetInstance().Organization)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization.Nodes.Add(node);
                }
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
        private void skinButton17_Click(object sender, EventArgs e)
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string data = "{\"code\":0,\"extend\":{},\"msg\":\"操作成功\",\"data\":[{ \"children\":[{ \"children\": [],\"gb_code\": \"061245140210420500\", \"id\": 5, \"name\": \"光坡村\"}], \"gb_code\": \"061245140210400000\", \"id\": 4, \"name\": \"左州镇\"}]}";
            organizationInfo OrgInfo = Serializer.Deserialize<organizationInfo>(data);
            SingletonInfo.GetInstance().Organization = OrgInfo.data;


            #region 保存区域信息 
            TableDataHelper.WriteTable(Enums.TableType.Organization, OrgInfo.data);
            #endregion
            if (OrgInfo!=null)
            {
                foreach (organizationdata item in SingletonInfo.GetInstance().Organization)
                {
                    TreeNode node = new TreeNode();
                    node.Text = item.name;
                    node.Tag = item;
                    subnode(node, item.children);
                    treeViewOrganization.Nodes.Add(node);
                }
            }

        }

        private void btn_play_Click(object sender, EventArgs e)
        {
            try
            {
                //if (treeViewOrganization.SelectedNode == null)
                //{
                //    return;
                //}
                //else
                {
                    List<string> organization_id_List = new List<string>();
                    organization_id_List = CheckedNodes(treeViewOrganization.TopNode, organization_id_List);
                    if (organization_id_List.Count>0)
                    {
                        Generalresponse response= (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(organization_id_List, "播放");
                        if (response.code == 0)
                        {
                            broadcastrecord broadcastrecordresponse = (broadcastrecord)SingletonInfo.GetInstance().post.PostCommnand(null, "直播列表");
                            Showdgv_broadcastrecord(broadcastrecordresponse.data);

                        }
                        else
                        {
                            MessageBox.Show("播放失败："+ response.msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog(typeof(MainForm), "播放失败");
            }
        }


        private void Showdgv_broadcastrecord(List<broadcastrecorddata> Listdata)
        {
            this.Invoke(new Action(() =>
            {
                dgv_broadcastrecord.Rows.Clear();
                if (Listdata.Count > 0)
                {
                    dgv_broadcastrecord.DataSource = Listdata;
                }
            }));
        }

        /// <summary>
        /// 遍历树
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="checkednodes"></param>
        /// <returns></returns>
        public List<string> CheckedNodes(TreeNode parent, List<string> checkednodes)
        {
            TreeNode node = parent;
            if (node != null)
            {
                if (node.Checked == true && node.FirstNode == null)
                    checkednodes.Add(((organizationdata)node.Tag).id.ToString());

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

        private void skinButton13_Click(object sender, EventArgs e)
        {
            string tt = "{\"code\":0,\"data\":[{\"prAreaName\":\"左州镇\",\"prEvnSource\":\"客户端\",\"prEvnType\":\"日常\",\"prStarttime\":\"2018-06-03 15:27:44\",\"prlId\":1162,\"programName\":\"镇主节目\",\"userName\":\"test\"}],\"extend\":{},\"msg\":\"操作成功\"}";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            broadcastrecord pp= Serializer.Deserialize<broadcastrecord>(tt);

            Showdgv_broadcastrecord(pp.data);

        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_broadcastrecord.Rows.Count>0)
                {
                    List<string> IDList = new List<string>();
                    for (int i = 0; i < dgv_broadcastrecord.Rows.Count; i++)
                    {
                        if ((bool)dgv_broadcastrecord.Rows[i].Cells[0].EditedFormattedValue == true)
                        {
                            IDList.Add(dgv_broadcastrecord.Rows[i].Cells["prlId"].Value.ToString());
                        }
                    }
                    if (IDList.Count>0)
                    {
                        Generalresponse stopresponse = (Generalresponse)SingletonInfo.GetInstance().post.PostCommnand(IDList, "停止");
                        if (stopresponse.code==0)
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
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "停止播放失败");
            }
        }

        private void skinButton12_Click(object sender, EventArgs e)
        {
            try
            {
                string pp = "49 00 01 04 01 00 00 00 38 F4 34 15 23 00 00 00 01 03 01 01 01 20 18 04 22 00 01 01 05 30 30 30 30 30 01 5A DC 5F 78 5A DC 6B 30 35 01 01 0C F6 34 15 23 10 00 00 03 14 01 04 00 00 01 01 01 00 00 00 4A 00 04 C0 B8 00 00 00 00 00 42 AD BF 36 9D E6 DF EC 4D E9 FF 40 AB 6D 7E E4 5C F8 E5 5D 08 17 03 D4 E0 F3 BF 7A 93 08 1C 22 EF 39 2B C6 04 06 24 F3 11 E5 49 9E 5F FB 84 90 47 87 10 49 37 88 64 5B B9 44 7B 94 CD 85 B3 D1 AA A8 BA 44 0E";
                byte[] dd = SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(pp);

                if (dd.Length>0)
                {

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void skinButton18_Click(object sender, EventArgs e)
        {
            OnorOFFBroadcast tt = new OnorOFFBroadcast();
            tt.ebm_class = "4";
            tt.ebm_id = SingletonInfo.GetInstance().tcpsend.CreateEBM_ID();
            tt.ebm_level = "2";
            tt.ebm_type = "00000";
            tt.end_time = DateTime.Now.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss");
            tt.start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            tt.power_switch = "1";
            tt.volume = "50";
            tt.resource_code_type = "1";
            tt.resource_codeList = new List<string>();
            // tt.resource_codeList.Add("061245140210420500");//   广西资源码18位的   061245140210420500    0612+12位区域码+00
            tt.resource_codeList.Add("061245140210420501");
            tt.input_channel_id =1;//暂时用1
            tt.OutPut_Channel_IdList = new List<int>();
            tt.OutPut_Channel_IdList.Add(2);  //没有的数据长度占用为0

            SingletonInfo.GetInstance().tcpsend.SendTCPCommnand(tt,0x04);

        }
    }
}
