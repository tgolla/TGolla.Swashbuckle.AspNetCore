using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace TGolla.Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Operational filter context extensions.
    /// </summary>
    internal static class OperationFilterContextExtensions
    {
        /// <summary>
        /// Get controller and action attributes.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="context">The operation filter context.</param>
        /// <returns>An IEnumerable of attribute type found.</returns>
        /// <remarks>Pulled from https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters.</remarks>
        public static IEnumerable<T> GetControllerAndActionAttributes<T>(this OperationFilterContext context) where T : Attribute
        {
            if (context.MethodInfo != null)
            {
                var controllerAttributes = context.MethodInfo.DeclaringType?.GetTypeInfo().GetCustomAttributes<T>();
                var actionAttributes = context.MethodInfo.GetCustomAttributes<T>();

                var result = new List<T>(actionAttributes);
                if (controllerAttributes != null)
                {
                    result.AddRange(controllerAttributes);
                }

                return result;
            }

#if NETCOREAPP3_1_OR_GREATER
            if (context.ApiDescription.ActionDescriptor.EndpointMetadata != null)
            {
                var endpointAttributes = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<T>();

                var result = new List<T>(endpointAttributes);
                return result;
            }
#endif
            return Enumerable.Empty<T>();
        }
    }
}
