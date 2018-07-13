
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
        
        public void Dispose()
        {
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
    }
}
