namespace Libiada.Web.Extensions;

using System.Security.Claims;

public static class HttpContextAccessorExtensions
{
    public static ClaimsPrincipal GetCurrentUser(this IHttpContextAccessor httpContextAccessor)
    {
        HttpContext httpContext = httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null");
        return httpContext.User;
    }
}
