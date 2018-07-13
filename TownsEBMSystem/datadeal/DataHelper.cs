using EBSignature;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TownsEBMSystem
{
    public class DataHelper: IDisposable
    {

        public delegate void MyDelegate(object data);

        public static event MyDelegate MyEvent; //注意须关键字 static 


        public byte[] HandleReceiveData(byte[] receivedata, int datalenth)
        {
            List<byte> dataList = new List<byte>();
            foreach (byte item in receivedata)
            {
                dataList.Add(item);
            }
            if (dataList.Count > 0)
            {
                byte[] recdata = dataList.ToArray().Take(datalenth).ToArray();

                if (recdata[0] == 0x50)//判断属于来自平台的合法数据头
                {

                    #region 测试打印 接收到的数据
                    string str = "";
                    foreach (var item in recdata)
                    {
                        str += " " + item.ToString("X2");
                    }
                    LogHelper.WriteLog(typeof(DataHelper), str);
                    #endregion
                    //CRC校验 
                    if (CheckCRC32(recdata))
                    {
                        //if (Verifysignature(recdata))
                        //{

                        //    byte[] data = HandleRealData(recdata);
                        //    if (data.Length > 0)
                        //    {
                        //        return data;
                        //    }

                        //}
                        //else
                        //{
                        //    LogHelper.WriteLog(typeof(DataHelper), "验签失败");
                        //    return null;
                        //}

                        //验签暂时关闭  20180510
                        byte[] data = HandleRealData(recdata);
                        if (data.Length > 0)
                        {
                            return data;
                        }
                    }
                    else
                    {
                        LogHelper.WriteLog(typeof(DataHelper), "CRC校验失败");
                        return null;
                    }
                }
                else
                {
                    LogHelper.WriteLog(typeof(DataHelper), "收到不合法数据");
                    return null;
                }
            }
            else
            {
                return null;
            }

            return null;

        }

        /// <summary>
        /// CRC判断
        /// </summary>
        /// <param name="fulldata"></param>
        /// <returns></returns>
        public bool CheckCRC32(byte[] fulldata)
        {
            //后四个字节为CRC值
            int lenth = fulldata.Length; ;
            byte[] datatmp = fulldata.Take(lenth - 4).ToArray(); ;
            byte[] crcdata = CRC32.GetCRC32(datatmp);
            byte[] crcbyte = fulldata.Skip(lenth - 4).ToArray();

            for (int i = 0; i < crcdata.Length; i++)
            {
                if (crcdata[i] != crcbyte[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 计算UTC 4个字节
        /// </summary>
        /// <returns></returns>
        private byte[] CalculateUTC()
        {
            DateTime dt = DateTime.UtcNow;
            int tt = UtcHelper.ConvertDateTimeInt(dt);
            string str = Convert.ToString(tt, 16).PadLeft(8, '0');
            List<byte> Array = new List<byte>();
            for (int i = 0; i < 4; i++)
            {

                Array.Add((byte)(Convert.ToInt32(str.Substring(0 + 2 * i, 2), 16)));

            }
            return Array.ToArray();
        }

        public byte[] HandleRealData(byte[] receivedata)
        {
            byte protocol_type = receivedata[3];
            byte[] backdata = null;
            byte[] length = receivedata.Skip(5).Take(4).ToArray();

            int datalenth = GetDataLenth(length);

            byte[] datareal = receivedata.Skip(9).Take(datalenth).ToArray();
            switch (protocol_type)
            {
                case 0x18:
                    backdata = BuildBackData(TaskBeginUpdataDeal(datareal), 0x22);
                    break;
                case 0x20:
                    backdata = BuildBackData(0);
                    break;

            }
            return backdata;
        }
       

        /// <summary>
        /// 构建通用回复数据
        /// </summary>
        /// <param name="Returncode"></param>
        /// <returns></returns>
        private byte[] BuildBackData(int Returncode)
        {
            List<byte> backdatabyte = new List<byte>();
            string codedescrib = "";
            switch (Returncode)
            {
                case -1:
                    codedescrib = "未知错误";
                    break;
                case 0:
                    codedescrib = "执行成功";
                    break;
                case 1:
                    codedescrib = "数据长度错误";
                    break;
                case 2:
                    codedescrib = "版本号错误";
                    break;
                case 3:
                    codedescrib = "指令冲突错误";
                    break;
            }

          //  byte[] byteArray = System.Text.UTF8Encoding.Default.GetBytes(codedescrib);



            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(codedescrib);

            backdatabyte = GetHeadbyte(0x49, 0x21, 1, 4 + 4 + byteArray.Length);



            byte[] returncode = BitConverter.GetBytes(Returncode);
            Array.Reverse(returncode);
            backdatabyte.AddRange(returncode);

            int len = byteArray.Length;
            backdatabyte.AddRange(Int32toByte(len));


            backdatabyte.AddRange(byteArray);



            //bool cou = false;
            //int lastlen = 0;
           // byte[] signatureAll = signature(backdatabyte.ToArray(), ref cou, ref lastlen);

            //byte[] utc;
            //byte[] SN;
            //byte[] Signaturedata;

            //if (cou)
            //{
            //    utc = signatureAll.Skip(4 + 3 + 32 + lastlen).Take(4).ToArray();
            //    SN = signatureAll.Skip(4 + 3 + 32 + lastlen + 4).Take(6).ToArray();
            //    Signaturedata = signatureAll.Skip(4 + 3 + lastlen + 4 + 6).Take(64).ToArray();
            //}
            //else
            //{
            //    utc = signatureAll.Skip(4 + 3 + lastlen).Take(4).ToArray();
            //    SN = signatureAll.Skip(4 + 3 + lastlen + 4).Take(6).ToArray();
            //    Signaturedata = signatureAll.Skip(4 + 3 + lastlen + 4 + 6).Take(64).ToArray();

            //}


            //int signaturelen = utc.Length + SN.Length + Signaturedata.Length;

            //暂时不用签名 对应数值赋值0
            int signaturelen = 4 + 6 + 64;
            byte[] utc = new byte[4];
            byte[] SN = new byte[6];
            byte[] Signaturedata = new byte[64];




            backdatabyte.AddRange(Int2ByteArray(signaturelen));
            backdatabyte.AddRange(utc);
            backdatabyte.AddRange(SN);
            backdatabyte.AddRange(Signaturedata);

            byte[] crcdata = CRC32.GetCRC32(backdatabyte.ToArray());
            backdatabyte.AddRange(crcdata);



            string str = "";
            foreach (byte item in backdatabyte)
            {
                str += " " + item.ToString("X2");
            }

            LogHelper.WriteLog(typeof(DataHelper), str);
            return backdatabyte.ToArray();
        }

        /// <summary>
        /// 构建非通用回复数据
        /// </summary>
        /// <param name="inputdata"></param>
        /// <returns></returns>
        public byte[] BuildBackData(byte[] inputdata,byte commandtype)
        {
            List<byte> backdatabyte = new List<byte>();
            backdatabyte = GetHeadbyte(0x50, commandtype, 2, inputdata.Length);
            backdatabyte.AddRange(inputdata);
            //bool cou = false;
            //int lastlen = 0;
            //byte[] signatureAll = signature(backdatabyte.ToArray(), ref cou, ref lastlen);

            //byte[] utc;
            //byte[] SN;
            //byte[] Signaturedata;

            //if (cou)
            //{
            //    utc = signatureAll.Skip(4 + 3 + 32 + lastlen).Take(4).ToArray();
            //    SN = signatureAll.Skip(4 + 3 + 32 + lastlen + 4).Take(6).ToArray();
            //    Signaturedata = signatureAll.Skip(4 + 3 + lastlen + 4 + 6).Take(64).ToArray();
            //}
            //else
            //{
            //    utc = signatureAll.Skip(4 + 3 + lastlen).Take(4).ToArray();
            //    SN = signatureAll.Skip(4 + 3 + lastlen + 4).Take(6).ToArray();
            //    Signaturedata = signatureAll.Skip(4 + 3 + lastlen + 4 + 6).Take(64).ToArray();

            //}


            // int signaturelen = utc.Length + SN.Length + Signaturedata.Length;

            //暂时不用签名 所以对应数值赋值0
            int signaturelen = 4 + 6 + 64;
            byte[] utc = new byte[4];
            byte[] SN = new byte[6];
            byte[] Signaturedata = new byte[64];





            backdatabyte.AddRange(Int2ByteArray(signaturelen));
            backdatabyte.AddRange(utc);
            backdatabyte.AddRange(SN);
            backdatabyte.AddRange(Signaturedata);

            byte[] crcdata = CRC32.GetCRC32(backdatabyte.ToArray());
            backdatabyte.AddRange(crcdata);



            string str = "";
            foreach (byte item in backdatabyte)
            {
                str += " " + item.ToString("X2");
            }

            LogHelper.WriteLog(typeof(DataHelper), str);
            return backdatabyte.ToArray();
        }

        /// <summary>
        /// 构建数据头
        /// </summary>
        /// <param name="headtype"></param>
        /// <param name="commandtype"></param>
        /// <param name="commandfrom"></param>
        /// <param name="datalen"></param>
        /// <returns></returns>
        public List<byte> GetHeadbyte(byte headtype, byte commandtype, int commandfrom, int datalen)
        {
            List<byte> headbyte = new List<byte>();
            headbyte.Add(headtype);
            headbyte.Add(0);
            headbyte.Add(1);
            headbyte.Add(commandtype);
            headbyte.Add(2);


            byte[] bData = BitConverter.GetBytes(datalen);
            Array.Reverse(bData);
            headbyte.AddRange(bData);
            return headbyte;

        }


       
        public byte[] TaskBeginUpdataDeal(byte[] input)
        {
            TaskUploadBegin tmp = new TaskUploadBegin();
            tmp.program_resource = input.Skip(30). Take(1).ToArray()[0].ToString();

            if (tmp.program_resource=="3"|| tmp.program_resource == "4")
            {
                tmp.ebm_class= input.Skip(31).Take(1).ToArray()[0].ToString();
            }

            #region 线程通知县平台  或者事件触发
            DataHelper.MyEvent(tmp);
            #endregion


            #region  构建回复数据
            List<byte> rebackdata = new List<byte>();
            rebackdata.Add(0);//0表示成功 1表示失败
            byte[] reserved = new byte[6];
            rebackdata.AddRange(reserved);
            return rebackdata.ToArray();
            #endregion
        }

        #region  格式转换

        /// <summary>
        /// BCD码转为10进制串(阿拉伯数据)   不支持0开头
        /// </summary>
        /// <param name="bytes">BCD码 </param>
        /// <returns>10进制串 </returns>
        public String bcd2Str(byte[] bytes)
        {
            string str = "";
            foreach (byte item in bytes)
            {
                str += Convert.ToString(item, 16).PadLeft(2, '0');
            }
            return str;
        }

        /// <summary>
        /// 把string转byte[] 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public byte[] BCD2Byte(string str)
        {
            List<byte> tmp = new List<byte>();

            for (int i = 0; i < str.Length / 2; i++)
            {
                tmp.Add((byte)Convert.ToInt32(str.Substring(i * 2, 2), 16));
            }
            return tmp.ToArray();

        }


        public string Byte2DatetimeStr(byte[] input)
        {
            Array.Reverse(input);
            int sec = BitConverter.ToInt32(input, 0);           // 从字节数组转换成 int
            DateTime mmmm = DateTime.Parse(DateTime.Now.ToString("1970-01-01 08:00:00"));
            DateTime dt = mmmm.AddSeconds(sec);
            return dt.ToString("yyyy-MM-dd HH:mm:ss");

        }

        /// <summary>
        /// DateTime string 转4字节byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] DatetimeStr2Byte(string input)
        {

            DateTime dd = Convert.ToDateTime(input);
            DateTime mmmm = DateTime.Parse(DateTime.Now.ToString("1970-01-01 08:00:00"));
            int sec = (int)(dd.Subtract(mmmm).TotalSeconds);

            byte[] time = BitConverter.GetBytes(sec);

            Array.Reverse(time);

            return time;
        }


        /// <summary>
        /// 把 2字节的byte[] 转int  高位在前 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int Byte2int(byte[] input)
        {
            string strlen = "";
            foreach (byte item in input)
            {
                strlen += Convert.ToString(item, 16).PadLeft(2, '0');
            }
            int datalen = Convert.ToInt32(strlen, 16);

            return datalen;

        }

        /// <summary>
        /// string datetime 转 byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] strDatetime2byte(string input)
        {
            DateTime tt = Convert.ToDateTime(input);
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            string eee = Convert.ToString((int)(tt - startTime).TotalSeconds, 16).PadLeft(8, '0');
            byte[] bytedata = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytedata[i] = (byte)Convert.ToInt32(eee.Substring(i * 2, 2), 16);
            }

            return bytedata;
        }

        /// <summary>
        /// 把Int值转化成两个字节的byte[]  高位在前
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] Int2ByteArray(int data)
        {
            string str = Convert.ToString(data, 16).PadLeft(4, '0');
            List<byte> Array = new List<byte>();
            for (int i = 0; i < 2; i++)
            {

                Array.Add((byte)(Convert.ToInt32(str.Substring(0 + 2 * i, 2), 16)));

            }
            return Array.ToArray();
        }

        /// <summary>
        /// 4字节转ip
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string Byte2StringIP(byte[] input)
        {
            string ip = "";
            foreach (var item in input)
            {
                ip += "." + item.ToString();
            }
            ip = ip.Substring(1);
            return ip;

        }


        /// <summary>
        /// 把4字节的byte[] 转int 高位在前
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int GetDataLenth(byte[] input)
        {

            Array.Reverse(input);
            return BitConverter.ToInt32(input, 0);
        }

        /// <summary>
        /// 把int32转4字节byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private byte[] Int32toByte(int input)
        {

            byte[] bData = BitConverter.GetBytes(input);
            Array.Reverse(bData);

            return bData;
        }


        /// <summary>
        /// 判断byte[]中是否都为255
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool CheckFFvalue(byte[] input)
        {
            foreach (var item in input)
            {
                if (item!=255)
                {
                    return false;
                }
            }
            return true;
        
        }

        /// <summary>
        /// 把 FF AA DD AA 的字符串转byte[]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] str2byte(string input)
        {
            string[] qwe = input.Split(' ');
            List<byte> bb = new List<byte>();

            foreach (var item in qwe)
            {
                bb.Add((byte)Convert.ToInt32(item, 16));
            }
            return bb.ToArray();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
