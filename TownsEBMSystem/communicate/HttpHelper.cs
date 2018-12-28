using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace TownsEBMSystem
{
    public class HttpHelper
    {

        public object PostCommnand( object o, string requesttype)
        {
            string sReturnString="";
            string paraUrlCoded = "";
            string strURL = "";

            object reback = new object();

            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            switch (requesttype)
            {
                case "登录":
                    LoginInfo loginfo = (LoginInfo)o;
                    paraUrlCoded = "username";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.username);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("password");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.password);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("licenseCode");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.licenseCodeMD5);

                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("btn_one");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.btn_one);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("btn_two");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.btn_two);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("btn_three");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.btn_three);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("btn_four");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.btn_four);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("btn_five");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.btn_five);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("btn_six");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.btn_six);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("lock_pwd");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.lock_pwd);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("lock_cycle");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(loginfo.localParam.lock_cycle);

                    strURL = SingletonInfo.GetInstance().HttpServer + "platform/login.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);

                    if (sReturnString!="")
                    {
                       LoginInfoReback logreb = Serializer.Deserialize<LoginInfoReback>(sReturnString);
                        reback = logreb;
                    }
                    break;

                case "获取区域":
                    paraUrlCoded = "creditCode";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    strURL = SingletonInfo.GetInstance().HttpServer + "organization/get.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString != "")
                    {
                        organizationInfo OrgInfo= Serializer.Deserialize<organizationInfo>(sReturnString);
                        reback = OrgInfo;
                    }
                    break;
                case "播放":
                    SendPlayInfo playInfo = (SendPlayInfo)o;
                    string id = "";
                    foreach (var item in playInfo.organization_List)
                    {
                        id += "," + item.id;
                    }

                    id = id.Substring(1, id.Length-1);


                    paraUrlCoded = "pidValue";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().pid);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("organization_id");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(id);

                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("broadcastType");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(playInfo.broadcastType);

                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("creditCode");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    strURL = SingletonInfo.GetInstance().HttpServer + "broadcast/program/play.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString!="")
                    {
                        Generalresponse  response = Serializer.Deserialize<Generalresponse>(sReturnString);
                        reback = response;
                    }
                    break;

                case "图标播放":

                    SendPlayInfoNew sendPlayInfoNew = (SendPlayInfoNew)o;

                    string selectedID = sendPlayInfoNew.Id_List[0];
                    string broadcasttype = sendPlayInfoNew.broadcastType;
                    paraUrlCoded = "pidValue";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().pid);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("organization_id");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(selectedID);

                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("broadcastType");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(broadcasttype);

                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("creditCode");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    strURL = SingletonInfo.GetInstance().HttpServer + "broadcast/program/play.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString!="")
                    {
                        Generalresponse  response = Serializer.Deserialize<Generalresponse>(sReturnString);
                        reback = response;
                    }
                    break;
                case "直播列表":
                    strURL = SingletonInfo.GetInstance().HttpServer + "broadcast/program/record.htm";
                    paraUrlCoded = "creditCode";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString != "")
                    {
                        broadcastrecord response = Serializer.Deserialize<broadcastrecord>(sReturnString);
                        reback = response;
                    }
                    break;

                case "心跳":
                    paraUrlCoded = "creditCode";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    strURL = SingletonInfo.GetInstance().HttpServer + "platform/recHeartBeat.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString != "")
                    {
                        HeartBeatResponse response = Serializer.Deserialize<HeartBeatResponse>(sReturnString);
                        reback = response;
                    }
                    break;

                case "停止":

                    List<string> Stop_id_List = (List<string>)o;
                    string stopid = "";
                    foreach (var item in Stop_id_List)
                    {
                        stopid += "," + item;
                    }

                    stopid = stopid.Substring(1, stopid.Length - 1);


                    paraUrlCoded = "creditCode";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    paraUrlCoded += "&" + System.Web.HttpUtility.UrlEncode("prlId");
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(stopid);
                    strURL = SingletonInfo.GetInstance().HttpServer + "broadcast/program/stop.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString != "")
                    {
                        Generalresponse response = Serializer.Deserialize<Generalresponse>(sReturnString);
                        reback = response;
                    }
                    break;
                case "地图":
                    paraUrlCoded = "creditCode";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    strURL = SingletonInfo.GetInstance().HttpServer + "platform/googleMap/getUrl.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString != "")
                    {
                        Generalresponse response = Serializer.Deserialize<Generalresponse>(sReturnString);
                        reback = response;
                    }
                    break;

                case "版本信息"://版本信息  20181203
                    paraUrlCoded = "creditCode";
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(SingletonInfo.GetInstance().creditCode);
                    strURL = SingletonInfo.GetInstance().HttpServer + "/platform/getVersion.htm";
                    sReturnString = SendHttpData(strURL, paraUrlCoded);
                    if (sReturnString != "")
                    {
                        UpgradInfo response = Serializer.Deserialize<UpgradInfo>(sReturnString);
                        reback = response;
                    }
                    else
                    {
                        reback = null;
                    }
                    break;
            }
            return reback;
        }

        /// <summary>
        /// Http同步接收接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public string SendHttpData(string url,string para)
        {
            try
            {
                string strURL = url;
                System.Net.HttpWebRequest request;
                request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
                //Post请求方式
                request.Method = "POST";
                // 内容类型
                request.ContentType = "application/x-www-form-urlencoded";

                // 参数经过URL编码
                string paraUrlCoded = para;
                byte[] payload;
                //将URL编码后的字符串转化为字节
                payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                //设置请求的 ContentLength 
                request.ContentLength = payload.Length;
                //获得请 求流
                System.IO.Stream writer = request.GetRequestStream();
                //将请求参数写入流
                writer.Write(payload, 0, payload.Length);
                // 关闭请求流
                writer.Close();

                System.Net.HttpWebResponse response;
                // 获得响应流
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string responseText = myreader.ReadToEnd();
                myreader.Close();
                return responseText;
            }
            catch (Exception)
            {
                return "";
            }
           
        }
    }
}
