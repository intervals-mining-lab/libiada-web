namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using LibiadaWeb.Tasks;

    /// <summary>
    /// Abstract parent controller for all tasks controllers (calculators, etc.).
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
        protected AbstractResultController(TaskType taskType) => this.taskType = taskType;

        /// <summary>
        /// The result.
        /// </summary>
        /// <param name="id">
        /// The task Id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Result(int id)
        {
            try
            {
                Task task = TaskManager.Instance.GetTask(id);
                switch (task.TaskData.TaskState)
                {
                    case TaskState.Completed:
                    case TaskState.Error:
                        Dictionary<string, object> result = task.Result;
                        if (result == null)
                        {
                            throw new Exception("No data.");
                        }

                        foreach (string key in result.Keys)
                        {
                            ViewData[key] = key == "data" || key == "additionalData" ? "{}" : result[key];
                        }

                        break;
                    default:
                        throw new Exception($"Task with id = {id} is not completed, current status is {task.TaskData.TaskState}");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);

                ViewBag.Error = true;

                ViewBag.ErrorMessage = e.Message;
            }

            ViewBag.taskId = id;
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
            TaskManager.Instance.CreateTask(action, taskType);
            return RedirectToAction("Index", "TaskManager");
        }
    }
}
