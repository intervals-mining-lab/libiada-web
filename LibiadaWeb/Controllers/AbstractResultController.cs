namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using LibiadaWeb.Tasks;

    /// <summary>
    /// The abstract result controller.
    /// </summary>
    public abstract class AbstractResultController : Controller
    {
        /// <summary>
        /// The task type.
        /// </summary>
        private readonly TaskType taskType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractResultController"/> class.
        /// </summary>
        /// <param name="taskType">
        /// The task Type.
        /// </param>
        protected AbstractResultController(TaskType taskType)
        {
            this.taskType = taskType;
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <param name="taskId">
        /// The task Id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Result(string taskId)
        {
            try
            {
                var result = TempData["result"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("No data.");
                }

                foreach (string key in result.Keys)
                {
                    ViewData[key] = key == "data" || key == "additionalData" ? "{}" : result[key];
                }

                TempData.Keep();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);

                ViewBag.Error = true;

                ViewBag.ErrorMessage = e.Message;
            }

            ViewBag.taskId = taskId;
            return View();
        }

        /// <summary>
        /// Creates new task in task manager.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        protected ActionResult CreateTask(Func<Dictionary<string, object>> action)
        {
            TaskManager.CreateTask(action, taskType);
            return RedirectToAction("Index", "TaskManager");
        }
    }
}
