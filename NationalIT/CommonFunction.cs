using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NationalIT
{
    public class CommonFunction
    {
        public static string getYesNO(bool value)
        {
            if (value)
            {
                return "YES";
            }
            return "NO";
        }
        public static DateTime? ChangeFormatDate(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return null;
            }
            string[] t = date.Trim().Split(new char[]{'/'},StringSplitOptions.RemoveEmptyEntries);

            return new DateTime(int.Parse(t[2]), int.Parse(t[0]), int.Parse(t[1]));
        }
    }
}