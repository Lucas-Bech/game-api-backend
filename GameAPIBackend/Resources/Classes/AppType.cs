using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public class AppType
    {
        private uint _id;
        private string _name;

        public uint Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }

        public AppType()
        {

        }
    }
}
