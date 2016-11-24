namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using LibiadaWeb.Models.Account;
    using LibiadaWeb.Tasks;

    /// <summary>
    /// The abstract result controller.
    /// </summary>
    public abstract class AbstractResultController : Controller
    {
        /// <summary>
        /// The controller name.
        /// </summary>
        private readonly string controllerName;

        /// <summary>
        /// The display name.
        /// </summary>
        private readonly string displayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractResultController"/> class.
        /// </summary>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        protected AbstractResultController(string displayName)
        {
            this.controllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            this.displayName = displayName;
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
        public virtual ActionResult Result(string taskId)
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
                    ViewData[key] = key == "data" ? "{}" : result[key];
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
            var task = new Task(taskId, action, controllerName, displayName, UserHelper.GetUserId());

            TaskManager.AddTask(task);
            return RedirectToAction("Index", "TaskManager");
        }
    }
}
