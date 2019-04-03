using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Extensions
{
    public static class MethodsEx
    {
        public static string GetCurrentTimeString()
        {
            var time = DateTime.Now;
            return time.Year.ToString("D4") +
                time.Month.ToString("D2") +
                time.Day.ToString("D2") +
                time.Hour.ToString("D2") +
                time.Minute.ToString("D2") +
                time.Second.ToString("D2") +
                time.Millisecond.ToString("D4");
        }

        //0123456789012345678901234567890123456
        //ac699058-c26f-46a6-8280-5c4d12da6c66
        public static string RemoveGuidDelimiters(this string str)
        {
            return str.Remove(8, 1).Remove(12, 1).Remove(16, 1).Remove(20, 1);
        }
        public static string AddGuidDelimiters(this string str)
        {
            return str.Substring(0, 8) + "-" +
                str.Substring(8, 4) + "-" +
                str.Substring(12, 4) + "-" +
                str.Substring(16, 4) + "-" +
                str.Substring(20, 12); 
        }
    }
}
