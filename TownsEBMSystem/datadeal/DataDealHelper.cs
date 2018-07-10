
//using Apache.NMS;
using EBMTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace TownsEBMSystem
{
    /// <summary>
    /// 数据缓存处理
    /// </summary>
    class DataDealHelper : IDisposable
    {

        public delegate void MyDelegate(object data);

        public static event MyDelegate MyEvent; //注意须关键字 static  

        public void Dispose()
        {
        }

        
        /// <summary>
        /// Json结构规范化
        /// </summary>
        /// <param name="data"></param>
        private void JsonstructureDeal(ref  string data)
        {
            int loacal = data.IndexOf('[');
            data = data.Substring(loacal, data.Length - loacal - 2);

        }

        public List<byte[]> GetSendCert(List<Cert_> certList)
        {
            if (certList.Count == 0)
            {
                return null;
            }
            List<byte[]> list = new List<byte[]>();
            foreach (var cert in certList)
            {
                if (cert.SendState)
                {
                    if (cert.Tag == 1)
                    {
                        list.Add(Encoding.GetEncoding("GB2312").GetBytes(cert.Cert_data));
                    }
                }
            }
            return list;
        }


        public List<EBIndex> GetSendEBMIndex(List<EBMIndex_> EBIndex_List)
        {
            if (EBIndex_List.Count == 0)
            {
                return null;
            }
            List<EBIndex> list = new List<EBIndex>();
            foreach (var index in EBIndex_List)
            {
                if (index.SendState)
                {
                    list.Add(index.EBIndex);
                    //if (!index.DesFlag)
                    //{
                    //    list[list.Count - 1].DetlChlDescriptor = null;
                    //}  测试注释 20180709
                }
            }
            return list;
        }

        public List<byte[]> GetSendCertAuth(List<CertAuth_> certAuthList)
        {
            if (certAuthList.Count == 0)
            {
                return null;
            }
            List<byte[]> list = new List<byte[]>();
            foreach (var cert in certAuthList)
            {
                if (cert.SendState)
                {
                    if (cert.Tag == 1)
                    {
                        list.Add(Encoding.GetEncoding("GB2312").GetBytes(cert.CertAuth_data));
                    }
                }
            }
            return list;
        }
    }
}
