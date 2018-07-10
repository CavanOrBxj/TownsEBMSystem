using System.Threading;
using System.Collections.Generic;
using System.Data;
using EBSignature;

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

        public int InlayCAType;//内置CA的类型  1表示EbMSGCASignature  2表示EbMSGPLSignature
        public bool IsUseCAInfo;//表明是否启用CA  true表示启用  false表示不启用
        public EbmSignature InlayCA;
        public bool IsStartSend;//是否已经启动发送
        public string cramblertype;
        public int OriginalNetworkId;//应急广播原始网络标识符 0-65535
        public bool IsGXProtocol;//表明是否是广西协议
        public bool IsUseAddCert;//是否使用增加的证书
        public string Cert_SN;//增加的证书编号
        public string PriKey;//增加证书的私钥
        public string PubKey;//增加证书的公钥
        public int Cert_Index;//证书索引


        public string input_channel_id;//线路切换记录ID

        public string ebm_id;//与县平台断线的情况下 播发广播生成的id

        public string starttime;//tcp指令的发送时间  改时间将用于组装TS指令
        public string endtime;//tcp指令的发送时间  改时间将用于组装TS指令


        public string S_details_channel_transport_stream_id;
        public string S_details_channel_program_number;
        public string S_details_channel_PCR_PID;

        public string ts_pid;

        public int IndexItemID;//全局唯一的索引表识别位

        private SingletonInfo()                                                                 
        {

            InlayCA = new EbmSignature();
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


            InlayCAType = 0;
            IsUseCAInfo = true; //默认启用CA
            IsStartSend = false;
            cramblertype = "";
            OriginalNetworkId = 0;//是否需要保存？   20180328
            IsGXProtocol = false;
            IsUseAddCert = false;
            Cert_SN = "";
            PriKey = "";
            PubKey = "";
            Cert_Index = 0;

            input_channel_id = "";

            ebm_id = "";
            starttime = "";
            endtime = "";


            S_details_channel_transport_stream_id = "";
            S_details_channel_program_number = "";
            S_details_channel_PCR_PID = "";


            ts_pid = "";
            IndexItemID = 0;

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