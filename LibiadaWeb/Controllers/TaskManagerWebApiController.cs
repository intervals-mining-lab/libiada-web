namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

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
            Task task = TaskManager.GetTask(id);

            if (task.TaskData.TaskState != TaskState.Completed)
            {
                throw new Exception("Task state is not 'complete'");
            }

            return task.Result["data"].ToString();
        }

        /// <summary>
        /// Get subsequences comparer data element.
        /// </summary>
        /// <param name="taskId">
        /// The task id.
        /// </param>
        /// <param name="firstIndex">
        /// The first sequence index.
        /// </param>
        /// <param name="secondIndex">
        /// The second sequence index.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if task is not complete or doesn't have additional data.
        /// </exception>
        public string GetSubsequencesComparerDataElement(int taskId, int firstIndex, int secondIndex)
        {
            Task task = TaskManager.GetTask(taskId);

            if (task.TaskData.TaskState != TaskState.Completed)
            {
                throw new Exception("Task state is not 'complete'");
            }

            if (!task.Result.ContainsKey("additionalData"))
            {
                throw new Exception("Task doesn't have additional data");
            }

            List<(int, int, double)> result = ((List<(int, int, double)>[,])task.Result["additionalData"])[firstIndex, secondIndex];

            return JsonConvert.SerializeObject(result);
        }
    }
}
