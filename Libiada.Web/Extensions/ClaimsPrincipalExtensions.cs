namespace Libiada.Web.Extensions;

using System.Security.Principal;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this IPrincipal user) => user.IsInRole("Admin");
}
