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
                            DisplayName = t.TaskState.GetDisplayValue(),
                            Created = t.Created.ToString(OutputFormats.DateTimeFormat),
                            Started = t.Started?.ToString(OutputFormats.DateTimeFormat),
                            Completed = t.Completed?.ToString(OutputFormats.DateTimeFormat),
                            ExecutionTime = t.ExecutionTime?.ToString(OutputFormats.TimeFormat),
                            TaskState = t.TaskState.ToString(),
                            TaskStateName = t.TaskState.GetDisplayValue(),
                            t.UserId,
                            t.UserName
                        });

            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "tasks", tasks.ToArray() }
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
                Task task = TaskManager.GetTask(id);

                switch (task.TaskData.TaskState)
                {
                    case TaskState.Completed:
                    case TaskState.Error:
                        TempData["Result"] = task.Result;
                        return RedirectToAction("Result", task.TaskData.TaskType.GetName(), new { id });
                    default:
                        TempData["ErrorMessage"] = $"Task with id = {id} is not completed, current status is {task.TaskData.TaskState}";
                        return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Cannot redirect to result of task with id = {id}. {e.Message}";
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
