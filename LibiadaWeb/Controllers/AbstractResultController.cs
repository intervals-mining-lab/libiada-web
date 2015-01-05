namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using LibiadaWeb.Maintenance;

    /// <summary>
    /// The abstract result controller.
    /// </summary>
    public abstract class AbstractResultController : Controller
    {
        /// <summary>
        /// The controller name.
        /// </summary>
        protected readonly string ControllerName;

        /// <summary>
        /// The display name.
        /// </summary>
        protected readonly string DisplayName;

        protected AbstractResultController(string controllerName, string displayName)
        {
            ControllerName = controllerName;
            DisplayName = displayName;
        }

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

        /// <summary>
        /// The action method.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        protected ActionResult Action(Func<Dictionary<string, object>> action)
        {
            int taskId = TaskManager.GetId();
            var task = new Task(taskId, action, ControllerName, DisplayName);

            TaskManager.AddTask(task);
            return RedirectToAction("Index", "TaskManager", new { id = taskId });
        }

    }
}