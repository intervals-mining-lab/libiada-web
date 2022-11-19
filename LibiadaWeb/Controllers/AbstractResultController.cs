namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

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
        /// The task id in database.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Result(int id)
        {
            try
            {
                Task task = TaskManager.Instance.GetTask(id);
                var taskStatus = task.TaskData.TaskState;
                if (taskStatus != TaskState.Completed && taskStatus != TaskState.Error)
                {
                    throw new Exception($"Task with id = {id} is not complete, current status is {taskStatus}");
                } else if(taskStatus == TaskState.Error)
                {
                    ViewBag.Error = true;
                    using(var db = new LibiadaWebEntities())
                    {
                        ViewBag.Error = JsonConvert.DeserializeObject(db.TaskResult.Single(tr => tr.TaskId == id && tr.Key == "Error").Value);
                    }
                }
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
        /// Creates new task in task manager.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        protected ActionResult CreateTask(Func<Dictionary<string, string>> action)
        {
            long taskId = TaskManager.Instance.CreateTask(action, taskType);
            return RedirectToAction(taskId.ToString(), "TaskManager");
        }
    }
}
