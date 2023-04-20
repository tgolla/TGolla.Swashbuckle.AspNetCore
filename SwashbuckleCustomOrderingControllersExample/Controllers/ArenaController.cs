using Microsoft.AspNetCore.Mvc;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

namespace SwashbuckleCustomOrderingControllersExample.Controllers
{
    /// <summary>
    /// Arena  theater controller.
    /// </summary>
    [ApiController]
    [Route("theater/[controller]")]
    [SwaggerControllerOrder(3)]
    public class ArenaController : ControllerBase
    {
        private readonly ILogger<ArenaController> logger;

        /// <summary>
        /// Arena controller constructor.
        /// </summary>
        /// <param name="logger">A generic interface for logging where the category name is derived from the specified TCategoryName type name used to enable activation of a named ILogger from dependency injection.</param>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public ArenaController(ILogger<ArenaController> logger, IConfiguration configuration)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Get information about the theater type.
        /// </summary>
        /// <returns>Information about the theater type.</returns>
        [HttpGet ("info")]
        public IActionResult GetInfo()
        {
            return Ok(new { type = "Arena" });
        }
    }
}
