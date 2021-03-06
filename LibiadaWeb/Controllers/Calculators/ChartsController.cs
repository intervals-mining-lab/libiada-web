﻿namespace LibiadaWeb.Controllers.Calculators
{
    using System.Web.Mvc;

    /// <summary>
    /// The charts controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ChartsController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}
