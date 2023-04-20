using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TGolla.Swashbuckle.AspNetCore.SwaggerGen;

namespace SwashbuckleCustomOrderingControllersExample
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Used by alternate example of sort by controller and then HTTP method as ordered in array.
            //string[] methodsOrder = new string[7] { "get", "post", "put", "patch", "delete", "options", "trace" };

            SwaggerControllerOrder<ControllerBase> swaggerControllerOrder = new SwaggerControllerOrder<ControllerBase>(Assembly.GetEntryAssembly());

            services.AddSwaggerGen(c =>
            {
                // Sets the order action by to use the SwaggerControllerOrder attribute to reorder controllers in a non-alphabetical order.
                // To see the difference between using the controller sort order and not, comment out the following line.
                c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}");

                // Alternate example of sort by controller and removing an assigned group name (i.e. [ApiExplorerSettings(GroupName = "Hidden")]) from the sort order.
                //c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{apiDesc.RelativePath}");

                // Alternate example of sort by controller and then HTTP method (alphabetical). 
                //c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
                // Or...
                //c.OrderActionsBy(apiDesc => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{apiDesc.HttpMethod}");

                // Alternate example of sort by controller and then HTTP method as ordered in array defined above. 
                //c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod.ToLower())}");
                // Or...
                //c.OrderActionsBy(apiDesc => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod.ToLower())}");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swashbuckle Custom Ordering of Controllers");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
