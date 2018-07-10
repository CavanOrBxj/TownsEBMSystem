using System.Text.RegularExpressions;

namespace TownsEBMSystem.Utils
{
    class NumberHelper
    {
        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?/d*[.]?\d*$");
        }
        public static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^-?[1-9]\d*$");
            //return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
        public static bool IsUnsign(string value)
        {
            return Regex.IsMatch(value, @"^/d*[.]?\d*$");
        }
    }
}
