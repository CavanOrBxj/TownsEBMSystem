using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace TownsEBMSystem
{
    public class OnorOFFBroadcast
    {
        public string ebm_id { get; set; }
        public string power_switch { get; set; }
        public string ebm_class { get; set; }

        public string ebm_type { get; set; }

        public string ebm_level { get; set; }

        public string start_time { get; set; }//UTC时间

        public string end_time { get; set; }//UTC时间

        public string volume { get; set; }

        public string resource_code_type { get; set; }

        public List<string> resource_codeList;//资源码信息  默认所有资源码都是同一长度


        public List<MultilingualContentInfo> multilingual_contentList;//


        public int input_channel_id { get; set; }

        public List<int> OutPut_Channel_IdList;
    }


    public class MultilingualContentInfo
    {
        public string language_code { get; set; }

        public string coded_character_set { get; set; }

        public int text_length { get; set; }

        public string text_char { get; set; }

        public int agency_name_length { get; set; }

        public string agency_name_char { get; set; }

        public List<AuxiliaryInfo> AuxiliaryInfoList;
    }

    public class AuxiliaryInfo
    {
        public int auxiliary_data_type { get; set; }
        public int auxiliary_data_length { get; set; }
        public string auxiliary_data { get; set; }

    }


    public class OnorOFFResponse
    {
        public string front_code { get; set; }
        public string ebm_id { get; set; }

        //0表示成功  1表示失败
        public int result_code { get; set; }
        public int result_desc_length { get; set; }

        public string result_desc { get; set; }

        public int accept_stream_address_length { get; set; }

        public string accept_stream_address { get; set; }
    }

    public class PlayRecord_tcp_ts
    {
        public string resource_code { get; set; }

        public string IndexItemID { get; set; }

        public string prAreaName { get; set; }

        public string prEvnType { get; set; }

    }



    public class GeneralResponse
    {
        public string return_code { get; set; }
        public int return_data_length { get; set; }
        public string return_data { get; set; }

    }

    /// <summary>
    /// 功放开关
    /// </summary>
    public class SwitchAmplifier
    {
        public string switch_option { get; set; }
        public string resource_code_type { get; set; }

        public List<string> resource_codeList;

    }


    /// <summary>
    /// 白名单更新
    /// </summary>
    public class WhiteListUpdate
    {
        public List<WhiteListInfo> white_list;

    }

    public class WhiteListInfo
    {
        public string oper_type { get; set; }

        public string phone_number { get; set; }

        public string user_name { get; set; }

        public string permission_type { get; set; }

        public List<string> permission_area_codeList;

    }

    public class WhiteListRecord
    {
        public string username { get; set; }

        public string phone_number { get; set; }
        public string Organizations { get; set; }

        public string gb_codes { get; set; }
    }



    #region  任务开始上报
    public class TaskUploadBegin
    {
        public string front_code { get; set; }
        public string ebm_id { get; set; }

        public string program_resource { get; set; }
        public string ebm_class { get; set; }
        public string ebm_level { get; set; }

        public string ebm_type { get; set; }

        public string input_channel_id { get; set; }

        public List<output_channel_struct> utput_channel_structList;
        public string tel_length { get; set; }
        public string tel_number { get; set; }

        public string volume { get; set; }

        public List<string> volumeList;

    }

    public class output_channel_struct
    {
        public string output_channel_type { get; set; }
        public string output_channel_id { get; set; }
    }

    public class TaskUploadOver
    {
        public string front_code { get; set; }
        public string ebm_id { get; set; }
    }


    #endregion


    #region  心跳结构类
    public class RecvHeartBeat
    {
        public string front_code { get; set; }
        public string front_State { get; set; }
        /// <summary>
        /// 辅助数据  此项由包能胜于20180717 15：00添加
        /// </summary>
        public string auxiliarydata { get; set; }
        public string connection_time { get; set; }
    }
  
    #endregion

    public class GeneralRebackParam
    {
        public string reback_type { get; set; }
        public string reback_address { get; set; }

        public string resource_code_type { get; set; }

        public List<string> resource_codeList;//资源码信息  默认所有资源码都是同一长度

    }

    public class GeneralRebackCycle
    {
        public string reback_cycle { get; set; }

        public string resource_code_type { get; set; }

        public List<string> resource_codeList;//资源码信息  默认所有资源码都是同一长度

    }

    public class GeneralVolumn
    {
        public string volume { get; set; }

        public string resource_code_type { get; set; }

        public List<string> resource_codeList;//资源码信息  默认所有资源码都是同一长度

    }

    /// <summary>
    /// 通用网络参数设置
    /// </summary>
    public class GeneralNetworkParam
    {
        public string ip { get; set; }
        public string subnet_mask { get; set; }
        public string gateway { get; set; }
        public string resource_code_type { get; set; }
        public string resource_code { get; set; }

    }

    /// <summary>
    /// 传递参数载体
    /// </summary>
    public class ParamObject
    {
        public byte commandcode { get; set; }

        public object paramobj { get; set; }
    
    }

    public class Datagridviewmainitem
    {  
         /// <summary>
         /// 界面勾选状态
         /// </summary>
        public bool checkstate { get; set; }
        /// <summary>
        /// 区域信息
        /// </summary>
        public organizationdata areadata { get; set; }
        /// <summary>
        /// 操作类型  1播放  0停止
        /// </summary>
        public string deviceoperate { get; set; }

        /// <summary>
        /// http播放记录id   未播放为-1
        /// </summary>
        public string prlId { get; set; }
        /// <summary>
        /// 播放类型   日常 应急  未播放
        /// </summary>
        public string prEvnType { get; set; }
        /// <summary>
        /// TS播放后生成的ID  未启用时为"-1"
        /// </summary>s
        public string IndexItemID { get; set; }  

        
        public string areaname {
            get { return areadata.name; }
            set{} 
        }

    }
         
}
