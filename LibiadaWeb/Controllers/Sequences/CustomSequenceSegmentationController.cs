using Segmenter.Model;

namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Helpers;
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
            HttpPostedFileBase[] files,
            bool localFile,
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
                int sequencesCount = localFile ? Request.Files.Count : customSequences.Length;

                var sequencesNames = new string[sequencesCount];
                var sequences = new string[sequencesCount];


                for (int i = 0; i < sequencesCount; i++)
                {
                    if (localFile)
                    {
                        sequencesNames[i] = Request.Files[i].FileName;

                        Stream sequenceStream = FileHelper.GetFileStream(Request.Files[i]);
                        using (var sr = new StreamReader(sequenceStream))
                        {
                            sequences[i] = sr.ReadToEnd();
                        }
                    }
                    else
                    {
                        sequencesNames[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                        sequences[i] = customSequences[i];
                    }

                    var inputData = new Input
                    {
                        Seeker = deviationCalculationMethod,
                        Algorithm = 0,
                        Balance = balance,
                        Chain = sequences[i],
                        ChainName = sequencesNames[i],
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

                    segmenter.Run();
                }

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