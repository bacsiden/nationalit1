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
    }
}