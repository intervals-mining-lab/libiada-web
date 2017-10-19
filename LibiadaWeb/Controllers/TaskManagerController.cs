namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Models;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

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
            //if (Request.UserLanguages != null && Request.UserLanguages.Any())
            //{
            //    var culture = new CultureInfo(Request.UserLanguages[0]);

            //    Thread.CurrentThread.CurrentCulture = culture;
            //    Thread.CurrentThread.CurrentUICulture = culture;
            //}

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ViewBag.UserError = TempData["ErrorMessage"];
            }

            ViewBag.data = "{}";

            return View();
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                TaskManager.DeleteTask(id);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Unable to delete task with id = {id}, reason: {e.Message}";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// The delete all.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAll()
        {
            try
            {
                TaskManager.DeleteAllTasks();
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Unable to delete tasks, reason: {e.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
