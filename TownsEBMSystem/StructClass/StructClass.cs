using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace TownsEBMSystem
{
    public class LoginInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// 授权码
        /// </summary>
        public string licenseCode { get; set; }

        /// <summary>
        /// 授权码MD5加密
        /// </summary>
        public string licenseCodeMD5 { get { return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(licenseCode, "MD5"); } }
    }

    /// <summary>
    /// 登录返回结构
    /// </summary>
    public class LoginInfoReback
    {
        /// <summary>
        /// 状态码  0：成功 -1：失败
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回数据  无数据为null
        /// </summary>
        public string data { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
        public extendInfo extend;
    }

    public class extendInfo
    {
        /// <summary>
        /// 信任代码  
        /// </summary>
        public string creditCode { get; set; }
    }


    public class organizationInfo
    {

        /// <summary>
        /// 状态码  0：成功 -1：失败
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回数据  无数据为null
        /// </summary>
        public List<organizationdata> data { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
        public extendInfo extend;

     
    }

    public class organizationdata
    {
        public List<organizationdata> children;

        public string gb_code { get; set; }
        public int id { get; set; }

        public string name { get; set; }

    }


    /// <summary>
    /// 常规回复
    /// </summary>
    public class Generalresponse
    {

        /// <summary>
        /// 状态码  0：成功 -1：失败
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回数据  无数据为null
        /// </summary>
        public string data { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string msg { get; set; }


        /// <summary>
        /// 扩展数据
        /// </summary>
        public extendInfo extend;
    }

    /// <summary>
    /// 直播列表反馈类
    /// </summary>
    public class broadcastrecord
    {
        /// <summary>
        /// 状态码  0：成功 -1：失败
        /// </summary>
        public int code { get; set; }

        public List<broadcastrecorddata> data;

        /// <summary>
        /// 扩展数据
        /// </summary>
        public extendInfo extend;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string msg { get; set; }

    }

    public class broadcastrecorddata
    {
        /// <summary>
        /// 记录id
        /// </summary>
        public int prlId { get; set; }

        /// <summary>
        /// 区域名称
        /// </summary>
        public string  prAreaName { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string prEvnSource { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string prEvnType { get; set; }
        /// <summary>
        /// 播放时间
        /// </summary>
        public string prStarttime { get; set; }
        /// <summary>
        /// 节目名称
        /// </summary>
        public string programName { get; set; }
        /// <summary>
        /// 播放用户
        /// </summary>
        public string userName { get; set; }
    }
}
