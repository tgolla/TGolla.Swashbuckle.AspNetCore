using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetCore.AutoRegisterDi;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Security.Cryptography;
using TestWebApi.Services;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
JwtSettings jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

var assembliesToScan = new[]
{
    Assembly.GetAssembly(typeof(GenerateTokensService))
};

builder.Services.RegisterAssemblyPublicNonGenericClasses(assembliesToScan)
    .Where(c => c.Name.EndsWith("Service"))
    .AsPublicImplementedInterfaces();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
        // Creating the RSA key.
        RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        provider.ImportSubjectPublicKeyInfo(new ReadOnlySpan<byte>(Convert.FromBase64String(jwtSettings.PublicKey)), out _);
        RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(provider);

        options.IncludeErrorDetails = true; // Great for debugging.
        options.SaveToken = true;

        // Configure the actual Bearer validation
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = rsaSecurityKey,
            ValidAudience = jwtSettings.Audience,
            ValidIssuer = jwtSettings.Issuer,
            RequireSignedTokens = true,
            RequireExpirationTime = true, // "exp"
            ValidateLifetime = true, // "exp" will be validated.
            ValidateAudience = true,
            ValidateIssuer = true
        };
    });

//TODO: ***** Security group roles (policies) of APIs. Modify when adding security group role to APIs. *****
List<string> securityGroups = new List<string> { "Administrator", "Manager" };

builder.Services.AddAuthorization(options =>
{
    // Add policy for each group the user belongs to.
    foreach (string securityGroup in securityGroups)
    {
        options.AddPolicy(securityGroup, policy =>
            policy.RequireAuthenticatedUser().RequireAssertion(context =>
            {
                // Loop through groups claims.
                foreach (var claim in context.User.Claims.Where(c => c.Type.Equals("groups")))
                {
                    if (claim.Value.Equals(securityGroup))
                        return true;
                }

                return false;
            }));
    }
});

string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
string assemblyInformationalVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
string title = "Test Web API";
string currentDocumentName = "current";

// Used by alternate example of sort by controller and then HTTP method as ordered in array.
//string[] methodsOrder = new string[7] { "get", "post", "put", "patch", "delete", "options", "trace" };

SwaggerControllerOrder<ControllerBase> swaggerControllerOrder = new SwaggerControllerOrder<ControllerBase>(Assembly.GetEntryAssembly());

// Note: As of 4/17/2023 adding includeControllerXmlComments: true for IncludeXmlComments breaks swaggerControllerOrder.
//     c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
// Reference https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1960

builder.Services.AddSwaggerGen(c =>
{
    // Sets the order action by to use the SwaggerControllerOrder attribute to reorder controllers in a non-alphabetical order
    // and removes an assigned group name (i.e. [ApiExplorerSettings(GroupName = "Hidden")]) from the sort order.
    c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{apiDesc.RelativePath}");

    // Alternate example of sort by controller and then HTTP method (alphabetical). 
    //c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");

    // Alternate example of sort by controller and then HTTP method as ordered in array defined above. 
    //c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod.ToLower())}");

    // Adds the public Swagger document.
    c.SwaggerDoc(currentDocumentName, new OpenApiInfo
    {
        Title = title,
        Version = assemblyInformationalVersion
    });

    // Set the path for the XML comments used to generate the Swagger JSON file.
    // Note the filename is generated from the assembly name.
    var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.EnableAnnotations();
    
    c.OperationFilter<AppendAuthorizationToDescription>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"  A token can be acquired using any one of the /Tokens calls.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

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
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger(c =>
{
    c.RouteTemplate = $"{assemblyName}/swagger/{{documentName}}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = $"{assemblyName.ToLower()}/swagger";
    c.SwaggerEndpoint($"/{assemblyName}/swagger/{currentDocumentName}/swagger.json", title);
    c.DocumentTitle = title;
    c.DocExpansion(DocExpansion.None);
});

app.MapControllers();

app.Run();
