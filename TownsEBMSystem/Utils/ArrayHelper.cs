using System;
using System.Text;

namespace TownsEBMSystem.Utils
{
    class ArrayHelper
    {
        public static byte[] String2Bytes(string rdsData)
        {
            try
            {
                string[] data = rdsData.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                byte[] arB_byte = new byte[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    arB_byte[i] = Convert.ToByte(data[i], 16);
                }
                if (arB_byte.Length == 0)
                {
                    arB_byte = new byte[1];
                    arB_byte[0] = 0;
                }
                return arB_byte;
            }
            catch
            {
                return null;
            }
        }

        public static string Bytes2String(byte[] arrayData)
        {
            if (arrayData == null || arrayData.Length < 1) return "";
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < arrayData.Length; i++)
            {
                data.Append(Convert.ToString(arrayData[i], 16).PadLeft(2, '0').ToUpper() + " ");
            }
            data.Remove(data.Length - 1, 1);

            return data.ToString();
        }
    }
}
