using Segmenter.Model;

namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using Segmenter.Model.Criterion;
    using Segmenter.Model.Seekers;
    using Segmenter.Model.Threshold;

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
            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "thresholds", EnumHelper.GetSelectList(typeof(Threshold)) },
                    { "segmentationCriterion", EnumHelper.GetSelectList(typeof(SegmentationCriterion)) },
                    { "deviationCalculationMethods", EnumHelper.GetSelectList(typeof(DeviationCalculationMethod)) }
                });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            string[] customSequences,
            HttpPostedFileBase[] file,
            double leftBorder,
            double rightBorder,
            double step,
            double precision,
            Threshold threshold,
            int wordLengthDecrement,
            SegmentationCriterion segmentationCriterion,
            int wordLength,
            DeviationCalculationMethod deviationCalculationMethod,
            int balance)
        {
            return CreateTask(() =>
            {
                string chain;
                string chainName;



                Input inputData = new Input
                {
                    Seeker = deviationCalculationMethod,
                    Algorithm = 0,
                    Balance = balance,
                    Chain = chain,
                    ChainName = chainName,
                    LeftBound = leftBorder,
                    RightBound = rightBorder,
                    Precision = precision,
                    Step = step,
                    StopCriterion = segmentationCriterion,
                    ThresholdMethod = threshold,
                    WindowDecrement = wordLengthDecrement,
                    WindowLength = wordLength
                };


                var segmenter = new Algorithm(inputData);

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