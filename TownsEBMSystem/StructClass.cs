using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace TownsEBMSystem
{
    public class LoginInfo
    {
        public string username { get; set; }
        public string password { get; set; }

        public string AuthorizationCode { get; set; }

        public string AuthorizationCodeMD5 { get { return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(AuthorizationCode, "MD5"); } }
    }


    public class LoginInfoReback
    {
        public int code { get; set; }
        public string data { get; set; }

        public string msg { get; set; }
    }
}
