namespace Libiada.Web.Helpers;

using System.Security.Claims;

public interface IViewDataBuilderFactory
{
    IViewDataBuilder Create(ClaimsPrincipal user);
}