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

        public async Task<IApp> GetAppFromCache(uint appId, bool isDLC = false)
        {
            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app.id, app.name, d.id, d.name, p.id, p.name, app.coming_soon, app.release_date, g.id as id, g.name, ac.category_id as id, c.name" +
                    $" FROM app" +
                    $" LEFT JOIN developer d ON d.id = app.developer_id" +
                    $" LEFT JOIN publisher p ON p.id = app.publisher_id" +
                    $" LEFT JOIN app_genre ag ON ag.app_id = app.id" +
                    $" LEFT JOIN genre g ON g.id = ag.genre_id" +
                    $" LEFT JOIN app_category ac ON ac.app_id = app.id" +
                    $" LEFT JOIN category c ON c.id = ac.category_id" +
                    $" WHERE app.id = {appId}";

                    if (isDLC)
                    {
                        sql = $"{sql} AND EXISTS(SELECT app_id FROM dlc WHERE app_id = app.id);";
                        return (await conn.QueryAsync<DLC, Developer, Publisher, bool, DateTime?, Genre, Category, DLC>(sql,
                            (app, dev, pub, comingSoon, releaseDate, genre, category) =>
                            {
                                app.Developers.Add(dev);
                                app.Publishers.Add(pub);
                                app.ReleaseDate = new ReleaseInfo(comingSoon, releaseDate);
                                app.Genres.Add(genre);
                                app.Categories.Add(category);
                                return app;
                            },
                            splitOn: "id, coming_soon, release_date, id, id")).FirstOrDefault();
                    }
                    else
                    {
                        return (await conn.QueryAsync<App, Developer, Publisher, bool, DateTime?, Genre, Category, App>(sql,
                            (app, dev, pub, comingSoon, releaseDate, genre, category) =>
                            {
                                app.Developers.Add(dev);
                                app.Publishers.Add(pub);
                                app.ReleaseDate = new ReleaseInfo(comingSoon, releaseDate);
                                app.Genres.Add(genre);
                                app.Categories.Add(category);
                                return app;
                            },
                            splitOn: "id, coming_soon, release_date, id, id")).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            return null;
        }

        public async Task<List<DLC>> GetDLCsFromCache(uint appId)
        {
            List<DLC> dlc = new List<DLC>();
            List<uint> dlcIDs = new List<uint>();
            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app_id as id FROM dlc" +
                                 $" WHERE base_app_id = {appId}";

                    dlcIDs = (await conn.QueryAsync<uint>(sql)).ToList();
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            foreach (uint dlcID in dlcIDs)
            {
                dlc.Add((DLC)(await GetAppFromCache(dlcID, true)));
            }
            return dlc;
        }

        private async Task<App> GetAppFromApi(uint appId)
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

        //unfinished, missing caching of certain values
        public async Task<bool> CacheApp(uint appId)
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
                    uint appID = app.Id;
                    string name = app.Name;
                    List<uint> dlcIDs = app.DLCIDs;
                    uint developerID = app.Developers[0].Id;
                    uint publisherID = app.Publishers[0].Id;
                    bool comingSoon = app.ReleaseDate.ComingSoon;
                    DateTime releaseDate = (DateTime) app.ReleaseDate.Date;

                    string cacheApp = $"";
                    await conn.ExecuteAsync(cacheApp);
                }
                return true;
            }
            catch (Exception ex) { Log(ex.Message); };

            return false;
        }

    }
}
