using System;
using System.Collections.Generic;
using System.Web.Mvc;
using LibiadaWeb.Maintenance;

namespace LibiadaWeb.Controllers
{
    public abstract class AbstractResultController : Controller
    {
        protected string ControllerName;
        protected string DisplayName;

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if there is no data.
        /// </exception>
        public ActionResult Result()
        {
            try
            {
                var result = TempData["result"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("No data.");
                }

                foreach (var key in result.Keys)
                {
                    ViewData[key] = result[key];
                }

                TempData.Keep();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);

                ViewBag.Error = true;

                ViewBag.ErrorMessage = e.Message;
            }

            return View();
        }

        protected ActionResult Action(Func<Dictionary<string, object>> action)
        {
            int taskId = TaskManager.GetId();
            var task = new Task(action, taskId)
            {
                ControllerName = ControllerName,
                TaskData = { DisplayName = DisplayName }
            };

            TaskManager.AddTask(task);
            return RedirectToAction("Index", "TaskManager", new { id = taskId });
        }

    }
}