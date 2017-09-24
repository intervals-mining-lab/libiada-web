namespace LibiadaWeb.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Controller for partial views.
    /// Needed for angular templates.
    /// </summary>
    public class PartialController : Controller
    {
        /// <summary>
        /// The matters table partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public PartialViewResult _MattersTable() => PartialView();
    }
}
