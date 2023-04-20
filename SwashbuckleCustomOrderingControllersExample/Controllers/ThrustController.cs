using Microsoft.AspNetCore.Mvc;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

namespace SwashbuckleCustomOrderingControllersExample.Controllers
{
    /// <summary>
    /// Thrust theater controller.
    /// </summary>
    [ApiController]
    [Route("theater/[controller]")]
    [SwaggerControllerOrder(2)]
    public class ThrustController : ControllerBase
    {
        private readonly ILogger<ThrustController> logger;

        /// <summary>
        /// Thrust controller constructor.
        /// </summary>
        /// <param name="logger">A generic interface for logging where the category name is derived from the specified TCategoryName type name used to enable activation of a named ILogger from dependency injection.</param>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public ThrustController(ILogger<ThrustController> logger, IConfiguration configuration)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Get information about the theater type.
        /// </summary>
        /// <returns>Information about the theater type.</returns>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(new { type = "Thrust" });
        }
    }
}
