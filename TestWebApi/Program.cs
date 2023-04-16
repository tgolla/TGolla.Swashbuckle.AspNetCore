using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
string assemblyInformationalVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
string title = "Test Web API";
string currentDocumentName = "current";

// Used by alternate example of sort by controller and then HTTP method as ordered in array.
//string[] methodsOrder = new string[7] { "get", "post", "put", "patch", "delete", "options", "trace" };

SwaggerControllerOrder<ControllerBase> swaggerControllerOrder = new SwaggerControllerOrder<ControllerBase>(Assembly.GetEntryAssembly());

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
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.EnableAnnotations();

    c.OperationFilter<AppendAuthorizationToDescription>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"  A token can be acquired using /authentication/authenticate.",
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

app.UseAuthorization();
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