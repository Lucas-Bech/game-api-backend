using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameAPILibrary.Utilities
{
    public static class DateUtils
    {
        public static DateTime SteamDateToDateTime(string steamDate)
        {
            //Steam doesn't always provide the wanted format
            //This will add a 0 to the date if the date is 1-9 (1, 2, 3 etc.)
            //Such that they become (01, 02, 03 etc.)
            if (steamDate.Split(" ")[0].Length < 2)
                steamDate = "0" + steamDate;

            var date = DateTime.ParseExact(steamDate, "dd MMM, yyyy", CultureInfo.InvariantCulture);
            return date;
        }
    }
}
