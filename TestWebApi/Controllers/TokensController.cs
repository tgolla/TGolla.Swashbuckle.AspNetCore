using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

namespace TestWebApi.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    [SwaggerControllerOrder(0)]
    public class TokensController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Test")]
        public IActionResult Test() { return Ok(); }
    }
}
