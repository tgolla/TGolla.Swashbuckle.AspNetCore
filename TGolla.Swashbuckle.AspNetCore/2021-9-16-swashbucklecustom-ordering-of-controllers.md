This article is an adaptation of an article written by Rob Janssen ([RobIII](https://blog.robiii.nl/ 'RobIII')) in 2018 on customizing the order in which controllers are display in the [Swagger UI by Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore "Swagger UI by Swashbuckle").  It addresses the depreciation of the method OrderActionGroupsBy which is no longer available when using ```AddSwaggerGen```, ```UseSwagger``` and ```UseSwaggerUI``` in your projects ```startup.cs``` file by using the ```AddSwaggerGen``` method ```OrderActionsBy``` in the configuration.

By default, when using Swashbuckle to generate a Swagger document, controllers are ordered alphabetically.  There are however situations where alphabetical ordering is not best for your documentation.  For example, consider an API project in which each controller represents a type of theater (Arena, Black Box, Proscenium, Thrust).  Instead of sorting these alphabetically it makes more sense to order them as they are normally sorted when teaching theater (Proscenium, Thrust, Arena, Black Box).

In this article we will look at using a custom attribute to affect the order controllers are displayed in the Swagger documentation.  Again, most of this code was adapted from Rob Janssen’s article [Swashbuckle Custom Ordering of Controllers](https://blog.robiii.nl/2018/08/swashbuckle-custom-ordering-of.html "Swashbuckle Custom Ordering of Controllers"). A complete example can be found on GitHub at... 

[https://github.com/tgolla/SwashbuckleCustomOrderingControllersExample](https://github.com/tgolla/SwashbuckleCustomOrderingControllersExample "Swashbuckle Custom Ordering Controllers Example")

## How It Works

When adding Swagger to your project using the Swashbuckle tools you have the ability to alter the sort order of the Controllers and even the individual APIs in the configuration information of the ```AddSwaggerGen``` method using ```OrderActionsBy```.  With ```OrderActionsBy``` you use a lambda expression to alter the sort key string use to order (group) API calls. 

By default, the sort key is set to the first tag which by default, is set to the controller’s name, hence controllers are sorted alphabetically after which API calls are normally ordered by relative path.  The following code represents the equivalent to the predefined default as defined using  ```OrderActionsBy```.

```
OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}");
```

In our example we are going to annotate each controller class with an order attribute, create a class that uses reflections to collect a list of all the controllers along with the order attribute value and expose a method in the class that returns the order and controller name in a sort key string.  This method can then be used in the ```OrderActionsBy``` lambda expression.

```
OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}")
```

We will also look at going one step further by adding additional information like the HTTP method (GET, POST, PUT, DELETE,...) to affect the order of the individual API calls.

## The Code
To begin we need to define an attribute class (```SwaggerControllerOrderAttribute.cs```).

```
namespace TGolla.Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Annotates a controller with a Swagger sorting order that is used when generating 
    /// the Swagger documentation to order the controllers in a specific desired order.
    /// </summary>
    /// <remarks>
    /// Ref: http://blog.robiii.nl/2018/08/swashbuckle-custom-ordering-of.html modified for AddSwaggerGen() extension OrderActionsBy().
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md#change-operation-sort-order-eg-for-ui-sorting
    /// </remarks>
    public class SwaggerControllerOrderAttribute : Attribute
    {
        /// <summary>
        /// Gets the sorting order of the controller.
        /// </summary>
        public uint Order { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerControllerOrderAttribute"/> class.
        /// </summary>
        /// <param name="order">Sets the sorting order of the controller.</param>
        public SwaggerControllerOrderAttribute(uint order)
        {
            Order = order;
        }
    }
}
```

This class allows us to annotate a controller with the attribute ```[SwaggerControllerOrder(x)]``` where _x_ is an integer value indicating the order by ascending value.  By default, a controller that is not annotated with the attribute is assigned the order value of 4294967295.  Controllers with the same order assigned either by default or with the attribute are then sorted alphabetically, however this can be altered when setting the ```OrderActionBy``` method.
In our example we will want to annotate each of our controllers as follows.

```
    [ApiController]
    [Route("theater/[controller]")]
    [SwaggerControllerOrder(1)]
    public class ProsceniumController : ControllerBase
    {
        // ...
    }

    [ApiController]
    [Route("theater/[controller]")]
    [SwaggerControllerOrder(2)]
    public class ThrustController: ControllerBase
    {
        // ...
    }

    [ApiController]
    [Route("theater/[controller]")]
    [SwaggerControllerOrder(3)]
    public class ArenaController: ControllerBase
    {
        // ...
    }

    [ApiController]
    [Route("theater/[controller]")]
    [SwaggerControllerOrder(4)]
    public class BlackBoxController: ControllerBase
    {
        // ...
    }
```

Next we need to create a class that will collect a list of controllers with the SwaggerControllerOrder attribute value (```SwaggerControllerOrder```).

```
using System.Reflection;

namespace TGolla.Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Class for determining controller sort keys based on the SwaggerControllerOrder attribute.
    /// </summary>
    /// <typeparam name="T">The type controllers should implement.  By default this would normally be ControllerBase or Controller
    /// unless you have derived your own specific api controller class.</typeparam>
    /// <remarks>
    /// Ref: http://blog.robiii.nl/2018/08/swashbuckle-custom-ordering-of.html modified for AddSwaggerGen() extension OrderActionsBy().
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md#change-operation-sort-order-eg-for-ui-sorting
    /// </remarks>
    public class SwaggerControllerOrder<T>
    {
        private readonly Dictionary<string, uint> orders;   // Our lookup table which contains controllername -> sortorder pairs.

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerControllerOrder&lt;TargetException&gt;"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to scan for for classes implementing <typeparamref name="T"/>.</param>
        public SwaggerControllerOrder(Assembly assembly)
            : this(GetFromAssembly<T>(assembly)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerControllerOrder&lt;TargetException&gt;"/> class.
        /// </summary>
        /// <param name="controllers">
        /// The controllers to scan for a <see cref="SwaggerControllerOrderAttribute"/> to determine the sortorder.
        /// </param>
        public SwaggerControllerOrder(IEnumerable<Type> controllers)
        {
            // Initialize our dictionary; scan the given controllers for our custom attribute, read the Order property
            // from the attribute and store it as controllername -> sorderorder pair in the (case-insensitive) dicationary.
            orders = new Dictionary<string, uint>(
                controllers.Where(c => c.GetCustomAttributes<SwaggerControllerOrderAttribute>().Any())
                .Select(c => new { Name = ResolveControllerName(c.Name), c.GetCustomAttribute<SwaggerControllerOrderAttribute>().Order })
                .ToDictionary(v => v.Name, v => v.Order), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns all <typeparamref name="TController"/>'s from the given assembly.
        /// </summary>
        /// <typeparam name="TController">The type classes must implement to be regarded a controller.</typeparam>
        /// <param name="assembly">The assembly to scan for given <typeparamref name="TController"/>s.</param>
        /// <returns>Returns all types implementing <typeparamref name="TController"/>.</returns>
        public static IEnumerable<Type> GetFromAssembly<TController>(Assembly assembly)
        {
            return assembly.GetTypes().Where(c => typeof(TController).IsAssignableFrom(c));
        }

        /// <summary>
        /// Determines the 'friendly' name of the controller by stripping the (by convention) "Controller" suffix
        /// from the name. If there's a built-in way to do this in .Net then I'd love to hear about it!
        /// </summary>
        /// <param name="name">The name of the controller.</param>
        /// <returns>The friendly name of the controller.</returns>
        private static string ResolveControllerName(string name)
        {
            const string suffix = "Controller"; // We want to strip "Controller" from "FooController"

            // Ensure name ends with suffix (case-insensitive)
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                // Return name with suffix stripped
                return name.Substring(0, name.Length - suffix.Length);
            // Suffix not found, return name as-is
            return name;
        }

        /// <summary>
        /// Returns the unsigned integer sort order value.  
        /// </summary>
        /// <param name="controller">The controller name.</param>
        /// <returns>The unsigned integer sort order value.</returns>
        private uint Order(string controller)
        {
            // Try to get the sort order value from our lookup; if none is found, assume uint.MaxValue.
            if (!orders.TryGetValue(controller, out uint order))
                order = uint.MaxValue;

            return order;
        }

        /// <summary>
        /// Returns an order key based on a the SwaggerControllerOrderAttribute for use with OrderActionsBy.
        /// </summary>
        /// <param name="controller">The controller name.</param>
        /// <returns>A zero padded 32-bit unsigned integer.</returns>
        public string OrderKey(string controller)
        {
            return Order(controller).ToString("D10");
        }

        /// <summary>
        /// Returns a sort key based on a the SwaggerControllerOrderAttribute for use with OrderActionsBy.
        /// </summary>
        /// <param name="controller">The controller name.</param>
        /// <returns>A zero padded 32-bit unsigned integer combined with the controller's name.</returns>
        public string SortKey(string controller)
        {
            return $"{OrderKey(controller)}_{controller}";
        }
    }
}
```

To use the class, we define an instance in the ``` ConfigureServices``` method of your ```Startup.cs```.

```
SwaggerControllerOrder<ControllerBase> swaggerControllerOrder = new SwaggerControllerOrder<ControllerBase>(Assembly.GetEntryAssembly());
```

When instantiated the ```SwaggerControllerOrder``` class searches the assembly for controllers of type ```ControllerBase``` and builds a dictionary list of controller names with their associated ``` SwaggerControllerOrder``` attribute value.  Those without the ``` SwaggerControllerOrder``` attribute are not place in the list and will be assigned the default max value 4294967295.

With the class instantiated we can now use it when adding the Swagger generation service by adding an ``` OrderActionsBy``` method call to the ``` AddSwaggerGen``` configuration in the ```program.cs``` file.

```
builder.Services.AddSwaggerGen(c =>
{
    c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}");
});
```

## Taking It a Step Further

As mentioned earlier you can also use ``` OrderActionsBy``` to  affect the order of the individual API calls. 

For example if you are adding group names (i.e. ```[ApiExplorerSettings(GroupName = "Hidden")]```) to certain API calls to say hide those API calls from certain users you but don’t want then to be group (displayed) separately for your advanced users you can add the relative path to the sort key string.

```
c.OrderActionsBy((apiDesc) => $"{swaggerControllerOrder.SortKey(apiDesc.ActionDescriptor.RouteValues["controller"])}_{apiDesc.RelativePath}");
```

If you would like to see your API calls ordered alphabetically by HTTP method, you could use the following lambda expression.

```
c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
```

If you would like to see API calls ordered by HTTP method, in a specific order you could add the following sort order array...

```
string[] methodsOrder = new string[5] { "get", "post", "put", "patch", "delete", "options", "trace" };
```

... with the following lambda expression.

```
c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod.ToLower())}");
```

The possibilities are endless, and this example could even be taken one step further by adding an attribute to annotate ``` IActionResult``` (API) calls.

Ref: [https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting "Change Operation Sort Order (e.g. for UI Sorting)")

