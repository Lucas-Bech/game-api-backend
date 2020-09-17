using GameAPILibrary.Resources;
using GameAPILibrary.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAPILibrary.APIMappers
{
    public class AppDetails
    {
        private uint _id;
        private string _name;
        private uint _requiredAge;
        private string _type;
        private List<string> _developers = new List<string>();
        private List<string> _publishers = new List<string>();
        private ReleaseInfo _releaseDate;
        private List<Genre> _genres = new List<Genre>();
        private List<Category> _categories = new List<Category>();
        private List<uint> _dlc = new List<uint>();

        [JsonProperty("steam_appid")]
        public uint Id { get => _id; set => _id = value; }

        [JsonProperty("name")]
        public string Name { get => _name; set => _name = value; }

        [JsonProperty("required_age")]
        public uint RequiredAge { get => _requiredAge; set => _requiredAge = value; }

        [JsonProperty("type")]
        public string Type { get => _type; set => _type = value; }

        [JsonProperty("developers")]
        public List<string> Developers { get => _developers; set => _developers = value; }

        [JsonProperty("publishers")]
        public List<string> Publishers { get => _publishers; set => _publishers = value; }

        [JsonProperty("release_date")]
        public ReleaseInfo ReleaseDate { get => _releaseDate; set => _releaseDate = value; }

        public List<Genre> Genres { get => _genres; set => _genres = value; }

        public List<Category> Categories { get => _categories; set => _categories = value; }

        [JsonProperty("dlc")]
        public List<uint> DLC { get => _dlc; set => _dlc = value; }


        public AppDetails()
        {

        }

        public override string ToString()
        {
            string result = "";

            result += $"Id: {Id}\n" +
                $"Name: {Name}\n";

            if (Developers?.Count > 0)
                result += $"Developer: {Developers.FirstOrDefault()}\n";

            if (Publishers?.Count > 0)
                result += $"Publisher: {Publishers.FirstOrDefault()}\n";

            result += $"Releasedate: {ReleaseDate.ToString()}";

            return result;
        }

        public App ToApp()
        {
            List<Developer> devs = new List<Developer>();
            foreach (string str in Developers)
            {
                Developer dev = new Developer(str);
                devs.Add(dev);
            }

            List<Publisher> pubs = new List<Publisher>();
            foreach (string str in Publishers)
            {
                Publisher pub = new Publisher(str);
                pubs.Add(pub);
            }

            App app = new App(Id, Name, devs, pubs, ReleaseDate, Genres, Categories);
            app.Type.Name = Type;

            foreach(uint i in DLC)
            {
                app.DLC.Add(new DLC(i));
            }

            app.RequiredAge = RequiredAge;

            return app;
        }
    }
}
