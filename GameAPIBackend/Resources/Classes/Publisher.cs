using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public class Publisher
    {
        private string _name;
        private uint _id;

        [JsonProperty("id")]
        public uint Id { get => _id; set => _id = value; }
        [JsonProperty("name")]
        public string Name { get => _name; set => _name = value; }

        public Publisher()
        {

        }

        public Publisher(string name)
        {
            Name = name;
        }
        public Publisher(uint id, string name)
        {
            _id = id;
            _name = name;
        }
    }
}
