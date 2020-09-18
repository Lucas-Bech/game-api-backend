using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary
{
    public class Genre
    {
        private uint _id;
        private string _name;

        [JsonIgnore]
        public uint Id { get => _id; set => _id = value; }
        [JsonProperty("description")]
        public string Name { get => _name; set => _name = value; }

        public Genre()
        {

        }

        public Genre(uint id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
