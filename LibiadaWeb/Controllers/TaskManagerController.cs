namespace LibiadaWeb.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
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
            ViewBag.Tasks = TaskManager.GetTasksData();
            
            if (Request.UserLanguages != null && Request.UserLanguages.Any())
            {
                var culture = new CultureInfo(Request.UserLanguages[0]);

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View();
        }

        /// <summary>
        /// The redirect to result.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult RedirectToResult(int id)
        {
            var task = TaskManager.GetTask(id);
            switch (task.TaskData.TaskState)
            {
                case TaskState.Completed:
                case TaskState.Error:
                    TempData["Result"] = task.Result;
                    return RedirectToAction("Result", task.ControllerName);
                default:
                    TempData["ErrorMessage"] = string.Format("Task with id = {0} is not completed, current status is {1}", id, task.TaskData.TaskState);
                    return RedirectToAction("Index");
            }
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
                TempData["ErrorMessage"] = string.Format("Unable to delete task with id = {0}, reason: {1}", id, e.Message);
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
                TempData["ErrorMessage"] = string.Format("Unable to delete tasks, reason: {0}", e.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
