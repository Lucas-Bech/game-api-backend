﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GameAPILibrary;
using GameAPILibrary.Resources.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
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
            if (await _service.CacheIfOverdue(id))
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
            else
            {
                return JsonConvert.SerializeObject(new { error404 = "NOT FOUND", Message = "Failed to cache app" });
            }
        }

        // GET api/<GameDatabase>/dlc/?{id}
        [HttpGet("dlc/")]
        public async Task<string> Get([FromQuery(Name = "id")] uint id)
        {
            if (await _service.CacheIfOverdue(id))
            {
                var app = (DLC)await _service.GetAppFromCache(id, true);

                if (app is null)
                    return "No DLC with specified ID";
                else
                    return JsonConvert.SerializeObject(app, Formatting.Indented);
            }
            else
            {
                return JsonConvert.SerializeObject(new { error404 = "NOT FOUND", Message = "Failed to cache app" });
            }
        }


        // POST api/gamedatabase/games/?like={parameter}
        [Microsoft.AspNetCore.Mvc.HttpGet("games/")]
        public async Task<string> Test([FromQuery(Name = "like")] string parameter)
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
                //Limit the results to 2000 games.
                while (reader.Read() && x < 2000)
                {
                    App app = (App) await _service.GetAppFromCache(Convert.ToUInt32(reader.GetValue(0)));
                    if(!(app is null))
                        result.Add(app);
                    x++;
                }
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }
    }
}
