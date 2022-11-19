namespace LibiadaWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web.Http;

    using LibiadaWeb.Helpers;
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
        /// The task id in database.
        /// </param>
        /// <param name="key">
        /// Name of the parameter in task results.
        /// </param>
        /// <returns>
        /// The json as <see cref="string"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if task is not complete.
        /// </exception>
        public string GetTaskData(long id, string key = "data")
        {
            try
            {
                return TaskManager.Instance.GetTaskData(id, key);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message += $"{Environment.NewLine} {ex.Message}";
                }

                return JsonConvert.SerializeObject(new { Status = "Error", Message = message });
            }
        }

        /// <summary>
        /// Get subsequences comparer data element.
        /// </summary>
        /// <param name="taskId">
        /// The task id in database.
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
        public string GetSubsequencesComparerDataElement(int taskId, int firstIndex, int secondIndex, bool filtered)
        {
            try
            {
                var taskData = GetTaskData(taskId, filtered ? "filteredSimilarityMatrix" : "similarityMatrix");

                var similarityMatrix = JsonConvert.DeserializeObject<List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,]>(taskData);

                List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)> result = similarityMatrix[firstIndex, secondIndex];

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message += $"{Environment.NewLine} {ex.Message}";
                }

                return JsonConvert.SerializeObject(new { Status = "Error", Message = message });
            }
        }

        /// <summary>
        /// Subscribes a user to receive push notifications.
        /// </summary>
        /// <param name="subscriberData">
        /// Subscriber data that contains endpoint, pubic key and private key. 
        /// </param>
        [HttpPost]
        public void Subscribe(AspNetPushNotificationSubscriber subscriberData)
        {
            using (var db = new LibiadaWebEntities())
            {
                var subscriber = new AspNetPushNotificationSubscriber
                {
                    Auth = subscriberData.Auth,
                    P256dh = subscriberData.P256dh,
                    Endpoint = subscriberData.Endpoint,
                    UserId = AccountHelper.GetUserId()
                };

                db.AspNetPushNotificationSubscribers.Add(subscriber);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Unsubscribes a divice to not receive push notifications.
        /// </summary>
        /// <param name="Unsubscribe">
        /// Endpoint of the user device.
        /// </param>
        [HttpPost]
        public void Unsubscribe(AspNetPushNotificationSubscriber subscriberData)
        {
            using (var db = new LibiadaWebEntities())
            {
                string endpoint = subscriberData.Endpoint;
                int userId = AccountHelper.GetUserId();
                AspNetPushNotificationSubscriber subscriber = db.AspNetPushNotificationSubscribers.Single(s => s.Endpoint == endpoint
                                                                                                            && s.UserId == userId);
                db.AspNetPushNotificationSubscribers.Remove(subscriber);
                db.SaveChanges();
            }
        }

        public string GetApplicationServerKey()
        {
            try
            {
                var response = new { applicationServerKey = ConfigurationManager.AppSettings["PublicVapidKey"] };
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    message += $"{Environment.NewLine} {ex.Message}";
                }

                return JsonConvert.SerializeObject(new { Status = "Error", Message = message });
            }
        }
    }
}
