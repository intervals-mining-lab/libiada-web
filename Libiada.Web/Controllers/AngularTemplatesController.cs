namespace Libiada.Web.Controllers;

/// <summary>
/// Controller for partial views.
/// Needed for angular templates.
/// </summary>
[Route("[controller]")]
public class AngularTemplatesController : Controller
{
    /// <summary>
    /// Universal action for all angular template views.
    /// </summary>
    /// <param name="viewName">
    /// Name of the view.
    /// </param>
    /// <returns>
    /// The <see cref="PartialViewResult"/>.
    /// </returns>
    [HttpGet("{viewName}")]
    public IActionResult HandleUnknownAction(string viewName) => PartialView(viewName);
    
}
