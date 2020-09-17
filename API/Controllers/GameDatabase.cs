using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GameAPILibrary;
using GameAPILibrary.Resources.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Controllers
{
    //api/gamedatabase/
    [Route("api/[controller]")]
    [ApiController]
    public class GameDatabase : ControllerBase
    {
        private IDataService _service;

        public GameDatabase(IDataService service)
            => _service = service;

        // GET: api/<GameDatabase>
        [HttpGet]
        public IEnumerable<string> Get()
        {

            _service.CacheApp(919640);


            return new string[] { "value1", "value2" };
        }

        // GET api/<GameDatabase>/game/?{id}&{dlc}
        [HttpGet("game/")]
        public async Task<string> Get(
            [FromQuery(Name = "id")] uint id,
            [FromQuery(Name = "dlc")] bool dlc = false)
        {
            var app = (App) await _service.GetAppFromCache(id);

            if (app is null)
                return "No App with specified ID";
            else
            {
                if (dlc)
                {
                    app.SerializeDLCIDs = true;
                    app.DLC = await _service.GetDLCsFromCache(app.Id);
                }

                return JsonConvert.SerializeObject(app, Formatting.Indented);
            }
        }

        // GET api/<GameDatabase>/dlc/?{id}
        [HttpGet("dlc/")]
        public async Task<string> Get([FromQuery(Name = "id")] uint id)
        {
            var app = (DLC) await _service.GetAppFromCache(id, true);

            if (app is null)
                return "No DLC with specified ID";
            else
                return JsonConvert.SerializeObject(app, Formatting.Indented);
        }


        // POST api/gamedatabase/test/{parameter}
        [Microsoft.AspNetCore.Mvc.HttpGet("test/{parameter}")]
        public async Task<string> Test(string parameter)
        {
            DataSet ds = new DataSet();
            List<App> result = new List<App>();
            string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = $"SELECT app.id FROM gameapi.app WHERE name LIKE '%{parameter}%'";

                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    //Saving it to adapter as to avoid doing two loops to get data.
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }

            using (DataTableReader reader = ds.CreateDataReader())
            {
                int x = 0;
                //Limit the results to 1000 games.
                while (reader.Read() && x < 1000)
                {
                    App app = (App) await _service.GetAppFromCache(Convert.ToUInt32(reader.GetValue(0)));
                    if(!(app is null))
                        result.Add(app);
                    x++;
                }
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }


        // POST api/<GameDatabase>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }



        // PUT api/<GameDatabase>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GameDatabase>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
