using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppendAuthorizationToDescriptionExample.Services;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

namespace AppendAuthorizationToDescriptionExample.Controllers
{
    /// <summary>
    /// Generate tokens API calls.
    /// </summary>
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    [SwaggerControllerOrder(0)]
    public class TokensController : Controller
    {
        private readonly ILogger<TokensController> logger;
        private IGenerateTokensService generateTokensService;

        /// <summary>
        /// Initializes a new instance of TokensController.
        /// </summary>
        /// <param name="logger">A generic interface for logging where the category name is derived from the specified TCategoryName type name used to enable activation of a named ILogger from dependency injection.</param>
        /// <param name="generateTokensService">The generate tokens service.</param>
        public TokensController(ILogger<TokensController> logger, IGenerateTokensService generateTokensService)
        {
            this.logger = logger;
            this.generateTokensService = generateTokensService;
        }

        /// <summary>
        /// Generates a user token (with no groups).
        /// </summary>
        /// <returns>A user token.</returns>
        [HttpGet("GenerateUserToken")]
        public IActionResult GenerateUserTokenNoGroups() { return Ok(new { token = generateTokensService.GenerateUserTokenNoGroups() }); }

        /// <summary>
        /// Generates a management user token.
        /// </summary>
        /// <returns>A managemant user token.</returns>
        [HttpGet("GenerateManagementUserToken")]
        public IActionResult GenerateManagerToken() { return Ok(new { token = generateTokensService.GenerateManageToken() }); }

        /// <summary>
        /// Generates an administrative user token.
        /// </summary>
        /// <returns>A administrative user token.</returns>
        [HttpGet("GenerateAdministrativeUserToken")]
        public IActionResult GenerateAdministratorToken() { return Ok(new { token = generateTokensService.GenerateAdministratorToken() }); }

        /// <summary>
        /// Generates a management user with administrative privileges token.
        /// </summary>
        /// <returns>A managemant user with administrative privileges token.</returns>
        [HttpGet("GenerateManagementAdministratorUserToken")]
        public IActionResult GenerateManagerAdministratorToken() { return Ok(new { token = generateTokensService.GenerateManagerAdministratorToken() }); }
    }
}
