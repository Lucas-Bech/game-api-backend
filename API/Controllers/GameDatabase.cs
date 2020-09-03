using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    //api/gamedatabase/
    [Route("api/[controller]")]
    [ApiController]
    public class GameDatabase : ControllerBase
    {
        // GET: api/<GameDatabase>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<GameDatabase>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return $"{id}";
        }


        // POST api/gamedatabase/test/{parameter}
        [HttpGet("test/{parameter}")]
        public string Test(string parameter)
        {
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
                        result += $"{reader.GetValue(0)}\n";
                    }
                }
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
