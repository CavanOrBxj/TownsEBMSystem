
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TownsEBMSystem.Enums;

namespace TownsEBMSystem
{
    public class TableDataHelper
    {
        public static JObject ReadConfig()
        {
            try
            {
                string path = ConfigurationManager.AppSettings["StreamConfigPath"];
                //string path = ConfigurationManager.AppSettings[Program.IsCopyProcess ? "StreamConfigPathCopy" : "StreamConfigPath"];
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                    return null;
                }
                else
                {
                    //BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));
                    //if (br.PeekChar() != -1)
                    //{
                    //    string jsonConfig = br.ReadString();
                    //}
                    //string str = br.ReadString();
                    //var jsonConfig = Encoding.Default.GetString(Convert.FromBase64String(str));

                    string jsonConfig = File.ReadAllText(path, Encoding.UTF8);
                    JObject jo = JObject.Parse(jsonConfig);
                    return jo;
                }
            }
            catch
            {
                return null;
            }
        }

        //public static bool WriteConfig(EBMStream EbmStream)
        //{
        //    try
        //    {
        //        string path = ConfigurationManager.AppSettings["StreamConfigPath"];
        //        //string path = ConfigurationManager.AppSettings[Program.IsCopyProcess ? "StreamConfigPathCopy" : "StreamConfigPath"];

        //        JObject jo = new JObject();
        //        jo["ElementaryPid"] = EbmStream.ElementaryPid;
        //        jo["PMT_Pid"] = EbmStream.PMT_Pid;
        //        jo["Program_id"] = EbmStream.Program_id;
        //        jo["sDestSockAddress"] = EbmStream.sDestSockAddress;
        //        jo["Section_length"] = EbmStream.Section_length;
        //        jo["sLocalSockAddress"] = EbmStream.sLocalSockAddress;
        //        jo["Stream_BitRate"] = EbmStream.Stream_BitRate;
        //        jo["Stream_id"] = EbmStream.Stream_id;

        //        File.WriteAllText(path, jo.ToString(), Encoding.UTF8);
        //        //byte[] bytes = Encoding.Default.GetBytes(jo.ToString());
        //        //string str = Convert.ToBase64String(bytes);
        //        //BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));
        //        //bw.Write(str);
        //        //bw.Close();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static JObject ReadTable(TableType type)
        {
            try
            {
                string path = string.Empty;
                switch (type)
                {
                    case TableType.Index:
                        path = ConfigurationManager.AppSettings["EBMIndexPath"];
                        break;
                    case TableType.DailyBroadcast:
                        path = ConfigurationManager.AppSettings["DailyBroadcastPath"];
                        break;
                    case TableType.CertAuth:
                        path = ConfigurationManager.AppSettings["EBMCertAuthPath"];
                        break;
                    case TableType.Configure:
                        path = ConfigurationManager.AppSettings["EBMConfigurePath"];
                        break;
                    case TableType.Content:
                        path = ConfigurationManager.AppSettings["EBMContentPath"];
                        break;

                    case TableType.Organization:
                        path = ConfigurationManager.AppSettings["OrganizationConfigurePath"];
                        break;
                }
                if (path == string.Empty)
                {
                    return null;
                }
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                    return null;
                }
                string table = File.ReadAllText(path, Encoding.UTF8);
                JObject jo = JObject.Parse(table);
                return jo;
            }
            catch
            {
                return null;
            }
        }

        public static bool WriteTable(TableType type, params object[] data)
        {
            try
            {
                string path = string.Empty;
                switch (type)
                {
                    case TableType.Index:
                        path = ConfigurationManager.AppSettings["EBMIndexPath"];
                        break;
                    case TableType.DailyBroadcast:
                        path = ConfigurationManager.AppSettings["DailyBroadcastPath"];
                        break;
                    case TableType.CertAuth:
                        path = ConfigurationManager.AppSettings["EBMCertAuthPath"];
                        break;
                    case TableType.Configure:
                        path = ConfigurationManager.AppSettings["EBMConfigurePath"];
                        break;
                    case TableType.Content:
                        path = ConfigurationManager.AppSettings["EBMContentPath"];
                        break;
                    case TableType.Organization:
                        path = ConfigurationManager.AppSettings["OrganizationConfigurePath"];
                        break;
                }
                if (path == string.Empty) return false;

                
                    JObject jo = new JObject();
                    for (int i = 0; i < data.Length; i++)
                    {
                        jo[i.ToString()] = JsonConvert.SerializeObject(data[i]);
                    }
                    File.WriteAllText(path, jo.ToString(), Encoding.UTF8);
                

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读取配置项
        /// </summary>
        /// <param name="property">为空则返回整个配置项的json对象</param>
        /// <param name="value"></param>
        public static void ReadAppConfig(string property, out object value)
        {
            try
            {
                value = null;
                string path = ConfigurationManager.AppSettings["AppConfigurePath"];
                if (string.IsNullOrWhiteSpace(path)) return;
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                    return;
                }
                string table = Encoding.UTF8.GetString(File.ReadAllBytes(path));
                JObject jo = JObject.Parse(table);
                value = string.IsNullOrWhiteSpace(property) ? jo : jo[property];
            }
            catch
            {
                value = null;
            }
        }

        public static bool WriteAppConfig(string property, object value)
        {
            try
            {
                object config = null;
                ReadAppConfig("", out config);
                JObject jo = new JObject();
                if (config != null)
                {
                    jo = config as JObject;
                }
                jo[property] = value.ToString();
                string path = ConfigurationManager.AppSettings["AppConfigurePath"];
                if (string.IsNullOrWhiteSpace(path)) return false;
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(jo.ToString()));
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static byte[] GetFileData(string fileUrl)
        {
            FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
            string sFileEx = "";
            try
            {
                string[] fileUrlPart = fileUrl.Split('.');
                if( fileUrlPart.Length > 1 )
                    sFileEx = fileUrlPart[1].ToUpper();
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);

                List<byte> listData = new List<byte>();
                if (sFileEx == "TXT")
                {
                    string sFileData = "";
                    sFileData = Encoding.ASCII.GetString(buffur).Trim();
                    string[] dataPars = sFileData.Split(' ');

                    for (int i = 0; i < dataPars.Length; i++)
                    {
                        if( dataPars[i].Length >= 2)
                            listData.Add(byte.Parse(dataPars[i], System.Globalization.NumberStyles.HexNumber));
                    }
                    buffur = listData.ToArray();
                }
                return buffur;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

    }
}
