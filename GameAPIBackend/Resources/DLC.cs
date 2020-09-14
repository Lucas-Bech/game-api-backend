using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary
{
    public class DLC
    {
        private uint _id;
        //The App that owns the DLC
        private App _baseApp = null;

        [JsonIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public App BaseApp { get => _baseApp; set => _baseApp = value; }

        [JsonProperty("base_app_id", NullValueHandling = NullValueHandling.Ignore)]
        public uint? BaseAppId
        {
            get
            {
                if (!(BaseApp is null))
                    return BaseApp.Id;
                else
                    return null;
            }
        }

        [JsonProperty("id")]
        public uint Id { get => _id; set => _id = value; }

        public DLC()
        {

        }

        public DLC(uint id, App baseApp)
        {
            Id = id;
            BaseApp = baseApp;
        }
    }
}
