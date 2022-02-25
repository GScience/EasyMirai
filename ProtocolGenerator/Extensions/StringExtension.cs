using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolGenerator.Extensions
{
    internal static class StringExtension
    {
        public static string FirstToUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.First().ToString().ToUpper() + (str.Length > 1 ? str[1..] : "");
        }
    }
}
