namespace LibiadaWeb.Controllers
{
    using LibiadaWeb.Maintenance;
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
            Dictionary<int, TaskState> tasks = new Dictionary<int, TaskState>();
            foreach (var task in TaskManager.Tasks)
            {
                tasks.Add(task.Key, task.Value.TaskState);
            }
            ViewBag.Tasks = tasks;
            return View();
        }

    }
}