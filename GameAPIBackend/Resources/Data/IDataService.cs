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
        ///If isDLC is set to true, returns DLC
        ///</summary>
        public Task<IApp> GetAppFromCache(uint appId, bool isDLC = false);

        ///<summary>
        ///Returns Apps with a name %like% input
        ///</summary>
        public Task<List<App>> GetAppsFromCache(string input, uint limit);

        ///<summary>
        ///Gets DLCs for specified appId. If baseApp is passed the DLC's will contain a reference to said app
        ///</summary>
        public Task<List<DLC>> GetDLCsFromCache(uint appId);

        ///<summary>
        ///Caches data if caching is overdue by using the CacheApp method
        ///</summary>
        public Task<bool> CacheIfOverdue(uint appId);

        ///<summary>
        ///Caches data related to specified app to the database
        ///</summary>
        public Task<bool> CacheApp(uint appId);
    }
}
