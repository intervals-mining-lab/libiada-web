namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The custom sequence segmentation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CustomSequenceSegmentationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceSegmentationController"/> class.
        /// </summary>
        public CustomSequenceSegmentationController() : base(TaskType.Segmentation)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="customSequences">
        /// The custom sequences.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            string[] customSequences,
            HttpPostedFileBase[] file)
        {
            return CreateTask(() =>
            {
                var result = new Dictionary<string, object>
                {

                };
                return new Dictionary<string, object>
                       {
                           { "data", JsonConvert.SerializeObject(result) }
                       };
            });
        }
    }
}