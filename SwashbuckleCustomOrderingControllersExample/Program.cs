using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Used by alternate example of sort by controller and then HTTP method as ordered in array.
string[] methodsOrder = new string[7] { "get", "post", "put", "patch", "delete", "options", "trace" };

SwaggerControllerOrder<ControllerBase> swaggerControllerOrder = new SwaggerControllerOrder<ControllerBase>(Assembly.GetEntryAssembly());

// Note: As of 4/17/2023 adding includeControllerXmlComments: true for IncludeXmlComments breaks swaggerControllerOrder.
//     c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
// Reference https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1960

builder.Services.AddSwaggerGen(c =>
{
    // Sets the order action by to use the SwaggerControllerOrder attribute to reorder controllers in a non-alphabetical order.
    // To see the difference between using the controller sort order and not, comment out the following line.
    //c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}");

    // Alternate example of sort by controller and removing an assigned group name (i.e. [ApiExplorerSettings(GroupName = "Hidden")]) from the sort order.
    //c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{apiDesc.RelativePath}");

    // Alternate example of sort by controller and then HTTP method (alphabetical). 
    //c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
    // Or...
    //c.OrderActionsBy(apiDesc => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{apiDesc.HttpMethod}");

    // Alternate example of sort by controller and then HTTP method as ordered in array defined above. 
    //c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod.ToLower())}");
    // Or...
    c.OrderActionsBy(apiDesc => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod.ToLower())}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseRouting();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swashbuckle Custom Ordering of Controllers");
});

app.MapControllers();

app.Run();
