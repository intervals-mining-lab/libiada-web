namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
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
            if (Request.UserLanguages != null && Request.UserLanguages.Any())
            {
                var culture = new CultureInfo(Request.UserLanguages[0]);

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ViewBag.UserError = TempData["ErrorMessage"];
            }

            var tasks = TaskManager.GetTasksData().Select(t => new
                        {
                            t.Id,
                            t.DisplayName,
                            Created = t.Created.ToString(OutputFormats.DateTimeFormat),
                            Started = t.Started == null ? string.Empty : ((DateTimeOffset)t.Started).ToString(OutputFormats.DateTimeFormat),
                            Completed = t.Completed == null ? string.Empty : ((DateTimeOffset)t.Completed).ToString(OutputFormats.DateTimeFormat),
                            ExecutionTime = t.ExecutionTime == null ? string.Empty : ((TimeSpan)t.ExecutionTime).ToString(OutputFormats.TimeFormat),
                            TaskState = t.TaskState.ToString(),
                            TaskStateName = EnumHelper.GetDisplayValue(t.TaskState),
                            t.UserId
                        });

            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "tasks", tasks }
                });

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
            try
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
            catch (Exception e)
            {
                TempData["ErrorMessage"] = string.Format("Cannot redirect to result of task with id = {0}. {1}", id, e.Message);
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
