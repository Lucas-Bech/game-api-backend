using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public class Developer
    {
        private string _name;
        private uint _id;
        public Developer(uint id, string name)
        {
            _id = id;
            _name = name;
        }
    }
}
