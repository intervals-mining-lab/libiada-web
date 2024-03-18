namespace Libiada.Web.Extensions;

using System.Security.Claims;

/// <summary>
///  Extensions methods for <see cref="ClaimsPrincipal"/>
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");

    /// <summary>
    /// Gets id of the given user.
    /// </summary>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity ?? throw new Exception("ClaimsPrincipal Identity is null");
        Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("ClaimsPrincipal Identity does not contain NameIdentifier");
        return int.Parse(claim.Value);
    }
}
