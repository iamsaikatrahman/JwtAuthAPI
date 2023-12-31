using JwtAuthAPI.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        [Route("Get")]
        public IActionResult Get()
        {
            return Ok(Summaries);
        }
        
        [HttpGet]
        [Route("GetUserRole")]
        [Authorize(Roles = ApplicationConstant.USER)]
        public IActionResult GetUserRole()
        {
            return Ok(Summaries);
        }
        
        [HttpGet]
        [Route("GetAdminRole")]
        [Authorize(Roles = ApplicationConstant.ADMIN)]
        public IActionResult GetAdminRole()
        {
            return Ok(Summaries);
        }
        
        [HttpGet]
        [Route("GetOwnerRole")]
        [Authorize(Roles = ApplicationConstant.OWNER)]
        public IActionResult GetOwnerRole()
        {
            return Ok(Summaries);
        }
    }
}