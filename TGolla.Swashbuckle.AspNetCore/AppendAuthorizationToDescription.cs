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
                operation.Description += "\r\n\r\nAuthentication/authorization is not required.";
                return;
            }

            var authorizeAttributes = context.GetControllerAndActionAttributes<AuthorizeAttribute>();
            var authorizeOnAnyOnePolicyAttributes = context.GetControllerAndActionAttributes<AuthorizeOnAnyOnePolicyAttribute>();

            // Get list of authorize policies.
            List<string> authorizeAttributePolicies = authorizeAttributes.Where(x => !string.IsNullOrEmpty(x.Policy))
                .OrderBy(x => x.Policy).Select(x => x.Policy).ToList();

            // Get a list of roles.
            List<string> authorizeAttributeRoles = authorizeAttributes.Where(x => !string.IsNullOrEmpty(x.Roles))
                .OrderBy(x => x.Roles).Select(x => x.Roles).ToList();

            // Get list of authorize any policy policies. 
            List<string> authorizeOnAnyOnePolicyAttributePolicies = new List<string>();
            if (authorizeOnAnyOnePolicyAttributes.Any())
            {
                authorizeOnAnyOnePolicyAttributePolicies = authorizeOnAnyOnePolicyAttributes.First().Arguments[0]
                    .ToString().Split(",").OrderBy(x => x).ToList();
            }

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
