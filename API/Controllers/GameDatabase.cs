using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
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
                return JsonConvert.SerializeObject(new { error404 = "NOT FOUND", Message = "Failed to cache app" });
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
                return JsonConvert.SerializeObject(new { error404 = "NOT FOUND", Message = "Failed to cache app" });
        }


        // POST api/gamedatabase/games/?like={parameter}
        [Microsoft.AspNetCore.Mvc.HttpGet("games/")]
        public async Task<string> Test([FromQuery(Name = "like")] string parameter)
        {
            var result = await _service.GetAppsFromCache(parameter);
            if(result.Count > 0)
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            else
                return JsonConvert.SerializeObject(new { error404 = "NOT FOUND", Message = "No apps matched the condition" });
        }
    }
}
