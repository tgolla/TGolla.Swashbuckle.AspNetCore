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
