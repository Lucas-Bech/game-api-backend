using GameAPILibrary.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameAPILibrary
{

    public class App : IApp
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
        private List<DLC> _dlc = new List<DLC>();

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

        [JsonProperty("dlc", Required = Required.Default)]
        public List<DLC> DLC { get => _dlc; set => _dlc = value; }

        [JsonProperty("dlc_ids", Required = Required.Default)]
        public List<uint> DLCIDs { get 
            {
                return DLC.Select(dlc => dlc.Id).ToList();
            }
        }

        [JsonIgnore]
        public bool SerializeDLC = false;
        public bool ShouldSerializeDLC()
        {
            return SerializeDLC;
        }

        [JsonIgnore]
        public bool SerializeDLCIDs = false;
        public bool ShouldSerializeDLCIDs()
        {
            return SerializeDLCIDs;
        }

        public App()
        {

        }

        public App(
            uint id,
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

        public override string ToString()
        {
            string result = "";

            result += $"Id: {Id}\n" +
                $"Name: {Name}\n";

            /*
            if (Developers?.Count > 0)
                result += $"Developer: {Developers.FirstOrDefault()}\n";

            if (Publishers?.Count > 0)
                result += $"Publisher: {Publishers.FirstOrDefault()}\n";
            */

            result += $"Releasedate: {ReleaseDate.ToString()}";

            return result;
        }
    }
}
