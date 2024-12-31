using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Matrix.WebServices.Authentication;

public class ApiKeyAuthFilter : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (authHeader.ToString().StartsWith("Basic "))
            {
                var keyFromHeader = authHeader.ToString().Substring("Basic ".Length);

                try
                {
                    var apiKey = Encoding.UTF8.GetString(Convert.FromBase64String(keyFromHeader));

                    if (apiKey != MatrixServer.ApiKey)
                    {
                        context.Result = new UnauthorizedObjectResult("Invalid API Key");
                    }
                }
                catch (FormatException _)
                {
                    context.Result = new UnauthorizedObjectResult("API Key was not properly formatted");
                }
            }
        }
        else
        {
            context.Result = new UnauthorizedObjectResult("API Key missing");
        } 
    }
}