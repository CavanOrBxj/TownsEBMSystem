using System.Threading;
using System.Collections.Generic;
using System.Data;

namespace TownsEBMSystem
{
    public class SingletonInfo
    {
        private static SingletonInfo _singleton;
        public HttpHelper post;
        public TcpHelper tcpsend;
        public string username;
        public string password;
        public string licenseCode;
        public string HttpServer;
        public string creditCode;
        public List<organizationdata> Organization;
        public string pid;
        public bool loginstatus;//表示系统有没有登录到县平台

        public int SendfaileTime;//心跳发送失败次数 

        public string SendTCPdataIP;
        public int SendTCPdataPORT;
        public string ebm_id_front;
        public string ebm_id_behind;
        public int ebm_id_count;
        private SingletonInfo()                                                                 
        {
         
            post = new HttpHelper();
            tcpsend = new TcpHelper();
            username = "";
            password = "";
            licenseCode = "";
            HttpServer = "";
            creditCode = "";
            Organization = new List<organizationdata>();
            pid = "";
            loginstatus = false;
            SendTCPdataIP = "";
            SendTCPdataPORT = 0;

            ebm_id_front = "";
            ebm_id_behind = "";
            ebm_id_count = 0;

        }
        public static SingletonInfo GetInstance()
        {
            if (_singleton == null)
            {
                Interlocked.CompareExchange(ref _singleton, new SingletonInfo(), null);
            }
            return _singleton;
        }
    }
}