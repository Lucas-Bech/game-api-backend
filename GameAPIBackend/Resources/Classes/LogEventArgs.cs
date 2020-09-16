using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public class LogEventArgs : EventArgs
    {
        private readonly string message;

        public LogEventArgs(string message)
        {
            this.message = message;
        }

        public string Message
        {
            get { return this.message; }
        }
    }
}
