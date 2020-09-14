using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary
{
    public class DLC : App
    {
        //The App that owns the DLC
        private App _baseApp;
        public App BaseApp { get => _baseApp; set => _baseApp = value; }

        public DLC()
        {

        }
    }
}
