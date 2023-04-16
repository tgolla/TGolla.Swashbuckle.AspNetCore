using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace TGolla.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires 
    /// authorization based on user passing any one policy in the provided list of policies.
    /// </summary>
    public class AuthorizeOnAnyOnePolicyAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the AuthorizeOnAnyOnePolicyAttribute class.
        /// </summary>
        /// <param name="policies">A comma delimited list of policies that are allowed to access the resource.</param>
        public AuthorizeOnAnyOnePolicyAttribute(string policies) : base(typeof(AuthorizeOnAnyOnePolicyFilter))
        {
            Regex commaDelimitedWhitespaceCleanup = new Regex("\\s+,\\s+|\\s+,|,\\s+",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            Arguments = new object[] { commaDelimitedWhitespaceCleanup.Replace(policies, ",") };
        }
    }
}
