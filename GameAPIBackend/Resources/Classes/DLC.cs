using GameAPILibrary.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameAPILibrary
{
    public class DLC : IApp
    {
        private uint _id;
        private string _name;
        private AppType _type = new AppType();
        private uint requiredAge;
        private List<Developer> _developers = new List<Developer>();
        private List<Publisher> _publishers = new List<Publisher>();
        private ReleaseInfo _releaseDate;
        private List<Genre> _genres = new List<Genre>();
        private List<Category> _categories = new List<Category>();
        private App _baseApp = null;

        [JsonProperty("steam_appid")]
        public uint Id { get => _id; set => _id = value; }

        [JsonProperty("name")]
        public string Name { get => _name; set => _name = value; }

        [JsonIgnore]
        public AppType Type { get => _type; set => _type = value; }

        [JsonProperty("type")]
        public string TypeName { get => _type.Name; }

        [JsonProperty("required_age")]
        public uint RequiredAge { get => requiredAge; set => requiredAge = value; }

        [JsonProperty("developers")]
        public List<Developer> Developers { get => _developers; set => _developers = value; }

        [JsonProperty("publishers")]
        public List<Publisher> Publishers { get => _publishers; set => _publishers = value; }

        [JsonProperty("release_date")]
        public ReleaseInfo ReleaseDate { get => _releaseDate; set => _releaseDate = value; }

        [JsonProperty("genres")]
        public List<Genre> Genres { get => _genres; set => _genres = value; }

        [JsonProperty("categories")]
        public List<Category> Categories { get => _categories; set => _categories = value; }


        [JsonIgnore]
        public App BaseApp { get => _baseApp;}

        //If baseapp isnt defined, it will return null. 
        //Newtonsoft Json will then ignore the property
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

        public DLC()
        {

        }

        public DLC(uint id)
        {
            Id = id;
        }

        public DLC(uint id, App baseApp)
        {
            Id = id;
            SetBaseApp(baseApp);
        }

        [JsonConstructor]
        public DLC(uint id,
            string name,
            List<Developer> developers,
            List<Publisher> publishers,
            ReleaseInfo releaseDate,
            List<Genre> genres,
            List<Category> categories)
        {
            Id = id;
            Name = name;
            Developers = developers;
            Publishers = publishers;
            ReleaseDate = releaseDate;
            Genres = genres;
            Categories = categories;
        }

        public void SetBaseApp(App baseApp)
        {
            _baseApp = baseApp;
        }
    }
}
