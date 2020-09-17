using GameAPILibrary.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public class ReleaseInfo
    {
        private bool _comingSoon;
        private DateTime? date;

        [JsonProperty("coming_soon")]
        public bool ComingSoon { get => _comingSoon; set => _comingSoon = value; }

        [JsonProperty("date")]
        public DateTime? Date { get => date; set => date = value; }

        [JsonConstructor]
        public ReleaseInfo(bool comingSoon, string date)
        {
            ComingSoon = comingSoon;
            Date = DateUtils.SteamDateToDateTime(date);
        }

        public ReleaseInfo(bool comingSoon, DateTime date)
        {
            ComingSoon = comingSoon;
            Date = date;
        }

        public ReleaseInfo(bool comingSoon, DateTime? date)
        {
            ComingSoon = comingSoon;
            Date = date;
        }
    }
}
