using TGolla.AspNetCore.Mvc.Filters;

namespace TGolla.Swashbuckle.AspNetCore
{
    /// <summary>
    /// IEnumerable AuthorizeOnAnyOnePolicyAttribute extensions.
    /// </summary>
    public static class IEnumerableAuthorizeOnAnyOnePolicyAttributeExtensions
    {
        /// <summary>
        /// Returns a list to AuthorizeOnAnyOnePolicyAttribute policies.
        /// </summary>
        /// <param name="authorizeOnAnyOnePolicyAttributes">IEnumerable of type AuthorizeOnAnyOnePolicyAttribute.</param>
        /// <returns>A list to AuthorizeOnAnyOnePolicyAttribute policies.</returns>
        public static List<string> AuthorizeOnAnyOnePolicyAttributePolicies(this IEnumerable<AuthorizeOnAnyOnePolicyAttribute> authorizeOnAnyOnePolicyAttributes)
        {
            List<string> authorizeOnAnyOnePolicyAttributePolicies = new List<string>();

            if (authorizeOnAnyOnePolicyAttributes.Any())
            {
                authorizeOnAnyOnePolicyAttributePolicies = authorizeOnAnyOnePolicyAttributes.First().Arguments[0]
                    .ToString().Split(",").OrderBy(x => x).ToList();
            }

            return authorizeOnAnyOnePolicyAttributePolicies;
        }
    }
}
