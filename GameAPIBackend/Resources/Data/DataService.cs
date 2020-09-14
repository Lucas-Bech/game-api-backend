using Dapper;
using GameAPILibrary.APIMappers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GameAPILibrary.Resources.Data
{
    public class DataService : IDataService
    {
        public DataService()
        {

        }

        public event IDataService.LogEventHandler LogHandler;

        private void Log(string message)
        {
            LogHandler.Invoke(this, new LogEventArgs(message));
        }

        public async Task<App> GetAppFromCache(uint appId)
        {
            await Task.Yield();

            var result = new App();
            
            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app.id, app.name, d.id, d.name, p.id, p.name, app.coming_soon, app.release_date, ag.genre_id, ac.category_id" +
                    $" FROM app" +
                    $" LEFT JOIN developer d ON d.id = app.developer_id" +
                    $" LEFT JOIN publisher p ON p.id = app.publisher_id" +
                    $" LEFT JOIN app_genre ag ON ag.app_id = app.id" +
                    $" LEFT JOIN genre g ON g.id = ag.genre_id" +
                    $" LEFT JOIN app_category ac ON ac.app_id = app.id" +
                    $" LEFT JOIN category c ON c.id = ac.category_id" +
                    $" WHERE app.id = {appId}";
                    result = conn.Query<App>(sql).FirstOrDefault();
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            return result;
        }

        public async Task<App> GetDLCFromCache(uint appId)
        {
            await Task.Yield();

            var result = new App();

            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app.id, app.name, d.id, d.name, p.id, p.name, app.coming_soon, app.release_date, ag.genre_id, ac.category_id" +
                    $" FROM app" +
                    $" LEFT JOIN developer d ON d.id = app.developer_id" +
                    $" LEFT JOIN publisher p ON p.id = app.publisher_id" +
                    $" LEFT JOIN app_genre ag ON ag.app_id = app.id" +
                    $" LEFT JOIN genre g ON g.id = ag.genre_id" +
                    $" LEFT JOIN app_category ac ON ac.app_id = app.id" +
                    $" LEFT JOIN category c ON c.id = ac.category_id" +
                    $" INNER JOIN dlc ON dlc.app_id = app.id" +
                    $" WHERE app.id = {appId}";
                    result = conn.Query<App>(sql).FirstOrDefault();
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            return result;
        }

        public async Task<App> GetAppFromApi(uint appId)
        {
            string appDetailsUrl = $"https://store.steampowered.com/api/appdetails/?appids={appId}";
            string json = "";
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(appDetailsUrl);
                json = await response.Content.ReadAsStringAsync();

                dynamic data = JsonConvert.DeserializeObject(json);
                var jsonData = JsonConvert.SerializeObject(data[$"{appId}"]["data"]);
                AppDetails appDetails = JsonConvert.DeserializeObject<AppDetails>(jsonData);

                App app = appDetails.ToApp();

                return app;
            }
            catch (Exception ex) { Log(ex.Message); };
            return null;
        }

        public async Task<App> CacheApp(uint appId)
        {
            App app = await GetAppFromApi(appId);

            try
            {
                if (app is null)
                    throw new System.NullReferenceException("app cannot be null");

                //The SQL string variables we be referring to the tables that they are inserting to

                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string cacheApp = $"";
                    await conn.ExecuteAsync(cacheApp);
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            return null;
        }

        public async Task<List<DLC>> GetDLCs(uint appId, App baseApp = null)
        {
            List<DLC> dlc = new List<DLC>();
            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app_id as id FROM dlc" +
                                 $" WHERE base_app_id = {appId}";

                    dlc = (await conn.QueryAsync<DLC>(sql)).ToList();
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            if(!(baseApp is null))
            {
                foreach(DLC tempDLC in dlc)
                {
                    tempDLC.BaseApp = baseApp;
                }
            }

            return dlc;
        }
    }
}
