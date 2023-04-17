using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TGolla.AspNetCore.Mvc.Filters;
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
        /// Test for an authenticated user. No group privileges needed.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestForAnAuthenticatedUser")]
        public IActionResult TestForAnAuthenticatedUser() { return Ok(); }

        /// <summary>
        /// Test for a user with administrator privileges.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestForAnAdministrator")]
        [Authorize(Policy = "Administrator")]
        public IActionResult TestForAnAdministrator() { return Ok(); }

        /// <summary>
        /// Test for a user with manager privileges.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestForAManager")]
        [Authorize(Policy = "Manager")]
        public IActionResult TestForAManager() { return Ok(); }

        /// <summary>
        /// Test for a user with manager and administrator privileges.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestForAManagerAdministrator")]
        [Authorize(Policy = "Manager")]
        [Authorize(Policy = "Administrator")]
        public IActionResult TestForAManagerAdministrator() { return Ok(); }

        /// <summary>
        /// Test for a user with either manager and/or administrator privileges.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestForEitherAManagerOrAdministrator")]
        [AuthorizeOnAnyOnePolicy("Manager,Administrator")]
        public IActionResult TestForEitherAManagerOrAdministrator() { return Ok(); }

    }
}
