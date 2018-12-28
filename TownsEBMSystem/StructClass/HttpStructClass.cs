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


        public LocalParam localParam;


    }

    /// <summary>
    /// 本地镇级参数 包括按钮显示 锁屏密码  锁屏周期
    /// </summary>
    public class LocalParam
    {
        /// <summary>
        /// 本地按钮显示  新增于20180801
        /// </summary>
        public string btn_one { get; set; }
        public string btn_two { get; set; }

        public string btn_three { get; set; }

        public string btn_four { get; set; }

        public string btn_five { get; set; }
        public string btn_six { get; set; }
        /// <summary>
        /// 当前锁频密码 
        /// </summary>
        public string lock_pwd { get; set; }
        /// <summary>
        /// 锁频周期
        /// </summary>
        public string lock_cycle { get; set; }

        /// <summary>
        /// 韩峰那边有变化 这个值就变化
        /// </summary>
        public string mark { get; set; }
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
        public extendInfo_login extend;
    }

    public class extendInfo_login
    {
        /// <summary>
        /// 信任代码  
        /// </summary>
        public string creditCode { get; set; }
    }

    public class extendInfo
    {
        public string extend { get; set; }
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

        public string resource { get { return "0612"+gb_code+"00"; } }

    }



    public class HeartBeatResponse
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
        public LocalParam extend { get; set; }
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
        public extendInfo extend { get; set; }
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

    public class SendPlayInfo
    {
        public List<organizationdata> organization_List { get; set; }
        public string broadcastType { get; set; }

    }

    public class SendPlayInfoNew
    {
        public List<string> Id_List { get; set; }
        public string broadcastType { get; set; }

    }

    public class LabelInfo
    {
        public string Label1txt { get; set; }
        public string Label9txt { get; set; }
        public string Label8txt { get; set; }
        public string Label3txt { get; set; }
        public string Label7txt { get; set; }
        public string Label6txt { get; set; }
    }


    public class UpgradInfo
    {
        public string fileSize { get; set; }
        public string version { get; set; }
    }
}
