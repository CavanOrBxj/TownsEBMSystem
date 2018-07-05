using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TownsEBMSystem
{
    class UtcHelper
    {
        public static int ConvertDateTimeInt(System.DateTime time)
        {

            double intResult = 0;

            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

            intResult = (time - startTime).TotalSeconds;

            return (int)intResult;

        }
        public static DateTime ConvertIntDatetime(double utc)
        {

            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

            startTime = startTime.AddSeconds(utc);

            startTime = startTime.AddHours(8);//转化为北京时间(北京时间=UTC时间+8小时 )            

            return startTime;

        }

    
    }
}
