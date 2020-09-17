using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GameAPILibrary.Utilities
{
    public static class DateUtils
    {
        public static DateTime SteamDateToDateTime(string steamDate)
        {
            try
            {
                //Steam doesn't always provide the wanted format, so we do multiple checks
                
                if (steamDate.Contains(','))
                    steamDate = steamDate.Replace(",", "");

                if (steamDate.Split(" ")[0].Length < 2)
                    steamDate = "0" + steamDate;

                try
                {
                    var date = DateTime.ParseExact(steamDate, "dd MMM yyyy", CultureInfo.InvariantCulture);
                    return date;
                }
                catch (Exception ex) { };

                try
                {
                    var date = DateTime.ParseExact(steamDate, "dd MMMM yyyy", CultureInfo.InvariantCulture);
                    return date;
                }
                catch (Exception ex) { };
            }
            catch (Exception ex) { };
            return DateTime.MinValue;
        }
    }
}
