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
        public Task<App> GetAppFromCache(uint appId);
        ///<summary>
        ///Returns DLC with specified ID if exists
        ///</summary>
        public Task<App> GetDLCFromCache(uint appId);
        ///<summary>
        ///Caches data to the database
        ///</summary>
        public Task<App> CacheApp(uint appId);
        ///<summary>
        ///Gets DLC ID's for specified app
        ///</summary>
        public Task<List<DLC>> GetDLCs(uint appId, App baseApp = null);
    }
}
