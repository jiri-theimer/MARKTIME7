using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PokusController : ControllerBase
    {
        [HttpGet(Name = "Ping")]
        public string Get()
        {
            
            var app = new BL.Singleton.RunningApp();

            return $"PING: {DateTime.Now}, app build: {app.AppBuild}, app name: {app.AppName}";



        }
    }
}
