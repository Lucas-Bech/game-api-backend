using GameAPILibrary.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private string _headerImage;

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

        [JsonProperty("header_image")]
        public string HeaderImage { get => _headerImage; set => _headerImage = value; }

        public DLC()
        {

        }

        public DLC(uint id)
        {
            Id = id;
        }

        [JsonConstructor]
        public DLC(uint id,
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
    }
}
