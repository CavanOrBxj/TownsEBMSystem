using TownsEBMSystem.Enums;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace TownsEBMSystem.Utils
{
    public class ComboBoxHelper
    {
        public const byte ConfigureTimeServiceTag = 1;
        public const byte ConfigureSetAddressTag = 2;
        public const byte ConfigureWorkModeTag = 3;
        public const byte ConfigureMainFrequencyTag = 4;
        public const byte ConfigureRebackTag = 5;
        public const byte ConfigureDefaltVolumeTag = 6;
        public const byte ConfigureRebackPeriodTag = 7;
        public const byte ConfigureContentMoniterRetbackTag = 104;
        public const byte ConfigureContentRealMoniterTag = 105;
        public const byte ConfigureStatusRetbackTag = 106;
        public const byte ConfigureSoftwareUpGradeTag = 240;
        public const byte ConfigureRdsConfigTag = 8;
        public const byte ConfigureStatusRetbackGXTag = 9;




        public const byte ChangeProgramTag = 1;
        public const byte StopProgramTag = 2;
        public const byte PlayCtrlTag = 3;
        public const byte OutSwitchTag = 4;
        public const byte RdsTransferTag = 5;



        private static readonly Dictionary<ParamType, Dictionary<byte, string>> byteDic = new Dictionary<ParamType, Dictionary<byte, string>>()
        {
            { ParamType.AddressType, new Dictionary<byte, string>() { { 1, "逻辑地址" }, { 2, "物理地址" } } },
            { ParamType.DailySwitchStatus, new Dictionary<byte, string>() { { 1, "关闭输出" }, { 2, "开启输出" } } },
            { ParamType.ConfigRebackType, new Dictionary<byte, string>() { { 1, "短信，地址为 11 个数字电话号码" }, { 2, "IP 地址和端口" }, { 3, "域名和端口号" } } },
            { ParamType.ContentAuxiliaryData, new Dictionary<byte, string>() { { 0, "无"}, { 1, "MPEG-1 LayerI/II 音频文件（.MPG）" }, { 2, "MPEG-1 LayerIII 音频文件（.MP3）" },
                { 21, "MPEG-2 编码音视频文件（.MPG）" }, { 22, "H.264 编码音视频文件（.264）" }, { 23, "AVS+编码音视频文件（.AVS）" },
                { 41, "PNG 图片文件（.PNG）" }, { 42, "JPEG 图片文件（.JPG）"}, { 43, "GIF 图片文件（.GIF）"}, { 45, "RDS编码数据"}, } },
            { ParamType.ConfigStatusParameterTag, new Dictionary<byte, string>() { { 1, "终端音量" }, { 2, "本地地址" }, { 3, "回传地址" }, { 4, "终端资源编码" },
                { 5, "物理地址编码" }, { 6, "工作状态" }, { 7, "故障代码" }} },
            { ParamType.ConfigWorkMode, new Dictionary<byte, string>() { { 1, "主机全断电" }, { 2, "待机" }, { 3, "应急唤醒" } } },
            { ParamType.ContentCodeCharacter, new Dictionary<byte, string>() { { 0, "GB 2312-1980" } } },
            { ParamType.RdsTransferDaily, new Dictionary<byte, string>() { { 1, "应急广播适配器" }, { 2, "收扩机" }, { 3, "音柱" }, { 4, "应急广播适配器+收扩机+音柱" } } },
            { ParamType.ConfigUpGradeCarrMode, new Dictionary<byte, string>() { { 0, "单载波模式" }, { 1, "3780载波模式" }, { 2, "自动" } } },
            { ParamType.ConfigUpGradeFHMode, new Dictionary<byte, string>() { { 0, "自动" }, { 1, "PN420模式" }, { 2, "PN595模式" }, { 3, "PN945模式" } } },
            { ParamType.ConfigUpGradeILMode, new Dictionary<byte, string>() { { 0, "自动" }, { 1, "模式1（B=52,M=240）" }, { 2, "模式2（B=52,M=720）" } } },
            { ParamType.ConfigUpGradeMode, new Dictionary<byte, string>() { { 0, "强制升级，只要版本号不同就升级" }, { 1, "新版本升级，表示新版本比当前版本高就升级" },
                { 2, "只有指定版本的旧版固件才做升级" } } },
            { ParamType.ConfigRdsTerminalType, new Dictionary<byte, string>() { { 1, "应急广播适配器" }, { 2, "收扩机" }, { 3, "音柱" }, { 4, "应急广播适配器+收扩机" } } },
            { ParamType.ConfigTerminalRetbackType, new Dictionary<byte, string>() { { 1, "收到巡检指令后立刻回传" }, { 2, "自动周期回传" } } },

        };

        private static readonly Dictionary<ParamType, Dictionary<short, string>> shortDic = new Dictionary<ParamType, Dictionary<short, string>>()
        {
            { ParamType.ConfigRetbackMode, new Dictionary<short, string>() { { 1, "使用TCP协议发送" }, { 4, "使用FTP协议发送" }, { 5, "使用P2P协议发送" } } },
            { ParamType.ConfigRealRetbackMode, new Dictionary<short, string>() { { 7, "使用基于UDP的RTP协议发送" }, { 8, "使用基于TCP的RTP协议发送" },
                { 9, "使用基于UDP的RTSP协议发送"}, { 10, "使用基于TCP的RTSP协议发送"} } },
            { ParamType.ConfigFreqQAM, new Dictionary<short, string>() { { 0, "16QAM" }, { 1, "32QAM" }, { 2, "64QAM" }, { 3, "128QAM" }, { 4, "256QAM" } } },

        };

        private static readonly Dictionary<ParamType, Dictionary<int, string>> intDic = new Dictionary<ParamType, Dictionary<int, string>>()
        {
            { ParamType.ModulationType, new Dictionary<int, string>() { { 1, "16QAM" }, { 2, "32QAM" }, { 3, "64QAM" }, { 4, "128QAM" }, { 5, "256QAM" } } },
            { ParamType.ConfigUpGradeDeviceType, new Dictionary<int, string>() { { 1, "HM1521+DD3000+TC2800_DTMB_音柱_收扩机" }, { 2, "HM1521+DD3000+TC2800_DVB-C_音柱_收扩机" } } },

        };


        public static void InitOutSwitchType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "关闭输出";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "开启输出";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitOutSwitchType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "关闭输出";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "开启输出";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitRdsTransferDailyType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "应急广播适配器";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "收扩机";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "音柱";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "应急广播适配器+收扩机+音柱";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitRdsTransferDailyType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "应急广播适配器";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "收扩机";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "音柱";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "应急广播适配器+收扩机+音柱";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitAddressType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "逻辑地址";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "物理地址";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitAddressType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "逻辑地址";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "物理地址";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitRetbackModeType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(short));
            DataRow dr = dt.NewRow();
            dr["Display"] = "使用UDP协议发送";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用TCP协议发送";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitRetbackModeType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(short));
            DataRow dr = dt.NewRow();
            dr["Display"] = "使用TCP协议发送";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用FTP协议发送";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用P2P协议发送";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }


        public static void InitAudioRetbackModeType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(int));
            DataRow dr = dt.NewRow();
            dr["Display"] = "使用UDP协议发送";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用TCP协议发送";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }


        public static void InitRealRetbackModeType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(short));
            DataRow dr = dt.NewRow();
            dr["Display"] = "使用基于UDP的RTP协议发送";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用基于TCP的RTP协议发送";
            dr["Value"] = 8;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用基于UDP的RTSP协议发送";
            dr["Value"] = 9;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用基于TCP的RTSP协议发送";
            dr["Value"] = 10;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitRealRetbackModeType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(short));
            DataRow dr = dt.NewRow();
            dr["Display"] = "使用基于UDP的RTP协议发送";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用基于TCP的RTP协议发送";
            dr["Value"] = 8;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用基于UDP的RTSP协议发送";
            dr["Value"] = 9;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用基于TCP的RTSP协议发送";
            dr["Value"] = 10;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigRebackType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "短信，地址为 11 个数字电话号码";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "IP 地址和端口";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "域名和端口号";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigRebackType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "短信，地址为 11 个数字电话号码";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "IP 地址和端口";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "域名和端口号";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitAuxiliaryDataType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "无";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "MPEG-1 LayerI/II 音频文件（.MPG）";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "MPEG-1 LayerIII 音频文件（.MP3）";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "MPEG-2 编码音视频文件（.MPG）";
            dr["Value"] = 21;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "H.264 编码音视频文件（.264）";
            dr["Value"] = 22;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "AVS+编码音视频文件（.AVS）";
            dr["Value"] = 23;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PNG 图片文件（.PNG）";
            dr["Value"] = 41;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "JPEG 图片文件（.JPG）";
            dr["Value"] = 42;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "GIF 图片文件（.GIF）";
            dr["Value"] = 43;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "RDS编码数据";
            dr["Value"] = 45;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitFreqQAMType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(short));
            DataRow dr = dt.NewRow();
            dr["Display"] = "16QAM";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "32QAM";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "64QAM";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "128QAM";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "256QAM";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitFreqQAMType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(short));
            DataRow dr = dt.NewRow();
            dr["Display"] = "16QAM";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "32QAM";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "64QAM";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "128QAM";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "256QAM";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitParameterType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "终端音量";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "本地地址";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "回传地址";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "终端资源编码";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "物理地址编码";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "工作状态";
            dr["Value"] = 6;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "故障代码";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitRepeatTimesCombo(ComboBox box, bool isConfig = false)
        {
            DataTable dtTimes = new DataTable();
            dtTimes.Columns.Add("Dispaly", typeof(string));
            dtTimes.Columns.Add("Value", typeof(int));
            DataRow drTimes = dtTimes.NewRow();
            if (!isConfig)
            {
                drTimes = dtTimes.NewRow();
                drTimes["Dispaly"] = "重复发送";
                drTimes["Value"] = 0;
                dtTimes.Rows.Add(drTimes);
                drTimes = dtTimes.NewRow();
                drTimes["Dispaly"] = "自定义次数";
                drTimes["Value"] = -1;
                dtTimes.Rows.Add(drTimes);
            }
            drTimes = dtTimes.NewRow();
            drTimes["Dispaly"] = "1";
            drTimes["Value"] = 1;
            dtTimes.Rows.Add(drTimes);
            drTimes = dtTimes.NewRow();
            drTimes["Dispaly"] = "2";
            drTimes["Value"] = 2;
            dtTimes.Rows.Add(drTimes);
            drTimes = dtTimes.NewRow();
            drTimes["Dispaly"] = "3";
            drTimes["Value"] = 3;
            dtTimes.Rows.Add(drTimes);
            //if (isConfig)
            //{
            //    drTimes = dtTimes.NewRow();
            //    drTimes["Dispaly"] = "4";
            //    drTimes["Value"] = 4;
            //    dtTimes.Rows.Add(drTimes);
            //    drTimes = dtTimes.NewRow();
            //    drTimes["Dispaly"] = "5";
            //    drTimes["Value"] = 5;
            //    dtTimes.Rows.Add(drTimes);
            //}
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dtTimes;
        }

        public static void InitConfigWorkMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "主机全断电";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "待机";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "应急唤醒";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigWorkMode(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "主机全断电";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "待机";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "应急唤醒";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitCodeCharacter(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "GB 2312-1980";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB/T 18030-2005";
            //dr1["Value"] = 1;
            //dt.Rows.Add(dr1);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB 13000-2010";
            //dr1["Value"] = 2;
            //dt.Rows.Add(dr1);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB 21669-2008";
            //dr1["Value"] = 3;
            //dt.Rows.Add(dr1);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB 16959-1997";
            //dr1["Value"] = 4;
            //dt.Rows.Add(dr1);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitCodeCharacter(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "GB 2312-1980";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB/T 18030-2005";
            //dr1["Value"] = 1;
            //dt.Rows.Add(dr1);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB 13000-2010";
            //dr1["Value"] = 2;
            //dt.Rows.Add(dr1);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB 21669-2008";
            //dr1["Value"] = 3;
            //dt.Rows.Add(dr1);
            //dr1 = dt.NewRow();
            //dr1["Display"] = "GB 16959-1997";
            //dr1["Value"] = 4;
            //dt.Rows.Add(dr1);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitBroadcastType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "节目切播";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            //dr = dt.NewRow();
            //dr["Dispaly"] = "节目停播";
            //dr["Value"] = 2;
            //dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "播放控制";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "输出控制";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "Rds编码数据透传";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitBroadcastType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "节目切播";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            //dr = dt.NewRow();
            //dr["Dispaly"] = "节目停播";
            //dr["Value"] = 2;
            //dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "播放控制";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "输出控制";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "Rds编码数据透传";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigureType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "时间校准";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "区域码设置";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "工作模式设置";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "锁定频率设置";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "设置回传方式";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "设置默认音量";
            dr["Value"] = 6;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "回传周期";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "启动内容监测回传";
            dr["Value"] = 8;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "启动内容监测实时监听";
            dr["Value"] = 9;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "终端工作状态查询";
            dr["Value"] = 11;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigureType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "时间校准";
            dr["Value"] = ConfigureTimeServiceTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "区域码设置";
            dr["Value"] = ConfigureSetAddressTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "工作模式设置";
            dr["Value"] = ConfigureWorkModeTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "锁定频率设置";
            dr["Value"] = ConfigureMainFrequencyTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "设置回传方式";
            dr["Value"] = ConfigureRebackTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "设置默认音量";
            dr["Value"] = ConfigureDefaltVolumeTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "回传周期";
            dr["Value"] = ConfigureRebackPeriodTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "启动内容监测回传";
            dr["Value"] = ConfigureContentMoniterRetbackTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "启动内容监测实时监听";
            dr["Value"] = ConfigureContentRealMoniterTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "终端工作状态查询";
            dr["Value"] = ConfigureStatusRetbackTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "终端固件升级";
            dr["Value"] = ConfigureSoftwareUpGradeTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "Rds配置";
            dr["Value"] = ConfigureRdsConfigTag;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "终端工作状态查询(广西版)";
            dr["Value"] = ConfigureStatusRetbackGXTag;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMLevel(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "缺省";
            dr["Value"] = "00";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "1级（特别重大）";
            dr["Value"] = "01";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "2级（重大）";
            dr["Value"] = "02";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "3级（较大）";
            dr["Value"] = "03";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "4级（一般）";
            dr["Value"] = "04";
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMLevel(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "缺省";
            dr["Value"] = "00";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "1级（特别重大）";
            dr["Value"] = "01";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "2级（重大）";
            dr["Value"] = "02";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "3级（较大）";
            dr["Value"] = "03";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "4级（一般）";
            dr["Value"] = "04";
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMClass(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "发布系统演练";
            dr["Value"] = "0001";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "模拟演练";
            dr["Value"] = "0010";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "实际演练";
            dr["Value"] = "0011";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "应急广播";
            dr["Value"] = "0100";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "日常广播";
            dr["Value"] = "0101";
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMClass(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "发布系统演练";
            dr["Value"] = "0001";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "模拟演练";
            dr["Value"] = "0010";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "实际演练";
            dr["Value"] = "0011";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "应急广播";
            dr["Value"] = "0100";
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "日常广播";
            dr["Value"] = "0101";
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMStreamType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17191.2视频";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.2视频或GB/T 17191.2受限参数视频流";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17191.3音频";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.3音频";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.1 private_sections";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "包含专用数据的GB/T 17975.1 PES分组";
            dr["Value"] = 6;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13522 MHEG";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "附录A-DSM CC";
            dr["Value"] = 8;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ITU-T Rec.H.222.1";
            dr["Value"] = 9;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型A";
            dr["Value"] = 10;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型B";
            dr["Value"] = 11;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型C";
            dr["Value"] = 12;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型D";
            dr["Value"] = 13;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.1 辅助";
            dr["Value"] = 14;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 20090.2 视频";
            dr["Value"] = 66;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 20090.3 音频";
            dr["Value"] = 67;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMStreamType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17191.2视频";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.2视频或GB/T 17191.2受限参数视频流";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17191.3音频";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.3音频";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.1 private_sections";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "包含专用数据的GB/T 17975.1 PES分组";
            dr["Value"] = 6;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13522 MHEG";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "附录A-DSM CC";
            dr["Value"] = 8;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ITU-T Rec.H.222.1";
            dr["Value"] = 9;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型A";
            dr["Value"] = 10;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型B";
            dr["Value"] = 11;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型C";
            dr["Value"] = 12;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "ISO/IEC 13818-6 类型D";
            dr["Value"] = 13;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 17975.1 辅助";
            dr["Value"] = 14;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 20090.2 视频";
            dr["Value"] = 66;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "GB/T 20090.3 音频";
            dr["Value"] = 67;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMDiscriptorTag2(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "视频流描述符";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "音频流描述符";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "专用数据指示符描述符";
            dr["Value"] = 15;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "AVS视频流描述符";
            dr["Value"] = 63;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Dispaly"] = "DRA音频流描述符";
            dr["Value"] = 160;
            dt.Rows.Add(dr);


            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMDiscriptorTag2(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "视频流描述符";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "音频流描述符";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "专用数据指示符描述符";
            dr["Value"] = 15;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "AVS视频流描述符";
            dr["Value"] = 63;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMDiscriptorTag(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "有线传送系统描述符";
            dr["Value"] = 68;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "地面传送系统描述符";
            dr["Value"] = 90;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMDiscriptorTag(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "有线传送系统描述符";
            dr["Value"] = 68;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "地面传送系统描述符";
            dr["Value"] = 90;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMCDSDModulation(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "16QAM";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "32QAM";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "64QAM";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "128QAM";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "256QAM";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMCDSDFECOuter(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "无FEC外码";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "RS(204，188)";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMCDSDFECInner(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "无卷积编码";
            dr["Value"] = 15;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率1／2";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率2／3";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率3／4";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率5／6";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率7／8";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率3／5";
            dr["Value"] = 6;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率4／5";
            dr["Value"] = 7;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率9／10";
            dr["Value"] = 8;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "卷积码率13／15";
            dr["Value"] = 9;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDFEC(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "码率为0.4的FEC（7488，3008）";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "码率为0.6的FEC（7488，4512）";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "码率为0.8的FEC（7488，6016）";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDFrameHeaderMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "帧头模式1：PN420，PN相位变化";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "帧头模式1：PN420，PN相位不变";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "帧头模式2：PN595";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "帧头模式3：PN945，PN相位变化";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "帧头模式3：PN945，PN相位不变";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDInterleaveingMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dispaly", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Dispaly"] = "绞织模式1：B=52，M=240";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Dispaly"] = "绞织模式2：B=52，M=720";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            box.DisplayMember = "Dispaly";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDBModulation(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "4-QAM";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "4QAM-NR";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "16-QAM";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "32-QAM";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "64-QAM";
            dr["Value"] = 5;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDNumberOfSubcarrier(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "子载波数量C=1，有双导频";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "子载波数量C=1，无双导频";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "子载波数量C=3780";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDSfnMfnFlag(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(bool));
            DataRow dr = dt.NewRow();
            dr["Display"] = "单频网";
            dr["Value"] = false;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "多频网";
            dr["Value"] = true;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitEBMTDSDOtherFrequencyFlag(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(bool));
            DataRow dr = dt.NewRow();
            dr["Display"] = "未使用其它频率";
            dr["Value"] = false;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "使用了其它频率";
            dr["Value"] = true;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeCarrMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "单载波模式";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "3780载波模式";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "自动";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeCarrMode(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "单载波模式";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "3780载波模式";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "自动";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeFHMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "自动";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PN420模式";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PN595模式";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PN945模式";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeFHMode(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "自动";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PN420模式";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PN595模式";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "PN945模式";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeILMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "自动";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "模式1（B=52,M=240）";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "模式2（B=52,M=720）";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeILMode(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "自动";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "模式1（B=52,M=240）";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "模式2（B=52,M=720）";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeMode(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "强制升级，只要版本号不同就升级";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "新版本升级，新版本比当前版本高就升级";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "只有指定版本的旧版固件才做升级";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeMode(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "强制升级，只要版本号不同就升级";
            dr["Value"] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "新版本升级，新版本比当前版本高就升级";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "只有指定版本的旧版固件才做升级";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeDeviceType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(int));
            DataRow dr = dt.NewRow();
            dr["Display"] = "HM1521+DD3000+TC2800_DTMB_音柱_收扩机";
            dr["Value"] = 00001;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "HM1521+DD3000+TC2800_DVB-C_音柱_收扩机";
            dr["Value"] = 00002;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeDeviceType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(int));
            DataRow dr = dt.NewRow();
            dr["Display"] = "HM1521+DD3000+TC2800_DTMB_音柱_收扩机";
            dr["Value"] = 00001;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "HM1521+DD3000+TC2800_DVB-C_音柱_收扩机";
            dr["Value"] = 00002;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeModType(ComboBox box, DeviceOrderType type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            switch (type)
            {
                case DeviceOrderType.TDS_OFDM_DTMB:
                    dr["Display"] = "1QAM";
                    dr["Value"] = 17;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "2QAM";
                    dr["Value"] = 18;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "4QAM";
                    dr["Value"] = 19;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "16QAM";
                    dr["Value"] = 20;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "32QAM";
                    dr["Value"] = 21;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "64QAM";
                    dr["Value"] = 22;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "128QAM";
                    dr["Value"] = 23;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "256QAM";
                    dr["Value"] = 24;
                    dt.Rows.Add(dr);
                    break;
                case DeviceOrderType.DVBC:
                    dr["Display"] = "1QAM";
                    dr["Value"] = 1;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "2QAM";
                    dr["Value"] = 2;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "4QAM";
                    dr["Value"] = 3;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "16QAM";
                    dr["Value"] = 4;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "32QAM";
                    dr["Value"] = 5;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "64QAM";
                    dr["Value"] = 6;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "128QAM";
                    dr["Value"] = 7;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "256QAM";
                    dr["Value"] = 8;
                    dt.Rows.Add(dr);
                    break;
                default:
                    break;
            }
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeModType(DataGridViewComboBoxColumn box, DeviceOrderType type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            switch (type)
            {
                case DeviceOrderType.TDS_OFDM_DTMB:
                    dr["Display"] = "1QAM";
                    dr["Value"] = 17;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "2QAM";
                    dr["Value"] = 18;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "4QAM";
                    dr["Value"] = 19;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "16QAM";
                    dr["Value"] = 20;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "32QAM";
                    dr["Value"] = 21;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "64QAM";
                    dr["Value"] = 22;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "128QAM";
                    dr["Value"] = 23;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "256QAM";
                    dr["Value"] = 24;
                    dt.Rows.Add(dr);
                    break;
                case DeviceOrderType.DVBC:
                    dr["Display"] = "1QAM";
                    dr["Value"] = 1;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "2QAM";
                    dr["Value"] = 2;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "4QAM";
                    dr["Value"] = 3;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "16QAM";
                    dr["Value"] = 4;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "32QAM";
                    dr["Value"] = 5;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "64QAM";
                    dr["Value"] = 6;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "128QAM";
                    dr["Value"] = 7;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "256QAM";
                    dr["Value"] = 8;
                    dt.Rows.Add(dr);
                    break;
                default:
                    break;
            }
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigRdsTerminalType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "应急广播适配器";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "收扩机";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "音柱";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "应急广播适配器 + 收扩机";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitConfigRdsTerminalType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "应急广播适配器";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "收扩机";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "音柱";
            dr["Value"] = 3;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "应急广播适配器 + 收扩机";
            dr["Value"] = 4;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitGXTerminalRetbackType(ComboBox box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "收到巡检指令后立刻回传";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "自动周期回传";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitGXTerminalRetbackType(DataGridViewComboBoxColumn box)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(byte));
            DataRow dr = dt.NewRow();
            dr["Display"] = "收到巡检指令后立刻回传";
            dr["Value"] = 1;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["Display"] = "自动周期回传";
            dr["Value"] = 2;
            dt.Rows.Add(dr);
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeRate(ComboBox box, DeviceOrderType type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(int));
            DataRow dr = dt.NewRow();
            switch (type)
            {
                case DeviceOrderType.TDS_OFDM_DTMB:
                    dr["Display"] = "0.2";
                    dr["Value"] = 2;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "0.4";
                    dr["Value"] = 4;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "0.6";
                    dr["Value"] = 6;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "0.8";
                    dr["Value"] = 8;
                    dt.Rows.Add(dr);
                    break;
                case DeviceOrderType.DVBC:
                    dr["Display"] = "6900";
                    dr["Value"] = 6900;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "6875";
                    dr["Value"] = 6875;
                    dt.Rows.Add(dr);
                    break;
                default:
                    break;
            }
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }

        public static void InitUpGradeRate(DataGridViewComboBoxColumn box, DeviceOrderType type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Display", typeof(string));
            dt.Columns.Add("Value", typeof(int));
            DataRow dr = dt.NewRow();
            switch (type)
            {
                case DeviceOrderType.TDS_OFDM_DTMB:
                    dr["Display"] = "0.2";
                    dr["Value"] = 2;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "0.4";
                    dr["Value"] = 4;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "0.6";
                    dr["Value"] = 6;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "0.8";
                    dr["Value"] = 8;
                    dt.Rows.Add(dr);
                    break;
                case DeviceOrderType.DVBC:
                    dr["Display"] = "6900";
                    dr["Value"] = 6900;
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr["Display"] = "6875";
                    dr["Value"] = 6875;
                    dt.Rows.Add(dr);
                    break;
                default:
                    break;
            }
            box.DisplayMember = "Display";
            box.ValueMember = "Value";
            box.DataSource = dt;
        }


        public static string GetTypeStringValue(ParamType type, byte value)
        {
            if (byteDic.ContainsKey(type))
            {
                if (byteDic[type].ContainsKey(value))
                {
                    return byteDic[type][value];
                }
            }
            return string.Empty;
        }

        public static string GetTypeStringValue(ParamType type, short value)
        {
            if (shortDic.ContainsKey(type))
            {
                if (shortDic[type].ContainsKey(value))
                {
                    return shortDic[type][value];
                }
            }
            return string.Empty;
        }

        public static string GetTypeStringValue(ParamType type, int value)
        {
            if (intDic.ContainsKey(type))
            {
                if (intDic[type].ContainsKey(value))
                {
                    return intDic[type][value];
                }
            }
            return string.Empty;
        }

    }
}
