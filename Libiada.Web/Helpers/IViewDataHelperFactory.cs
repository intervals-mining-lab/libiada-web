namespace Libiada.Web.Helpers;

using System.Security.Claims;

public interface IViewDataHelperFactory
{
    IViewDataHelper Create(ClaimsPrincipal user);
}