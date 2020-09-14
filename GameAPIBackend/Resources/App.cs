using GameAPILibrary.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameAPILibrary
{
    public class App
    {
        private uint _id;
        private string _name;
        private List<Developer> _developers;
        private List<Publisher> _publishers;
        private bool _comingSoon;
        private DateTime _releaseDate;
        private List<Genre> _genres;
        private List<Category> _categories;
        private List<Reviewer> _publisher;
        public uint Id { get => _id; set => _id = value; }
        [JsonProperty("data.name")]
        public string Name { get => _name; set => _name = value; }
        public List<Developer> Developers { get => _developers; set => _developers = value; }
        public List<Publisher> Publishers { get => _publishers; set => _publishers = value; }
        public bool ComingSoon { get => _comingSoon; set => _comingSoon = value; }
        public DateTime ReleaseDate { get => _releaseDate; set => _releaseDate = value; }
        public List<Genre> Genres { get => _genres; set => _genres = value; }
        public List<Category> Categories { get => _categories; set => _categories = value; }

        public App()
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
    }
}
