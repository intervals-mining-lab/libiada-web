namespace LibiadaWeb.Controllers
{
    using System;
    using System.Web.Http;

    using LibiadaWeb.Tasks;

    /// <summary>
    /// The task manager web api controller.
    /// </summary>
    [Authorize]
    public class TaskManagerWebApiController : ApiController
    {
        /// <summary>
        /// Gets the task data by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if task is not complete.
        /// </exception>
        public string GetTaskData(int id)
        {
            var task = TaskManager.GetTask(id);

            if (task.TaskData.TaskState != TaskState.Completed)
            {
                throw new Exception("Task state is not 'complete'");
            }

            return task.Result["data"].ToString();
        }
    }
}
