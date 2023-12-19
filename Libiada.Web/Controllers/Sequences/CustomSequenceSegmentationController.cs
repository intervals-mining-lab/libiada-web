﻿using Segmenter.Model;

namespace Libiada.Web.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using Libiada.Web.Helpers;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using Segmenter.Model.Criterion;
    using Segmenter.Model.Seekers;
    using Segmenter.Model.Threshold;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The custom sequence segmentation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CustomSequenceSegmentationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceSegmentationController"/> class.
        /// </summary>
        public CustomSequenceSegmentationController(ITaskManager taskManager) : base(TaskType.CustomSequenceSegmentation, taskManager)
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
                { "thresholds", Extensions.EnumExtensions.GetSelectList<Threshold>() },
                { "segmentationCriteria", Extensions.EnumExtensions.GetSelectList<SegmentationCriterion>() },
                { "deviationCalculationMethods", Extensions.EnumExtensions.GetSelectList<DeviationCalculationMethod>() },
                {"imageTransformers", Extensions.EnumExtensions.GetSelectList<ImageTransformer>() }
            });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            string[] customSequences,
            bool localFile,
            string leftBorder,
            string rightBorder,
            string step,
            string precision,
            Threshold threshold,
            int wordLengthDecrement,
            SegmentationCriterion segmentationCriterion,
            int wordLength,
            DeviationCalculationMethod deviationCalculationMethod,
            int balance,
            IFormFileCollection files)
        {
            return CreateTask(() =>
            {
                int sequencesCount = localFile ? files.Count : customSequences.Length;

                var sequencesNames = new string[sequencesCount];
                var sequences = new string[sequencesCount];
                var results = new object[sequencesCount];

                for (int i = 0; i < sequencesCount; i++)
                {
                    if (localFile)
                    {
                        sequencesNames[i] = files[i].FileName;

                        using Stream sequenceStream = FileHelper.GetFileStream(files[i]);
                        using var sr = new StreamReader(sequenceStream);
                        sequences[i] = sr.ReadToEnd();

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
                        LeftBound = double.Parse(leftBorder, CultureInfo.InvariantCulture),
                        RightBound = double.Parse(rightBorder, CultureInfo.InvariantCulture),
                        Precision = double.Parse(precision, CultureInfo.InvariantCulture),
                        Step = double.Parse(step, CultureInfo.InvariantCulture),
                        StopCriterion = segmentationCriterion,
                        ThresholdMethod = threshold,
                        WindowDecrement = wordLengthDecrement,
                        WindowLength = wordLength
                    };


                    var segmenter = new Algorithm(inputData);

                    segmenter.Slot();
                    results[i] = segmenter.Upload();
                }

                var result = new Dictionary<string, object>
                {
                    { "result", results }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}