
using EBMTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TownsEBMSystem
{
    public class Cert_
    {
        public string CertDataId { get; set; }
        public string Cert_data { get; set; }
        public int Tag { get; set; }  //数据类型 1表示文本数据  0表示文件数据

        public bool SendState { get; set; }//是否发送
    }

    public class CertTmp
    {
        public string CertDataid { get; set; }
        public string CertDataHexStr { get; set; }
        public string isSend { get; set; }  ////1表示发送，0表示不发送
    }

    public class CertAuthTmp
    {
        public string CertAuthDataid { get; set; }
        public string CertAuthDataHexStr { get; set; }
        public string isSend { get; set; }  ////1表示发送，0表示不发送
    }


    public class EBMIndexTmp
    {
        public string IndexItemID { get; set; }
        public string S_EBM_id { get; set; }
        public string S_EBM_original_network_id { get; set; }

        public string S_EBM_start_time { get; set; }
        public string S_EBM_end_time { get; set; }
        public string S_EBM_type { get; set; }
        public string S_EBM_class { get; set; }
        public string S_EBM_level { get; set; }
        public string List_EBM_resource_code { get; set; }
        public string BL_details_channel_indicate { get; set; }
        public string DesFlag { get; set; }
        public string S_details_channel_transport_stream_id { get; set; }
        public string S_details_channel_program_number { get; set; }
        public string S_details_channel_PCR_PID { get; set; }

        public object DeliverySystemDescriptor { get; set; }

        public List<ProgramStreamInfotmp> List_ProgramStreamInfo;

        public int descriptor_tag { get; set; }
    }


    public class CableDeliverySystemDescriptortmp
    {
        public string B_FEC_inner { get; set; }
        public string B_FEC_outer { get; set; }
        public string B_Modulation { get; set; }
        public string D_frequency { get; set; }
        public string D_Symbol_rate { get; set; }
    }

    public class TerristrialDeliverySystemDescriptortmp
    {
        public string B_FEC { get; set; }
        public string B_Frame_header_mode { get; set; }
        public string B_Interleaveing_mode { get; set; }
        public string B_Modulation { get; set; }
        public string B_Number_of_subcarrier { get; set; }
        public string D_Centre_frequency { get; set; }
        public string L_Other_frequency_flag { get; set; }
        public string L_Sfn_mfn_flag { get; set; }
    }


    public class ProgramStreamInfotmp
    {
        public string B_stream_type { get; set; }
        public string S_elementary_PID { get; set; }

        public object Descriptor2 { get; set; }//由于序列化问题改为 object类型
    }


    public class Descriptor2
    {
        public byte B_descriptor_tag { get; set; }
        public byte[] B_descriptor { get; set; }
    }

    public class CertAuth_
    {
        public string CertAuthDataId { get; set; }
        public bool SendState { get; set; }
        public string CertAuth_data { get; set; }
        public int Tag { get; set; }  //数据类型 1表示文本数据  0表示文件数据
    }

    public class CertAuthGlobal_
    {
      public  List<Cert_> list_Cert;
      public List<CertAuth_> list_CertAuth;
      public int Repeat_times { get; set; }
    }


    public class EBMIndex_
    {
        public string IndexItemID { get; set; }
        public bool SendState { get; set; }
        public string NickName { get; set; }
        public EBIndex EBIndex { get; set; }
        public bool BL_details_channel_indicate
        {
            get { return EBIndex.BL_details_channel_indicate; }
            set { EBIndex.BL_details_channel_indicate = value; }
        }
        public bool DesFlag { get; set; }
        private Cable_delivery_system_descriptor cdsd;
        private Terristrial_delivery_system_descriptor tdsd;
        public Cable_delivery_system_descriptor CDSDDescriptor
        {
            get { return cdsd; }
            set
            {
                cdsd = value;
                if (value != null) DetlChlDescriptor = value.GetDescriptor();
            }
        }
        public Terristrial_delivery_system_descriptor TDSDDescriptor
        {
            get { return tdsd; }
            set
            {
                tdsd = value;
                if (value != null) DetlChlDescriptor = value.GetDescriptor();
            }
        }
        public object DeliverySystemDescriptor
        {
            get
            {
                if (CDSDDescriptor != null)
                {
                    return CDSDDescriptor;
                }
                else if (TDSDDescriptor != null)
                {
                    return TDSDDescriptor;
                }
                return null;
            }
            set
            {
                if (value is Cable_delivery_system_descriptor)
                {
                    CDSDDescriptor = value as Cable_delivery_system_descriptor;
                    TDSDDescriptor = null;
                }
                else if (value is Terristrial_delivery_system_descriptor)
                {
                    TDSDDescriptor = value as Terristrial_delivery_system_descriptor;
                    CDSDDescriptor = null;
                }
            }
        }
        public StdDescriptor DetlChlDescriptor
        {
            get { return EBIndex.DetlChlDescriptor; }
            set { EBIndex.DetlChlDescriptor = value; }
        }
        public List<ProgramStreamInfo> List_ProgramStreamInfo
        {
            get { return EBIndex.list_ProgramStreamInfo; }
            set { EBIndex.list_ProgramStreamInfo = value; }
        }
        public List<string> List_EBM_resource_code
        {
            get { return EBIndex.List_EBM_resource_code; }
            set { EBIndex.List_EBM_resource_code = value; }
        }
        public string S_details_channel_PCR_PID
        {
            get { return EBIndex.S_details_channel_PCR_PID; }
            set { EBIndex.S_details_channel_PCR_PID = value; }
        }
        public string S_details_channel_program_number
        {
            get { return EBIndex.S_details_channel_program_number; }
            set { EBIndex.S_details_channel_program_number = value; }
        }
        public string S_details_channel_transport_stream_id
        {
            get { return EBIndex.S_details_channel_transport_stream_id; }
            set { EBIndex.S_details_channel_transport_stream_id = value; }
        }
        public string S_EBM_class
        {
            get { return EBIndex.S_EBM_class; }
            set { EBIndex.S_EBM_class = value; }
        }
        public string S_EBM_end_time
        {
            get { return EBIndex.S_EBM_end_time; }
            set { EBIndex.S_EBM_end_time = value; }
        }
        public string S_EBM_id
        {
            get { return EBIndex.S_EBM_id; }
            set { EBIndex.S_EBM_id = value; }
        }
        public string S_EBM_level
        {
            get { return EBIndex.S_EBM_level; }
            set { EBIndex.S_EBM_level = value; }
        }
        public string S_EBM_original_network_id
        {
            get { return EBIndex.S_EBM_original_network_id; }
            set { EBIndex.S_EBM_original_network_id = value; }
        }
        public string S_EBM_start_time
        {
            get { return EBIndex.S_EBM_start_time; }
            set { EBIndex.S_EBM_start_time = value; }
        }
        public string S_EBM_type
        {
            get { return EBIndex.S_EBM_type; }
            set { EBIndex.S_EBM_type = value; }
        }
    }

    public class EBMIndexGlobal_
    {
        public List<EBMIndex_> ListEbIndex { get; set; }
        public int Repeat_times { get; set; }
    }


    public class EBMConfigureGlobal_
    {
        public List<TimeService_> ListTimeService { get; set; }

        public List<SetAddress_> ListSetAddress { get; set; }

        public List<WorkMode_> ListWorkMode { get; set; }

        public List<MainFrequency_> ListMainFrequency { get; set; }

        public List<Reback_> ListReback { get; set; }

        public List<DefaltVolume_> ListDefaltVolume { get; set; }

        public List<RebackPeriod_> ListRebackPeriod { get; set; }

        public List<ContentMoniterRetback_> ListContentMoniterRetback { get; set; }

        public List<ContentRealMoniter_> ListContentRealMoniter { get; set; }

        public List<StatusRetback_> ListStatusRetback { get; set; }

        public List<SoftwareUpGrade_> ListSoftwareUpGrade { get; set; }

        public List<RdsConfig_> ListRdsConfig { get; set; }

        public List<ContentRealMoniterGX_> ListContentRealMoniterGX { get; set; }
        public int Repeat_times { get; set; }
    }


    public class DailyBroadcastGlobal_
    {
        public List<ChangeProgram_> ListChangeProgram { get; set; }

        public List<PlayCtrl_> ListPlayCtrl { get; set; }

        public List<OutSwitch_> ListOutSwitch { get; set; }

        public List<RdsTransfer_> ListRdsTransfer { get; set; }

        public int Repeat_times { get; set; }
    }

    public class EBContentGlobal_
    {
        public List<EBMID_Content> ListEBContent { get; set; }
        public int Repeat_times { get; set; }
    }


    public class SendMQData
    {
        public string CommandType { get; set; }
        public string Data { get; set; }

    }

    public class OperatorData
    {
        public string OperatorType { get; set; }

        public string ModuleType { get; set; }
        public object Data { get; set; }

    }


    public class ModifyEBMIndex
    {
        public string IndexItemID { get; set; }
        public string Data { get; set; }

    }


    #region 内建类  配置表

    public class TimeService_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureTimeServiceTag; } }
        public EBConfigureTimeService Configure { get; set; }
        //public string TimeSer
        //{
        //    get { return Configure.Real_time.ToString(); }
        //    set { Configure.Real_time = Convert.ToDateTime(value); }
        //}
        public bool GetSystemTime { get; set; }
        private int sendTick = 60;
        public int SendTick
        {
            get { return sendTick; }
            set { sendTick = value; }
        }

    }

    public class SetAddress_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureSetAddressTag; } }
        public EBConfigureSetAddress Configure { get; set; }
        //public string S_Logic_address
        //{
        //    get { return Configure.S_Logic_address; }
        //    set { Configure.S_Logic_address = value; }
        //}
        //public string S_Phisical_address
        //{
        //    get { return Configure.S_Phisical_address; }
        //    set { Configure.S_Phisical_address = value; }
        //}

    }

    public class WorkMode_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureWorkModeTag; } }
        public EBConfigureWorkMode Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        //public byte B_Terminal_wordmode
        //{
        //    get { return Configure.B_Terminal_wordmode; }
        //    set { Configure.B_Terminal_wordmode = value; }
        //}
  
    }

    public class MainFrequency_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureMainFrequencyTag; } }
        public EBConfigureMainFrequency Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        //public int Freq
        //{
        //    get { return Configure.Freq; }
        //    set { Configure.Freq = value; }
        //}
        //public short QAM
        //{
        //    get { return Configure.QAM; }
        //    set { Configure.QAM = value; }
        //}
        //public int SymbolRate
        //{
        //    get { return Configure.SymbolRate; }
        //    set { Configure.SymbolRate = value; }
        //}
   
    }

    public class Reback_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureRebackTag; } }
        public EBConfigureReback Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        //public byte B_reback_type
        //{
        //    get { return Configure.B_reback_type; }
        //    set { Configure.B_reback_type = value; }
        //}
        //public string S_reback_address
        //{
        //    get { return Configure.S_reback_address; }
        //    set { Configure.S_reback_address = value; }
        //}
   
    }

    public class DefaltVolume_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureDefaltVolumeTag; } }
        public EBConigureDefaltVolume Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        //public short Column
        //{
        //    get { return Configure.Column; }
        //    set { Configure.Column = value; }
        //}
    
    }

    public class RebackPeriod_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureRebackPeriodTag; } }

        public EBConfigureRebackPeriod Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        //public int reback_period
        //{
        //    get { return Configure.reback_period; }
        //    set { Configure.reback_period = value; }
        //}
  
    }

    public class ContentMoniterRetback_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureContentMoniterRetbackTag; } }
        public EBConfigureContentMoniterRetbackGX Configure { get; set; }


        //public int Start_package_index
        //{
        //    get { return Configure.Start_package_index; }
        //    set { Configure.Start_package_index = value; }
        //}
        //public string S_Reback_serverIP
        //{
        //    get { return Configure.S_Audio_reback_serverip; }
        //    set { Configure.S_Audio_reback_serverip = value; }
        //}

        //public int I_Reback_PORT
        //{
        //    get { return Configure.I_Audio_reback_port; }
        //    set { Configure.I_Audio_reback_port = value; }
        //}

        //public string S_File_id
        //{
        //    get { return Configure.S_File_id; }
        //    set { Configure.S_File_id = value; }
        //}
        ////public byte B_AudioRetback_mode
        ////{
        ////    get { return Configure.B_Audio_reback_mod; }
        ////    set { Configure.B_Audio_reback_mod = value; }
        ////}

        //public int B_AudioRetback_mode
        //{
        //    get { return (int)Configure.B_Audio_reback_mod; }
        //    set { Configure.B_Audio_reback_mod = (byte)value; }
        //}
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
   
    }

    public class ContentRealMoniter_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureContentRealMoniterTag; } }
        public EBConfigureContentRealMoniter Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        //public string S_EBM_id
        //{
        //    get { return Configure.S_EBM_id; }
        //    set { Configure.S_EBM_id = value; }
        //}
        //public string S_Server_addr
        //{
        //    get { return Configure.S_Server_addr; }
        //    set { Configure.S_Server_addr = value; }
        //}
        //public short Retback_mode
        //{
        //    get { return Configure.Retback_mode; }
        //    set { Configure.Retback_mode = value; }
        //}
        //public int Moniter_time_duration
        //{
        //    get { return Configure.Moniter_time_duration; }
        //    set { Configure.Moniter_time_duration = value; }
        //}
   
    }


    public class ContentRealMoniterGX_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureContentRealMoniterTag; } }
        public EBConfigureContentRealMoniterGX Configure { get; set; }
    }

    public class StatusRetback_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ConfigureStatusRetbackTag; } }
        public EBConfigureStatusRetback Configure { get; set; }
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
     
    }

    public class SoftwareUpGrade_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag
        {
            get
            {
                return Utils.ComboBoxHelper.ConfigureSoftwareUpGradeTag;
            }
        }
        public EBConfigureSoftwareUpGrade Configure { get; set; }
        //public byte B_CarrMode
        //{
        //    get { return Configure.B_CarrMode; }
        //    set { Configure.B_CarrMode = value; }
        //}
        //public byte B_FHMode
        //{
        //    get { return Configure.B_FHMode; }
        //    set { Configure.B_FHMode = value; }
        //}
        //public byte B_ILMode
        //{
        //    get { return Configure.B_ILMode; }
        //    set { Configure.B_ILMode = value; }
        //}
        //public byte B_Mode
        //{
        //    get { return Configure.B_Mode; }
        //    set { Configure.B_Mode = value; }
        //}
        //public byte B_ModType
        //{
        //    get { return Configure.B_ModType; }
        //    set { Configure.B_ModType = value; }
        //}
        //public int B_Pid
        //{
        //    get { return Configure.B_Pid; }
        //    set { Configure.B_Pid = value; }
        //}
        //public int I_DeviceType
        //{
        //    get { return Configure.I_DeviceType; }
        //    set { Configure.I_DeviceType = value; }
        //}
        //public int I_Freq
        //{
        //    get { return Configure.I_Freq; }
        //    set { Configure.I_Freq = value; }
        //}
        //public int I_Rate
        //{
        //    get { return Configure.I_Rate; }
        //    set { Configure.I_Rate = value; }
        //}
        //public string S_NewVersion
        //{
        //    get { return Configure.S_NewVersion; }
        //    set { Configure.S_NewVersion = value; }
        //}
        //public string S_OldVersion
        //{
        //    get { return Configure.S_OldVersion; }
        //    set { Configure.S_OldVersion = value; }
        //}
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        public Enums.DeviceOrderType DeviceOrderType { get; set; }
   
    }

    public class RdsConfig_ : Configure
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag
        {
            get
            {
                return Utils.ComboBoxHelper.ConfigureRdsConfigTag;
            }
        }
        public EBConfigureRdsConfig Configure { get; set; }
        //public byte B_Rds_terminal_type
        //{
        //    get { return Configure.B_Rds_terminal_type; }
        //    set { Configure.B_Rds_terminal_type = value; }
        //}
        //public byte B_Address_type
        //{
        //    get { return Configure.B_Address_type; }
        //    set { Configure.B_Address_type = value; }
        //}
        public string RdsDataText
        {
            get { return Utils.ArrayHelper.Bytes2String(Configure.Br_Rds_data); }
            set
            {
                var bytes = Utils.ArrayHelper.String2Bytes(value);
                if (bytes == null)
                {
                 
                }
                else
                {
                    Configure.Br_Rds_data = bytes;
                }
            }
        }
    }

    public abstract class Configure
    {
        public abstract byte B_Daily_cmd_tag { get; }
       // public bool SendState { get; set; }
    }

    #endregion

    #region 内建类 日常广播表

    [Serializable]
    public class ChangeProgram_ : DailyProgram
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.ChangeProgramTag; } }
        public DailyCmdChangeProgram Program { get; set; }
  
        public string BroadcastStatus { get; set; }
    }

    [Serializable]
    public class StopPorgram_ : DailyProgram
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.StopProgramTag; } }
        public DailyCmdProgramStop Program { get; set; }
       
    }

    [Serializable]
    public class OutSwitch_ : DailyProgram
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.OutSwitchTag; } }
        public DailyCmdOutSwitch Program { get; set; }
    
    }

    [Serializable]
    public class PlayCtrl_ : DailyProgram
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.PlayCtrlTag; } }
        public DailyCmdPlayCtrl Program { get; set; }
      
    }

    [Serializable]
    public class RdsTransfer_ : DailyProgram
    {
        public string ItemID { get; set; }
        public override byte B_Daily_cmd_tag { get { return Utils.ComboBoxHelper.RdsTransferTag; } }
        public DailyCmdRdsTransfer Program { get; set; }
    
        public string RdsDataText
        {
            get { return Utils.ArrayHelper.Bytes2String(Program.Br_Rds_data); }
            set
            {
                var bytes = Utils.ArrayHelper.String2Bytes(value);
                if (bytes == null)
                {
                    //MessageBox.Show("输入数据有误，请重新输入。数据按十六进制输入，多个数据用,或空格分隔(如AA FF)", "错误",
                    //    MessageBoxButtons.OK);
                }
                else
                {
                    Program.Br_Rds_data = bytes;
                }
            }
        }
    }

    [Serializable]
    public abstract class DailyProgram
    {
       // public abstract string Summary { get; }
        public abstract byte B_Daily_cmd_tag { get; }
      //  public bool SendState { get; set; }
    }

    #endregion





    // 摘要: 
    //     多语种内容类
    public class MultilangualContent_
    {
        public string ItemID;
        public string B_code_character_set;
        public string B_message_text;
        public List<AuxiliaryData_> list_auxiliary_data;
        public string S_agency_name;
        public string S_language_code;
    }

    // 摘要: 
    //     辅助数据类
    public class AuxiliaryData_
    {
        public string DisplayData ;//B_auxiliary_data;
        public string Type;// B_auxiliary_data_type;
    }

    public class EBMID_Content
    {
        public string EBM_ID { get; set; }
        public List<MultilangualContent_> MultilangualContentList { get; set; }
       
    }
}
