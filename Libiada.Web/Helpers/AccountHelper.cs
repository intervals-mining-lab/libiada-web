namespace Libiada.Web.Helpers;

using System.Security.Claims;
using System.Security.Principal;

/// <summary>
/// Envelop for some user methods.
/// </summary>
public static class AccountHelper
{
    /// <summary>
    /// Gets id of the given user.
    /// </summary>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public static int GetUserId(this IPrincipal principal)
    {
        var claimsIdentity = (ClaimsIdentity)principal.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim.Value);
    }
}
