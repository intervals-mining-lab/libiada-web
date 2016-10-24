using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using LibiadaWeb.Tasks;

namespace LibiadaWeb.Controllers
{
    [Authorize]
    public class TaskManagerWebApiController : ApiController
    {
        public string GetTaskData(int id)
        {
            var task = TaskManager.GetTask(id);

            if (task.TaskData.TaskState == TaskState.Completed)
            {

                return task.Result["data"].ToString();

            }

            throw new Exception();
        }
    }
}
