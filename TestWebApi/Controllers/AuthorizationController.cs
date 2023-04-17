using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

namespace TestWebApi.Controllers
{
    /// <summary>
    /// The authorization test API calls.
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Test")]
        [Authorize(Policy = "Manager")]
        public IActionResult Test() { return Ok(); }
    }
}
