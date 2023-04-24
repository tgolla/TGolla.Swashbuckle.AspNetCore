using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TGolla.AspNetCore.Mvc.Filters;

namespace TGolla.Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Added a security 
    /// </summary>
    public class AddSecurityRequirement : IOperationFilter
    {
        OpenApiSecurityScheme openApiSecurityScheme;

        /// <summary>
        /// Initializes a new instance of the AddSecurityRequirement class.
        /// </summary>
        /// <param name="openApiSecurityScheme"></param>
        public AddSecurityRequirement(OpenApiSecurityScheme openApiSecurityScheme) 
        {
            this.openApiSecurityScheme = openApiSecurityScheme;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var authorizeAttributes = context.GetControllerAndActionAttributes<AuthorizeAttribute>();
            var authorizeOnAnyOnePolicyAttributes = context.GetControllerAndActionAttributes<AuthorizeOnAnyOnePolicyAttribute>();

            // Get list of authorize policies.
            List<string> authorizeAttributePolicies = authorizeAttributes.AuthorizeAttributePolicies();

            // Get a list of roles.
            List<string> authorizeAttributeRoles = authorizeAttributes.AuthorizeAttributeRoles();

            // Get list of authorize on any one policy policies. 
            List<string> authorizeOnAnyOnePolicyAttributePolicies = authorizeOnAnyOnePolicyAttributes.AuthorizeOnAnyOnePolicyAttributePolicies();

            List<string> requiredScopes = authorizeAttributePolicies;
            requiredScopes.AddRange(authorizeAttributeRoles); 
            requiredScopes.AddRange(authorizeOnAnyOnePolicyAttributePolicies);

            if (authorizeAttributes.Any() || authorizeOnAnyOnePolicyAttributes.Any())
            {
                if (!operation.Responses.ContainsKey("401"))
                    operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

                if (authorizeAttributePolicies.Any() || authorizeAttributeRoles.Any() || authorizeOnAnyOnePolicyAttributePolicies.Any())
                {
                    if (!operation.Responses.ContainsKey("403"))
                        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
                }

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ openApiSecurityScheme ] = requiredScopes.ToList()
                    }
                };
            }
        }
    }
}
