using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
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

            _service.CacheApp(50);


            return new string[] { "value1", "value2" };
        }

        // GET api/<GameDatabase>/5
        [HttpGet("{id}")]
        public async Task<string> Get(uint id)
        {
            var app = await _service.GetApp(id);

            if (app is null)
                return "No App with specified ID";
            else
                return JsonConvert.SerializeObject(app);
        }


        // POST api/gamedatabase/test/{parameter}
        [HttpGet("test/{parameter}")]
        public async Task<string> Test(string parameter)
        {
            List<uint> AppIds = new List<uint>();
            string result = ""; 
            string ConnectionString = ConfigurationManager.AppSettings.Get("connectionstring");
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = $"SELECT app.id FROM gameapi.app WHERE name LIKE '%{parameter}%'";
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();

                    while(reader.Read())
                    {
                        AppIds.Add(Convert.ToUInt32(reader.GetValue(0)));
                    }
                }
            }

            foreach (uint i in AppIds)
            {
                var app = await _service.GetApp(i);
                result += $"{app.ToString()}\n\n";
            }
            return result;
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
