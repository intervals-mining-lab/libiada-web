namespace LibiadaWeb.Controllers
{
    using Maintenance;
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// The calculation controller.
    /// </summary>
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
            var tasks = new List<TaskData>();
            var tasksFromManager = TaskManager.Tasks;
            foreach (var task in tasksFromManager)
            {
                lock (task)
                {
                    tasks.Add(task.TaskData);
                }
            }
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.Tasks = tasks;
            return View();
        }

        public ActionResult RedirectToResult(int id)
        {
            var task = TaskManager.GetTask(id);
            lock (task)
            {
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

        }

    }
}