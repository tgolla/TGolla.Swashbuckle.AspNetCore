I've always wished swagger documentation included authentication and more importantly authorization information for each API call. Fortunately, Swashbuckle can be configured with various methods and filters to generate your very own customized Swagger documentation. Unfortunately, while the Swashbuckle documentation is good, it is often hard to find good examples.

This example started with a search of the Internet, with the thought that surely someone else had thought of this exact thing. But to my dismay, the only thing I found that came remotely close to what I was looking for, I found in the GitHub repository [Swashbuckle.AspNetCore.Filters](https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters) published by Matt Frear. In the code repository is an ```AppendAuthorizeToSummaryOperationFilter``` which appends authorization information to the API summary. While this was close to what I was looking for I was concerned about space, some of the APIs I need to document can have 5-10 policies.  I also wanted a bit more verbose description of the roles and/or policies required with the understanding that roles are ORed, while policies are ANDed.  And I need it to handle the custom authentication attribute ```AuthorizeOnAnyOnePolicyAttribute```. More about this attribute can be found in the GitHub repository TGolla.Swashbuckle.AspNetCore TGolla.AspNetCore.Mvc.Filters [`Readme.md`](https://github.com/tgolla/TGolla.Swashbuckle.AspNetCore/blob/main/TGolla.AspNetCore.Mvc.Filters/Readme.md) file. 

With an example to guide me I’ve put together the following operation filter which appends detailed information about authentication/authorization to the end of the API description. The description is the first thing you see following the summary and is populated with the text found inside the triple-slash comments ```<remarks></remarks>``` tags for the API call. 

If you would like to see the filter in action check out the ``` AppendAuthorizationToDescriptionExample``` website in the GitHub repository  [TGolla.Swashbuckle.AspNetCore](https://github.com/tgolla/TGolla.Swashbuckle.AspNetCore/tree/main/TGolla.Swashbuckle.AspNetCore) or start using the filter in your project by installing the NuGet package [TGolla.Swashbuckle.AspNetCore](https://www.nuget.org/packages/TGolla.Swashbuckle.AspNetCore/).

```csharp
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
```

The code is relatively simple.  The ```AppendAuthorizationToDescription``` operation filter is created using the ```IOperationFilter``` interface. The interface requires that you define the ```Apply()``` method. This method is handed an ```OpenApiOperation``` which gives you access to things like the API description and an ```OperationFilterContext``` which gives you access to the API attributes.

The first thing the ```Apply()``` method does is to look to see if the API method is decorated with an ```AllowAnonymousAttribute```.

```csharp
[AllowAnonymous]
```
This is done by calling the ```OperationFilterContext``` extension method  ```GetControllerAndActionAttributes```. This was pulled directly from the GitHub repository Swashbuckle.AspNetCore.Filters published by Matt Frear.  The method takes an ```Attribute``` type and returns an ``` IEnumerable``` list of any attributes of the type passed. If an ```AllowAnonymousAttribute``` is found and assuming the ```excludeAllowAnonymousDescription``` is false the message “Authentication/authorization is not required.” is appended to the operation description.

If an ```AllowAnonymousAttribute``` was not found the code continues on to again uses the ```GetControllerAndActionAttributes``` method, this time to collect a list of attributes type ```AuthorizeAttribute``` and a list of attributes type ```AuthorizeOnAnyOnePolicyAttribute```.  These lists are then queried using Linq to build string lists of policies, roles and authorize on any one policies with the extension methods `AuthorizeAttributePolicies`, `AuthorizeAttributeRoles`, and `AuthorizeOnAnyOnePolicyAttributePolicies`.  The string lists are then used to generate a verbose message concerning the authorization required which is appended to the description and if there are no required policies, roles or authorize on any one policies the message “Authentication, but no authorization is required.” is returned.

To implement the filter requires that you call the ```OperationFilter<>()``` method with the ```AppendAuthorizationToDescription``` class inside ```AddSwaggerGen()``` in your ```progrms.cs``` file.

``` csharp
builder.Services.AddSwaggerGen(c =>
{
	...
    c.OperationFilter<AppendAuthorizationToDescription>();
	...
}
```

If you wish to exclude the ```AllowAnonymousAttribute``` message “Authentication/authorization is not required.” from an API description you will need to set the ```excludeAllowAnonymousDescription``` parameter argument to true.

```csharp
c.OperationFilter<AppendAuthorizationToDescription>(true);
```

Also included in the TGolla.Swashbuckle.AspNetCore code repository is the ```AddSecurityRequirement``` operation filter which allows you to target operations that require a security schema. 

```csharp
builder.Services.AddSwaggerGen(c =>
{
	...
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"  A token can be acquired using any one of the /Tokens API calls.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
   	...
}
```

When you define a security schema by invoking the ```AddSecurityDefinition``` method you also need to indicate which operations that scheme is applicable to. You can apply schemes globally (i.e. to ALL operations) through the ```AddSecurityRequirement``` method. This is what adds the Authorize button and unlock/lock icons to the end of each API summary.

```csharp
builder.Services.AddSwaggerGen(c =>
{
	...
	c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
    ...
}
```

Or you can be more specific by replacing the ```AddSecurityRequirement``` method with the ```AddSecurityRequirement``` operation filter provide in this example. The filter will only apply the security schema to API actions decorated with either the ```AuthorizeAttribute``` or ```AuthorizeOnAnyOnePolicyAttribute``` attributes. In this configuration it also makes sense to set the ```excludeAllowAnonymousDescription``` parameter argument to true.

```csharp
builder.Services.AddSwaggerGen(c =>
{
	...
    c.OperationFilter<AppendAuthorizationToDescription>(true);
    ...
    c.OperationFilter<AddSecurityRequirement>(new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    });
    ...
}
```

Addition information on adding security definitions and requirements can be found the Swashbuckle documentation [domaindrivendev/Swashbuckle.AspNetCore: Swagger tools for documenting API's built on ASP.NET Core (github.com)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements).

