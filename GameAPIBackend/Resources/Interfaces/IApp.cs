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

        public List<Developer> Developers { get; set; }

        public List<Publisher> Publishers { get; set; }

        public ReleaseInfo ReleaseDate { get; set; }

        public List<Genre> Genres { get; set; }

        public List<Category> Categories { get; set; }
    }
}
