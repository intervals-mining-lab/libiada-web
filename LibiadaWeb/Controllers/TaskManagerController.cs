namespace LibiadaWeb.Controllers
{
    using System;
    using System.Web.Mvc;
    using LibiadaWeb.Tasks;

    /// <summary>
    /// The calculation controller.
    /// </summary>
    [Authorize]
    public class TaskManagerController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ViewBag.UserError = TempData["ErrorMessage"];
            }

            ViewBag.data = "{}";

            return View();
        }
    }
}
