This package implements the code solution for a StackOverflow question on how to secure an API call with more than one policy.

StackOverflow 
https://stackoverflow.com/questions/51443605/how-to-include-multiple-policies

# Question - How to include multiple policies

I have defined 2 policies, ```ADD``` and ```SUB``` as shown below.

```csharp
options.AddPolicy("ADD", policy =>
    policy.RequireClaim("Addition", "add"));

options.AddPolicy("SUB", policy =>
    policy.RequireClaim("Substraction", "subs"));
```

All that I want to do is to include 2 policies on a controller method. How can I perform this operation.

```csharp
 [Authorize(Policy = "ADD, SUB")]
 [HttpPost]
 public IActionResult PerformCalculation()
 {
 }
 ```

However, this gives me an error:

    InvalidOperationException: The AuthorizationPolicy named: 'ADD, SUB' was not found

# Answer

The first thing to realize is that the Authorize attribute Policy setting is singular unlike Roles which can be plural and that multiple policies are treated on an AND basis, unlike a list of roles which is treated on an OR basis.

In your example code ```“ADD, SUB”``` is considered a single policy name.  If you want to attribute you method with both policies, your code should be as follows.

```csharp
[Authorize(Policy = "ADD")]
[Authorize(Policy = "SUB")]
[HttpPost]
public IActionResult PerformCalculation()
{
}
```

However, this will not give you the effect you want of either or, since policies are AND together, hence both policies must pass to be authorized.  Nor will the suggestions of writing a single policy or a requirements handler to handle the multiple requirements give you the result of treating policies on a OR basis.

Instead, the solution is to create a ```TypeFilterAttribute``` that accepts a list of policies and is tied to a ```IAsyncAuthorizationFilter``` that test for either or.  The following outlines the two classes you will need to define and how to attribute your action method.   

The following code defines the new attribute ```AuthorizeOnAnyOnePolicy``` class.

```csharp
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

        Arguments = new object[] { policies };
    }
}
```

The following code defines the authorization filter class which loops through and executes each policy in the list.  Should all the policies fail the result of the authorization context is set to forbid.

```csharp
public class AuthorizeOnAnyOnePolicyFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationService authorization;
    public string Policies { get; private set; }

    /// <summary>
    /// Initializes a new instance of the AuthorizeOnAnyOnePolicyFilter class.
    /// </summary>
    /// <param name="policies">A comma delimited list of policies that are allowed to access the resource.</param>
    /// <param name="authorization">The AuthorizationFilterContext.</param>
    public AuthorizeOnAnyOnePolicyFilter(string policies, IAuthorizationService authorization)
    {
        Policies = policies;
        this.authorization = authorization;
    }

    /// <summary>
    /// Called early in the filter pipeline to confirm request is authorized.
    /// </summary>
    /// <param name="context">A context for authorization filters i.e. IAuthorizationFilter and IAsyncAuthorizationFilter implementations.</param>
    /// <returns>Sets the context.Result to ForbidResult() if the user fails all of the policies listed.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var policies = Policies.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            
        // Loop through policies.  User need only belong to one policy to be authorized.
        foreach (var policy in policies)
        {
            var authorized = await authorization.AuthorizeAsync(context.HttpContext.User, policy);
            if (authorized.Succeeded)
            {
                return;
            }

        }
            
        context.Result = new ForbidResult();
        return;
    }
}
```

With the policies defined as shown in the question you would attribute the method as follows.

```csharp
[AuthorizeOnAnyOnePolicy("ADD,SUB")]
[HttpPost]
public IActionResult PerformCalculation()
{
}
```

It’s that simple and you will find similar solutions in the following Stack Overflow questions.

- https://stackoverflow.com/questions/58276650/authorize-against-a-list-of-policies

- https://stackoverflow.com/questions/52628473/how-to-add-multiple-policies-in-action-using-authorize-attribute-using-identity


