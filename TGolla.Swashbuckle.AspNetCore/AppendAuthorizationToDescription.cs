using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TGolla.AspNetCore.Mvc.Filters;

namespace TGolla.Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Appends the API authentication/authorization information to the operation description.
    /// </summary>
    public class AppendAuthorizationToDescription : IOperationFilter
    {
        // Boolean used to determine if the AllowAnonymous description should be added.
        private bool excludeAllowAnonymousDescription = false;

        /// <summary>
        /// Initializes a new instance of the AppendAuthorizationToDescription class. 
        /// </summary>
        /// <param name="excludeAllowAnonymousDescription">Boolean used to determine if the AllowAnonymous description should be added.</param>
        public AppendAuthorizationToDescription(bool excludeAllowAnonymousDescription = false)
        {
            this.excludeAllowAnonymousDescription = excludeAllowAnonymousDescription;
        }

        /// <summary>
        /// Applys the appended API authentication/authorization information to the operation description.
        /// </summary>
        /// <param name="operation">An open API operation.</param>
        /// <param name="context">The operation filter context.</param>
        /// <remarks>Basic concepts pulled from https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters.</remarks>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.GetControllerAndActionAttributes<AllowAnonymousAttribute>().Any())
            {
                if (!excludeAllowAnonymousDescription)
                    operation.Description += "\r\n\r\nAuthentication/authorization is not required.";

                return;
            }

            var authorizeAttributes = context.GetControllerAndActionAttributes<AuthorizeAttribute>();
            var authorizeOnAnyOnePolicyAttributes = context.GetControllerAndActionAttributes<AuthorizeOnAnyOnePolicyAttribute>();

            // Get list of authorize policies.
            List<string> authorizeAttributePolicies = authorizeAttributes.AuthorizeAttributePolicies();

            // Get a list of roles.
            List<string> authorizeAttributeRoles = authorizeAttributes.AuthorizeAttributeRoles();

            // Get list of authorize on any one policy policies. 
            List<string> authorizeOnAnyOnePolicyAttributePolicies = authorizeOnAnyOnePolicyAttributes.AuthorizeOnAnyOnePolicyAttributePolicies();

            if (authorizeAttributePolicies.Any())
                operation.Description += $"\r\n\r\nAuthorization requires {((authorizeAttributePolicies.Count > 1) ? "each of " : "")} the following {((authorizeAttributePolicies.Count > 1) ? "policies" : "policy")}: <b>{string.Join("</b>, <b>", authorizeAttributePolicies)}</b>";

            if (authorizeAttributeRoles.Any())
                operation.Description += $"\r\n\r\nAuthorization requires {((authorizeAttributeRoles.Count > 1) ? "any one of " : "")} the following {((authorizeAttributeRoles.Count > 1) ? "roles" : "role")}: <b>{string.Join("</b>, <b>", authorizeAttributeRoles)}</b>";

            if (authorizeOnAnyOnePolicyAttributePolicies.Any())
                operation.Description += $"\r\n\r\nAuthorization requires {((authorizeOnAnyOnePolicyAttributePolicies.Count > 1) ? "any one of " : "")} the following {((authorizeOnAnyOnePolicyAttributePolicies.Count > 1) ? "policies" : "policy")}: <b>{string.Join("</b>, <b>", authorizeOnAnyOnePolicyAttributePolicies)}</b>";

            if (authorizeAttributes.Any() && !authorizeAttributePolicies.Any() && !authorizeAttributeRoles.Any() && !authorizeOnAnyOnePolicyAttributePolicies.Any())
                operation.Description += "\r\n\r\nAuthentication, but no authorization is required.";
        }
    }
}
