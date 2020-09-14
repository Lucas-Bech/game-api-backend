using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        public async Task<App> GetApp(uint appId)
        {
            await Task.Yield();

            var result = new App();
            
            try
            {
                string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
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
                    result = connection.Query<App>(sql).FirstOrDefault();
                }
            }
            catch (Exception ex) { Log(ex.Message); };

            return result;
        }

        public async Task<bool> CacheApp(uint appId)
        {
            string appDetailsUrl = $"https://store.steampowered.com/api/appdetails/?appids={appId}";
            string json = "";
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(appDetailsUrl);
                json = await response.Content.ReadAsStringAsync();


                dynamic data = JsonConvert.DeserializeObject(json);
                var test = JsonConvert.SerializeObject(data["50"]);
                App app = JsonConvert.DeserializeObject<App>(test);

                /*
                var obj = JObject.Parse(json);
                string test = (string) obj[$"{appId}"]["success"];





                
                var test = JsonConvert.DeserializeObject<App>(json);
                var name = test.Name;
                */
            }
            catch (Exception ex) { Log(ex.Message); };
            return true;
        }
    }
}
