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
                TaskManager.Instance.DeleteTask(id);
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
                TaskManager.Instance.DeleteAllTasks();
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Unable to delete tasks, reason: {e.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
