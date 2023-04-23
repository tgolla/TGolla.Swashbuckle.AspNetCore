using Microsoft.AspNetCore.Authorization;

namespace TGolla.Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// IEnumerable AuthorizeAttribute extensions.
    /// </summary>
    public static class IEnumerableAuthorizeAttributeExtensions
    {
        /// <summary>
        /// Returns a list of AuthorizeAttribute policies.
        /// </summary>
        /// <param name="authorizeAttributes">IEnumerable of type AuthorizeAttribute.</param>
        /// <returns>A list of AuthorizeAttribute policies.</returns>
        public static List<string> AuthorizeAttributePolicies(this IEnumerable<AuthorizeAttribute> authorizeAttributes)
        {
            return authorizeAttributes.Where(x => !string.IsNullOrEmpty(x.Policy))
                .OrderBy(x => x.Policy).Select(x => x.Policy).ToList();
        }

        /// <summary>
        /// Returns a list of AuthorizeAttribute roles.
        /// </summary>
        /// <param name="authorizeAttributes">IEnumerable of type AuthorizeAttribute.</param>
        /// <returns>A list of AuthorizeAttribute roles.</returns>
        public static List<string> AuthorizeAttributeRoles(this IEnumerable<AuthorizeAttribute> authorizeAttributes)
        {
            return authorizeAttributes.Where(x => !string.IsNullOrEmpty(x.Roles))
                .OrderBy(x => x.Roles).Select(x => x.Roles).ToList();
        }

    }
}
