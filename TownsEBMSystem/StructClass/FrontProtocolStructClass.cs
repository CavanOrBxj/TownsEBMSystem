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

    }


}
