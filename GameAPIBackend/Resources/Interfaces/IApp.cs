using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public interface IApp
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public AppType Type { get; set; }

        public uint RequiredAge { get; set; }

        public List<Developer> Developers { get; set; }
        public List<string> DevelopersStr { get; }

        public List<Publisher> Publishers { get; set; }
        public List<string> PublishersStr { get; }

        public ReleaseInfo ReleaseDate { get; set; }

        public List<Genre> Genres { get; set; }
        public List<string> GenresStr { get; }

        public List<Category> Categories { get; set; }
        public List<string> CategoriesStr { get; }

        public string HeaderImage { get; set; }

        public uint ReviewScore { get; set; }

    }
}