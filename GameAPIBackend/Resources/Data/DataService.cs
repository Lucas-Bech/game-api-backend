using Dapper;
using GameAPILibrary.APIMappers;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.CRUD;
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
            if(!(LogHandler is null))
                LogHandler.Invoke(this, new LogEventArgs(message));
        }

        public async Task<IApp> GetAppFromCache(uint appId, bool asDLC = false)
        {
            string condition = "WHERE app.id = @AppID";
            var param = new { AppID = appId };
            return (await GetApps(condition, param, asDLC)).FirstOrDefault();
        }

        public async Task<List<App>> GetAppsFromCache(string input, uint limit)
        {
            string condition = "WHERE app.name LIKE @value";
            var param = new { value = $"%{input}%"};
            return (await GetApps(condition, param, limit: limit)).Select(x => (App)x).ToList();
        }

        ///<summary>
        ///Used as a generalist method for getting apps from cache
        ///</summary>
        private async Task<List<IApp>> GetApps(string sqlCondition, object param, bool asDLC = false, uint limit = 0)
        {
            try
            {
                if (sqlCondition == "" || sqlCondition is null)
                    return new List<IApp>();

                var types = new[] { typeof(App), typeof(AppType), typeof(Developer), typeof(Publisher), typeof(bool), typeof(DateTime?), typeof(string), typeof(string) };
                if (asDLC)
                    types[0] = typeof(DLC);

                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");

                //Cache if limit is 5 or lower. 0 means no limit
                if (limit <= 5 && limit > 0)
                {
                    List<uint> appIDs = new List<uint>();
                    using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                    {
                        string sql = $"SELECT id FROM app {sqlCondition} LIMIT {limit}";
                        appIDs = (await conn.QueryAsync<uint>(sql, param)).ToList();
                    }

                    foreach (uint app in appIDs)
                    {
                        await CacheIfOverdue(app);
                    }
                }

                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app.id, app.name, app.header_image as HeaderImage, app.required_age as requiredage, app.review_score as reviewscore, t.id as id, t.name, d.id, d.name, p.id, p.name, app.coming_soon, app.release_date," +
                    $" (SELECT GROUP_CONCAT(CONCAT(c.id, ' ', c.name) SEPARATOR ',') FROM category c" +
                    $" WHERE EXISTS(SELECT ac.app_id FROM app_category ac WHERE ac.app_id = app.id AND c.id = ac.category_id)) as categories," +
                    $" (SELECT GROUP_CONCAT(CONCAT(g.id, ' ', g.name) SEPARATOR ',') FROM genre g" +
                    $" WHERE EXISTS(SELECT ag.app_id FROM app_genre ag WHERE ag.app_id = app.id AND g.id = ag.genre_id)) as genres" +
                    $" FROM app" +
                    $" LEFT JOIN type t ON t.id = app.type_id" +
                    $" LEFT JOIN developer d ON d.id = app.developer_id" +
                    $" LEFT JOIN publisher p ON p.id = app.publisher_id" +
                    $" {sqlCondition}";
                    if(limit > 0)
                        sql += $" LIMIT {limit}";

                    var apps = (await conn.QueryAsync<IApp>(sql,
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

                            //Create category objects
                            if (!(objects[6] is null))
                            {
                                var categories = objects[6].ToString().Split(",");
                                foreach (string str in categories)
                                {
                                    var data = str.Split(" ", 2);
                                    var id = Convert.ToUInt32(data[0]);
                                    var name = data[1];
                                    app.Categories.Add(new Category(id, name));
                                }
                            }

                            //Create genre objects
                            if (!(objects[7] is null))
                            {
                                var genres = objects[7].ToString().Split(",");
                                foreach (string str in genres)
                                {
                                    var data = str.Split(" ", 2);
                                    var id = Convert.ToUInt32(data[0]);
                                    var name = data[1];
                                    app.Genres.Add(new Genre(id, name));
                                }
                            }

                            return app;
                        },
                        splitOn: "id, coming_soon, release_date, categories, genres", param: param)).ToList();

                    return apps;
                }
            }
            catch (Exception ex) { Log(ex.Message); }
            return new List<IApp>();
        }

        public async Task<List<DLC>> GetDLCsFromCache(uint appID)
        {
            List<DLC> dlc = new List<DLC>();
            List<uint> dlcIDs = new List<uint>();
            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    string sql = $"SELECT app_id as id FROM dlc" +
                                 $" WHERE base_app_id = {appID}";

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

        private async Task<App> GetAppFromApi(uint appID)
        {
            string appDetailsUrl = $"https://store.steampowered.com/api/appdetails/?appids={appID}";
            string reviewURL = $"https://store.steampowered.com/appreviews/{appID}?json=1&num_per_page=0";
            string jsonDetails = "";
            string jsonReview = "";
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage responseDetails = await client.GetAsync(appDetailsUrl);
                jsonDetails = await responseDetails.Content.ReadAsStringAsync();

                HttpResponseMessage responseReview = await client.GetAsync(reviewURL);
                jsonReview = await responseReview.Content.ReadAsStringAsync();

                dynamic data = JsonConvert.DeserializeObject(jsonDetails);
                var jsonData = JsonConvert.SerializeObject(data[$"{appID}"]["data"]);
                AppDetails appDetails = JsonConvert.DeserializeObject<AppDetails>(jsonData);

                uint reviewScore = 0;
                data = JsonConvert.DeserializeObject(jsonReview);
                if (data.ToString().Contains("query_summary")){
                    jsonData = JsonConvert.SerializeObject(data["query_summary"]["review_score"]);
                    reviewScore = JsonConvert.DeserializeObject<uint>(jsonData);
                }

                if (!(appDetails is null))
                {
                    var app = appDetails.ToApp();
                    app.ReviewScore = reviewScore;
                    return app;
                }

                return null;
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

                DateTime toCompare = (DateTime) date;

                if (toCompare.AddHours(1) < DateTime.Now.ToUniversalTime())
                    await CacheApp(appID);

                return true;
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

                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    uint appID = app.Id;
                    string name = app.Name;
                    AppType type = app.Type;
                    uint requiredAge = app.RequiredAge;
                    string developer = app.Developers.Count > 0 ? app.Developers[0].Name : "";
                    string publisher = app.Publishers.Count > 0 ? app.Publishers[0].Name : "";
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
                        var parameters = new { age = app.RequiredAge, appID = app.Id, reviewScore = app.ReviewScore };
                        string cacheRel = $"UPDATE app" +
                            $" SET required_age = @age," +
                            $" review_score = @reviewScore" +
                            $" WHERE id = @appID";
                        await conn.ExecuteAsync(cacheRel, parameters);
                    }
                    catch (Exception ex) { Log(ex.Message); }


                    //Update header image
                    try
                    {
                        var parameters = new { headerImage = app.HeaderImage, appID = app.Id };
                        string cacheHeader = $"UPDATE app SET  WHERE id = @appID";
                        await conn.ExecuteAsync(cacheHeader, parameters);
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
