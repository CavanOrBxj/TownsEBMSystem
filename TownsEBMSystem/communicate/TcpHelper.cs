using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace TownsEBMSystem
{
    public class TcpHelper
    {

        public Socket newclient;
        public Thread myThread;
        public object  SendTCPCommnand( object o, byte protocol_type)
        {
            byte[] backdata = null;
            object rebackobj = new object();
            switch (protocol_type)
            {
                case 0x01:
                    break;
                case 0x02:
                    break;
                case 0x03:
                    break;
                case 0x04:
                    OnorOFFBroadcast onoroff = (OnorOFFBroadcast)o;
                    backdata = OnorOFFBroadcastCommnand(onoroff);
                    rebackobj = DealOnorOFFResponse(SendTcpData(backdata)); 
                    break;
                case 0x05:
                    break;
                case 0x06:
                    break;
                case 0x07:
                    break;
                case 0x08:
                    break;
                case 0x09:
                    break;
                case 0x0A:
                    break;
                case 0x0B:
                    RebackPeriod rebackperiod = (RebackPeriod)o;
                    backdata = RebackPeriodCommand(rebackperiod);
                    rebackobj = GeneralResponse(SendTcpData(backdata));
                    break;
                case 0x0C:
                    WhiteListUpdate whiteList = (WhiteListUpdate)o;
                    backdata = WhiteListUpdateCommand(whiteList);
                    rebackobj = GeneralResponse(SendTcpData(backdata));

                    break;
                case 0x0D:
                    break;
                case 0x0E:
                    break;
                case 0x0F:
                    break;
                case 0x10:
                    break;
                case 0x11:
                    break;
                case 0x18:
                    break;
                case 0x19:
                    break;
                case 0x20:
                    break;
                case 0x21:
                    break;
                case 0x22:
                    break;
                case 0x40:
                    break;
                case 0x3F:
                    SwitchAmplifier switchAmplifier = (SwitchAmplifier)o;
                    backdata = SwitchAmplifierCommand(switchAmplifier);
                    rebackobj = GeneralResponse(SendTcpData(backdata));
                    break;
            }

         
            return rebackobj;
        }


        #region 解析方法
        public OnorOFFResponse DealOnorOFFResponse(byte[] inputdata)
        {
            OnorOFFResponse tmp = new OnorOFFResponse(); ;
            if (inputdata == null || inputdata.Length <= 0)
            {
                LogHelper.WriteLog(typeof(TcpHelper), "适配器数据返回异常");
            }
            else
            {
                tmp.front_code= bcd2Str(inputdata.Skip(9).Take(12).ToArray()).Substring(1); 
                tmp.ebm_id= bcd2Str(inputdata.Skip(21).Take(18).ToArray()).Substring(1);
                tmp.result_code = inputdata.Skip(39).Take(1).ToArray()[0];
                tmp.result_desc_length = GetDataLenth(inputdata.Skip(40).Take(4).ToArray());

                tmp.result_desc = System.Text.Encoding.UTF8.GetString(inputdata.Skip(44).Take(tmp.result_desc_length).ToArray());
                tmp.accept_stream_address_length = Byte2int(inputdata.Skip(44+ tmp.result_desc_length).Take(2).ToArray());
                tmp.accept_stream_address = System.Text.Encoding.UTF8.GetString(inputdata.Skip(44 + tmp.result_desc_length + 2).Take(tmp.accept_stream_address_length).ToArray()); 
            }
            return tmp;
        }

        public GeneralResponse GeneralResponse(byte[] inputdata)
        {
            GeneralResponse tmp = new GeneralResponse(); ;
            if (inputdata == null || inputdata.Length <= 0)
            {
                LogHelper.WriteLog(typeof(TcpHelper), "适配器数据返回异常");
            }
            else
            {
                tmp.return_code = GetDataLenth(inputdata.Skip(9).Take(4).ToArray()).ToString(); 
                tmp.return_data_length= GetDataLenth(inputdata.Skip(13).Take(4).ToArray());
                tmp.return_data= System.Text.Encoding.UTF8.GetString(inputdata.Skip(17).Take(tmp.return_data_length).ToArray());
            }
            return tmp;
        }

        #endregion

        public byte[] SendTCPCommnand(string inputdata)
        {
            return SendTcpData(str2byte(inputdata));
        }

        /// <summary>
        /// 改成同步接口 tcp发送接收数据
        /// </summary>
        /// <param name="inputdata"></param>
        public byte[] SendTcpData(byte[] inputdata)
        {
            Connect();
            if (inputdata != null)
            {
                newclient.Send(inputdata);
            }
            
            return   ReceiveMsg();
        }

        /// <summary>
        /// 初始化TCP发送套接字
        /// </summary>
        public void Connect()
        {
            newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ipadd = SingletonInfo.GetInstance().SendTCPdataIP;//远程IP
            int port = SingletonInfo.GetInstance().SendTCPdataPORT;//远程端口
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipadd), port);
            try
            {
                newclient.Connect(ie);
            }
            catch (SocketException ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "TCP客户端初始化失败");
                return;
            }
            //ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            //myThread = new Thread(myThreaddelegate);
            //myThread.Start();
        }

        /// <summary>
        /// TCP客户端的数据接收  与TCP服务端注意区分
        /// </summary>
        public byte[] ReceiveMsg()
        {
            try
            {
                byte[] data = new byte[1024];
                int recv = 0;
                int cyclecount=5;
                while (true)
                {
                    Thread.Sleep(100);
                    recv = newclient.Receive(data);

                    if (recv > 0)
                    {
                        data= data.Take(recv).ToArray();
                        break;
                    }
                    if (cyclecount > 0)
                    {

                        cyclecount--;
                    }
                    else
                    {
                        break;
                    }
                }
                newclient.Close();
              //  myThread.Abort();
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MainForm), "TCP数据回馈接收失败");
                return null;

            }

        }

        public string CreateEBM_ID()
        {
            string ebm_id = "";
            ebm_id = SingletonInfo.GetInstance().ebm_id_front;
           // string ebm_id_behind = SingletonInfo.GetInstance().ebm_id_behind;
            string datatime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
            if (datatime == SingletonInfo.GetInstance().ebm_id_behind)
            {
                SingletonInfo.GetInstance().ebm_id_count += 1;
            }
            else
            {
                SingletonInfo.GetInstance().ebm_id_behind = datatime;
                SingletonInfo.GetInstance().ebm_id_count = 1;
            }
            ebm_id += SingletonInfo.GetInstance().ebm_id_behind + SingletonInfo.GetInstance().ebm_id_count.ToString().PadLeft(4, '0');
            return ebm_id;
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
        /// 把string转byte[] 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public byte[] BCD2Byte(string str)
        {
            List<byte> tmp = new List<byte>();

            bool check = str.Length % 2 == 0 ? true : false;//奇偶校验判断  偶数true 奇数false

            for (int i = 0; i < str.Length / 2; i++)
            {
                tmp.Add((byte)Convert.ToInt32(str.Substring(i * 2, 2), 16));
            }

            if (!check)
            {
                string last = str.Substring(str.Length - 1, 1);
                tmp.Add((byte)Convert.ToInt32(last));
            }
            return tmp.ToArray();

        }

        /// <summary>
        /// 把string转ascii  "00000"    byte[] tt=byte[]{48, 48, 48, 48, 48} 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public byte[] Str2ASCII(string str)
        {
            //List<byte> tmp = new List<byte>();
            //for (int i = 0; i < str.Length; i++)
            //{
            //    tmp.Add(System.Text.Encoding.ASCII.GetBytes(str.Substring(i, 1))[0]) ; 
            //}

            //return tmp.ToArray();

            return Encoding.Default.GetBytes(str);
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
        /// 产生头部数据
        /// </summary>
        /// <param name="protocoltype"></param>
        /// <returns></returns>
        public byte[] Headdata(byte protocoltype)
        {
            List<byte> headdata = new List<byte>();
            headdata.Add(0x49);
            headdata.Add(0);
            headdata.Add(0x01);
            headdata.Add(protocoltype);
            headdata.Add(0x01);

            return headdata.ToArray();
        }

        public byte[] Buidsenddata(List<byte> obj,byte protocoltype)
        {
            List<byte> senddata = new List<byte>();
            senddata.AddRange(Headdata(protocoltype));
            senddata.AddRange(Int32toByte(obj.Count));
            senddata.AddRange(obj);
            int signaturelen = 4 + 6 + 64;
            byte[] utc = new byte[4];
            byte[] SN = new byte[6];
            byte[] Signaturedata = new byte[64];
            senddata.AddRange(Int2ByteArray(signaturelen));
            senddata.AddRange(utc);
            senddata.AddRange(SN);
            senddata.AddRange(Signaturedata);

            byte[] crcdata = CRC32.GetCRC32(senddata.ToArray());
            senddata.AddRange(crcdata);

            string data = "";
            foreach (byte item in senddata.ToArray())
            {
                data += item.ToString("X2").PadLeft(2, '0').ToUpper() + " ";
            }
            return senddata.ToArray();
        }

        /// <summary>
        /// 创建开关数据流
        /// </summary>
        /// <param name="onoroff"></param>
        /// <returns></returns>
        public byte[] OnorOFFBroadcastCommnand(OnorOFFBroadcast onoroff)
        {

            List<byte> classdata = new List<byte>();

            classdata.AddRange(BCD2Byte("F" + onoroff.ebm_id));

            classdata.Add((byte)Convert.ToInt32(onoroff.power_switch));



            classdata.Add((byte)Convert.ToInt32(onoroff.ebm_class));

            classdata.AddRange(Str2ASCII(onoroff.ebm_type));

            classdata.Add((byte)Convert.ToInt32(onoroff.ebm_level));


            classdata.AddRange(DatetimeStr2Byte(onoroff.start_time));

            classdata.AddRange(DatetimeStr2Byte(onoroff.end_time));

            classdata.Add((byte)Convert.ToInt32(onoroff.volume));


            classdata.Add((byte)Convert.ToInt32(onoroff.resource_code_type));

            int resource_code_number = onoroff.resource_codeList.Count;

            classdata.Add((byte)resource_code_number);


            int resource_code_length = 0;
            if (resource_code_number > 0)
            {
                byte[] dsadas = BCD2Byte("F" + onoroff.resource_codeList[0]);//特别注意 这个resource_code_length是指所占字节的长度  并非实际字符长度
                resource_code_length = dsadas.Length;
            }

            classdata.Add((byte)resource_code_length);
            foreach (string resourcecode in onoroff.resource_codeList)
            {
                classdata.AddRange(BCD2Byte("F" + resourcecode));
            }


            int multilingual_content_number = 0;
            if (onoroff.multilingual_contentList != null)
            {
                multilingual_content_number = onoroff.multilingual_contentList.Count;
            }

            classdata.Add((byte)multilingual_content_number);


            if (onoroff.multilingual_contentList != null)
            {
                foreach (MultilingualContentInfo item in onoroff.multilingual_contentList)
                {
                    classdata.AddRange(Str2ASCII(item.language_code));
                    classdata.Add((byte)Convert.ToInt32(item.coded_character_set));
                    classdata.AddRange(Int2ByteArray(item.text_length));
                    classdata.AddRange(Str2ASCII(item.text_char));
                    classdata.Add((byte)Convert.ToInt32(item.agency_name_length));
                    classdata.AddRange(Str2ASCII(item.agency_name_char));

                    int auxiliary_number = 0;
                    if (item.AuxiliaryInfoList != null)
                    {
                        auxiliary_number = item.AuxiliaryInfoList.Count;
                    }
                    classdata.Add((byte)auxiliary_number);

                    if (item.AuxiliaryInfoList != null)
                    {
                        foreach (AuxiliaryInfo auxiliary in item.AuxiliaryInfoList)
                        {
                            classdata.Add((byte)Convert.ToInt32(auxiliary.auxiliary_data_type));
                            classdata.AddRange(Int32toByte(auxiliary.auxiliary_data_length));
                            classdata.AddRange(Str2ASCII(auxiliary.auxiliary_data));
                        }
                    }

                }
            }
            int input_channel_id = 0;
            input_channel_id = onoroff.input_channel_id;
            classdata.Add((byte)input_channel_id);
            int output_channel_number = 0;
            if (onoroff.OutPut_Channel_IdList != null)
            {
                output_channel_number = onoroff.OutPut_Channel_IdList.Count;

            }
            classdata.Add((byte)output_channel_number);

            if (onoroff.OutPut_Channel_IdList != null)
            {
                foreach (int output_channel_id in onoroff.OutPut_Channel_IdList)
                {
                    classdata.Add((byte)output_channel_id);
                }
            }


            return Buidsenddata(classdata, 0x04);
        }


        public byte[] WhiteListUpdateCommand(WhiteListUpdate whiteList)
        {

            List<byte> classdata = new List<byte>();

            int white_list_length = whiteList.white_list.Count;
            classdata.Add((byte)white_list_length);

            for (int i = 0; i < white_list_length; i++)
            {
                classdata.Add((byte)Convert.ToInt32(whiteList.white_list[i].oper_type));
                int phone_number_length = Encoding.ASCII.GetBytes(whiteList.white_list[i].phone_number).Length;
                classdata.Add((byte)phone_number_length);
                classdata.AddRange(Encoding.ASCII.GetBytes(whiteList.white_list[i].phone_number));

                int user_name_length = Encoding.UTF8.GetBytes(whiteList.white_list[i].user_name).Length;
                classdata.Add((byte)user_name_length);
                classdata.AddRange(Encoding.UTF8.GetBytes(whiteList.white_list[i].user_name));
                classdata.Add((byte)Convert.ToInt32(whiteList.white_list[i].permission_type));

                int permission_area_number = whiteList.white_list[i].permission_area_codeList.Count;
                classdata.Add((byte)permission_area_number);

                int permission_area_length = BCD2Byte(whiteList.white_list[i].permission_area_codeList[0]).Length;
                classdata.Add((byte)permission_area_length);

                for (int j = 0; j < permission_area_number; j++)
                {
                    classdata.AddRange(BCD2Byte(whiteList.white_list[i].permission_area_codeList[j]));
                }
            }
            return Buidsenddata(classdata, 0x0C);
        }


        public byte[] RebackPeriodCommand(RebackPeriod rebackperiod)
        {

            List<byte> classdata = new List<byte>();


            classdata.AddRange(Int32toByte(Convert.ToInt32(rebackperiod.reback_cycle))); 
            classdata.Add((byte)Convert.ToInt32(rebackperiod.resource_code_type));

            int resource_code_number = rebackperiod.resource_codeList.Count;

            classdata.Add((byte)resource_code_number);


            int resource_code_length = 0;
            if (resource_code_number > 0)
            {
                byte[] dsadas = BCD2Byte("F" + rebackperiod.resource_codeList[0]);//特别注意 这个resource_code_length是指所占字节的长度  并非实际字符长度
                resource_code_length = dsadas.Length;
            }

            classdata.Add((byte)resource_code_length);
            foreach (string resourcecode in rebackperiod.resource_codeList)
            {
                classdata.AddRange(BCD2Byte("F" + resourcecode));
            }
            return Buidsenddata(classdata, 0x0B);
        }

        public byte[] SwitchAmplifierCommand(SwitchAmplifier switchamplifier)
        {

            List<byte> classdata = new List<byte>();


            classdata.Add((byte)Convert.ToInt32(switchamplifier.switch_option));
            classdata.Add((byte)Convert.ToInt32(switchamplifier.resource_code_type));

            int resource_code_number = switchamplifier.resource_codeList.Count;

            classdata.Add((byte)resource_code_number);


            int resource_code_length = 0;
            if (resource_code_number > 0)
            {
                byte[] dsadas = BCD2Byte("F" + switchamplifier.resource_codeList[0]);//特别注意 这个resource_code_length是指所占字节的长度  并非实际字符长度
                resource_code_length = dsadas.Length;
            }

            classdata.Add((byte)resource_code_length);
            foreach (string resourcecode in switchamplifier.resource_codeList)
            {
                classdata.AddRange(BCD2Byte("F" + resourcecode));
            }
            return Buidsenddata(classdata, 0x3F);
        }

    }
}
