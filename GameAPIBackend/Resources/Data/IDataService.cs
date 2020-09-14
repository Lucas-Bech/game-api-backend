using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameAPILibrary.Resources.Data
{
    public interface IDataService
    {
        public delegate void LogEventHandler(
        object sender,
        LogEventArgs args);
        public event LogEventHandler LogHandler;

        ///<summary>
        ///Returns App with specified ID if exists
        ///</summary>
        public Task<App> GetApp(uint appId);
        ///<summary>
        ///Caches data to the database
        ///</summary>
        public Task<bool> CacheApp(uint appId);
    }
}
