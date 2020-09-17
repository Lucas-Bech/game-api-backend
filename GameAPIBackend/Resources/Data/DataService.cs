using Dapper;
using GameAPILibrary.APIMappers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                var types = new[] { typeof(App), typeof(AppType), typeof(Developer), typeof(Publisher), typeof(bool), typeof(DateTime?), typeof(Genre), typeof(Category) };
                
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app.id, app.name, app.required_age as requiredage, t.id as id, t.name, d.id, d.name, p.id, p.name, app.coming_soon, app.release_date, g.id as id, g.name, ac.category_id as id, c.name" +
                    $" FROM app" +
                    $" LEFT JOIN type t ON t.id = app.type_id" +
                    $" LEFT JOIN developer d ON d.id = app.developer_id" +
                    $" LEFT JOIN publisher p ON p.id = app.publisher_id" +
                    $" LEFT JOIN app_genre ag ON ag.app_id = app.id" +
                    $" LEFT JOIN genre g ON g.id = ag.genre_id" +
                    $" LEFT JOIN app_category ac ON ac.app_id = app.id" +
                    $" LEFT JOIN category c ON c.id = ac.category_id" +
                    $" WHERE app.id = {appId}";

                    //If user is requesting dlc, we make the first type to class DLC instead of APP
                    //And limit the query result to apps that exist in the dlc table
                    if (isDLC)
                    {
                        sql = $"{sql} AND EXISTS(SELECT app_id FROM dlc WHERE app_id = app.id);";
                        types[0] = typeof(DLC);
                    }

                    return (await conn.QueryAsync<IApp>(sql,
                        types,
                        (objects) =>
                        {
                            IApp app = (IApp)objects[0];
                            AppType type = (AppType)objects[1];
                            if (!(type is null))
                                app.Type = type;
                            app.Developers.Add((Developer)objects[2]);
                            app.Publishers.Add((Publisher)objects[3]);
                            app.ReleaseDate = new ReleaseInfo((bool)objects[4], (DateTime?)objects[5]);
                            app.Genres.Add((Genre)objects[6]);
                            app.Categories.Add((Category)objects[7]);
                            return app;
                        },
                        splitOn: "id, coming_soon, release_date, id, id")).FirstOrDefault();
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


        public async Task<bool> CacheIfOverdue(uint appID)
        {
            try
            {
                DateTime? date;
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    var param = new { appID = appID };
                    string sql = "SELECT last_caching_datetime FROM app WHERE id = @appID";
                    date = (await conn.QueryAsync<DateTime>(sql, param)).FirstOrDefault();
                }

                if(date < DateTime.Now.ToUniversalTime().AddHours(1))
                {
                    await CacheApp(appID);
                    return true;
                }
            }
            catch (Exception ex) { Log(ex.Message); }
            return false;
        }

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
                    AppType type = app.Type;
                    uint requiredAge = app.RequiredAge;
                    string developer = app.Developers[0].Name;
                    string publisher = app.Publishers[0].Name;
                    bool comingSoon = app.ReleaseDate.ComingSoon;
                    DateTime releaseDate = (DateTime) app.ReleaseDate.Date;
                    List<uint> dlcIDs = app.DLCIDs;
                    List<string> categories = app.Categories.Select(item => item.Name).ToList();
                    List<string> genres = app.Genres.Select(item => item.Name).ToList();


                    conn.Open();

                    //Update DLC relations
                    foreach (uint dlc in dlcIDs)
                    {
                        try
                        {
                            var parameters = new { appId = dlc, baseAppID = app.Id };
                            string cacheDLC = $"INSERT INTO dlc (app_id, base_app_id)" +
                                $" VALUES (@appId, @baseAppID) ON DUPLICATE KEY UPDATE app_id = @appID, base_app_id = @baseAppID";
                            await conn.ExecuteAsync(cacheDLC, parameters);
                        }
                        catch (Exception ex) { Log(ex.Message); }
                    }


                    //Update categories
                    foreach(string cat in categories)
                    {
                        try { 
                            var parameters = new { categoryName = cat };
                            string cacheCat = $"INSERT INTO category (name)" +
                                $" VALUES (@categoryName) ON DUPLICATE KEY UPDATE name = @categoryName";
                            await conn.ExecuteAsync(cacheCat, parameters);
                        }
                        catch (Exception ex) { Log(ex.Message); }
                    }

                    //Update category relations with apps
                    foreach (string cat in categories)
                    {
                        try
                        {
                            //Get ID for the category
                            var paramGet = new { categoryName = cat };
                            string getCatID = "SELECT id FROM category WHERE name = @categoryName";
                            int id = (await conn.QueryAsync<int>(getCatID, paramGet)).FirstOrDefault();


                            //Insert relation between app and category
                            var paramInsert = new { catID = id, appID = app.Id };
                            string cacheCategoryAppRelations = $"INSERT INTO app_category (app_id, category_id)" +
                                $" VALUES (@appID, @catID) ON DUPLICATE KEY UPDATE app_id = @appID";
                            await conn.ExecuteAsync(cacheCategoryAppRelations, paramInsert);
                        }
                        catch(Exception ex) { Log(ex.Message); }
                    }


                    //update genres
                    foreach (string gen in genres)
                    {
                        try
                        {
                            var parameters = new { genreName = gen };
                            string cacheGen = $"INSERT INTO genre (name)" +
                                $" VALUES (@genreName) ON DUPLICATE KEY UPDATE name = @genreName";
                            await conn.ExecuteAsync(cacheGen, parameters);
                        }
                        catch (Exception ex) { Log(ex.Message); }
                    }

                    //Update genre relations with apps
                    foreach (string gen in genres)
                    {
                        try
                        {
                            //Get ID for the genre
                            var paramGet = new { genreName = gen };
                            string getGenID = "SELECT id FROM genre WHERE name = @genreName";
                            int id = (await conn.QueryAsync<int>(getGenID, paramGet)).FirstOrDefault();


                            //Insert relation between app and genre
                            var paramInsert = new { genID = id, appID = app.Id };
                            string cacheCategoryAppRelations = $"INSERT INTO app_genre (app_id, genre_id)" +
                                $" VALUES (@appID, @genID) ON DUPLICATE KEY UPDATE app_id = @appID";
                            await conn.ExecuteAsync(cacheCategoryAppRelations, paramInsert);
                        }
                        catch (Exception ex) { Log(ex.Message); }
                    }


                    //Update types
                    try
                    {
                        var parameters = new { typeName = type.Name };
                        string cacheGen = $"INSERT INTO type (name)" +
                            $" VALUES (@typeName) ON DUPLICATE KEY UPDATE name = @typeName";
                        await conn.ExecuteAsync(cacheGen, parameters);

                        string getTypeIDSql = "SELECT id FROM type WHERE name = @typeName";
                        int id = (await conn.QueryAsync<int>(getTypeIDSql, parameters)).FirstOrDefault();

                        var paramApp = new { appID = app.Id, typeID = id };
                        string updateAppTypeSql = "UPDATE app SET type_id = @typeID WHERE id = @appID ";
                        await conn.ExecuteAsync(updateAppTypeSql, paramApp);
                    }
                    catch (Exception ex) { Log(ex.Message); }


                    //update developer
                    try
                    {
                        var parameters = new { devName = developer };
                        string cacheDev = $"INSERT INTO developer (name)" +
                            $" VALUES (@devName) ON DUPLICATE KEY UPDATE name = @devName";
                        await conn.ExecuteAsync(cacheDev, parameters);

                        string getDevIDSql = "SELECT id FROM developer WHERE name = @devName";
                        int id = (await conn.QueryAsync<int>(getDevIDSql, parameters)).FirstOrDefault();

                        var paramDev = new { appID = app.Id, devID = id };
                        string updateAppDevSql = "UPDATE app SET developer_id = @devID WHERE id = @appID ";
                        await conn.ExecuteAsync(updateAppDevSql, paramDev);
                    }
                    catch (Exception ex) { Log(ex.Message); }


                    //update publisher
                    try
                    {
                        var parameters = new { pubName = publisher };
                        string cachePub = $"INSERT INTO publisher (name)" +
                            $" VALUES (@pubName) ON DUPLICATE KEY UPDATE name = @pubName";
                        await conn.ExecuteAsync(cachePub, parameters);

                        string getPubIDSql = "SELECT id FROM publisher WHERE name = @pubName";
                        int id = (await conn.QueryAsync<int>(getPubIDSql, parameters)).FirstOrDefault();

                        var paramPub = new { appID = app.Id, pubID = id };
                        string updatePubSql = "UPDATE app SET publisher_id = @pubID WHERE id = @appID ";
                        await conn.ExecuteAsync(updatePubSql, paramPub);
                    }
                    catch (Exception ex) { Log(ex.Message); }


                    //update release date
                    try
                    {
                        var parameters = new { comingSoon = comingSoon, date = releaseDate, appID = app.Id };
                        string cacheRel = $"UPDATE app SET coming_soon = @comingSoon," +
                            $" release_date = @date WHERE id = @appID";
                        await conn.ExecuteAsync(cacheRel, parameters);
                    }
                    catch (Exception ex) { Log(ex.Message); }


                    //update required age
                    try
                    {
                        var parameters = new { age = app.RequiredAge, appID = app.Id };
                        string cacheRel = $"UPDATE app SET required_age = @age WHERE id = @appID";
                        await conn.ExecuteAsync(cacheRel, parameters);
                    }
                    catch (Exception ex) { Log(ex.Message); }
                }
                return true;
            }
            catch (Exception ex) { Log(ex.Message); };

            return false;
        }

    }
}
