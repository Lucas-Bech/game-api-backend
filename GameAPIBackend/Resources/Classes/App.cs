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
        private string _headerImage = "";
        private uint _reviewScore;

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

        [JsonIgnore]
        public List<Developer> Developers { get => _developers; set => _developers = value; }

        [JsonProperty("developers")]
        public List<string> DevelopersStr
        {
            get
            {
                if (_developers.Count > 0 && !(_developers[0] is null))
                    return _developers.Select(item => item.Name).ToList();
                else
                    return new List<string>();
            }
        }

        [JsonIgnore]
        public List<Publisher> Publishers { get => _publishers; set => _publishers = value; }

        [JsonProperty("publishers")]
        public List<string> PublishersStr
        {
            get
            {
                if (_publishers.Count > 0 && !(_publishers[0] is null))
                    return _publishers.Select(item => item.Name).ToList();
                else
                    return new List<string>();
            }
        }

        [JsonProperty("release_date")]
        public ReleaseInfo ReleaseDate { get => _releaseDate; set => _releaseDate = value; }

        [JsonIgnore]
        public List<Genre> Genres { get => _genres; set => _genres = value; }

        [JsonProperty("genres")]
        public List<string> GenresStr
        {
            get
            {
                if (_genres.Count > 0 && !(_genres[0] is null))
                    return _genres.Select(item => item.Name).ToList();
                else
                    return new List<string>();
            }
        }

        [JsonIgnore]
        public List<Category> Categories { get => _categories; set => _categories = value; }

        [JsonProperty("categories")]
        public List<string> CategoriesStr
        {
            get
            {
                if (_categories.Count > 0 && !(_categories[0] is null))
                    return _categories.Select(item => item.Name).ToList();
                else
                    return new List<string>();
            }
        }


        [JsonProperty("dlc")]
        public List<DLC> DLC { get => _dlc; set => _dlc = value; }

        [JsonProperty("dlc_ids")]
        public List<uint> DLCIDs { get 
            {
                return DLC.Select(dlc => dlc.Id).ToList();
            }
        }

        [JsonProperty("header_image")]
        public string HeaderImage { get => _headerImage; set => _headerImage = value; }

        [JsonProperty("review_score")]
        public uint ReviewScore { get => _reviewScore; set => _reviewScore = value; }

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
            List<Category> categories,
            string headerImage)
        {
            Id = id;
            Name = name;
            Developers = developers;
            Publishers = publishers;
            ReleaseDate = releaseDate;
            Genres = genres;
            Categories = categories;
            _headerImage = headerImage;
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
