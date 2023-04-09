namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Bio.Extensions;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using static Libiada.Database.Models.Calculators.SubsequencesCharacteristicsCalculator;
    using Libiada.Database;
    using LibiadaWeb.Tasks;

    /// <summary>
    /// The alignment controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequencesAlignmentController : AbstractResultController
    {
        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesAlignmentController"/> class.
        /// </summary>
        public SequencesAlignmentController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper, ITaskManager taskManager) : base(TaskType.SequencesAlignment, taskManager)
        {
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
            this.db = db;
            this.viewDataHelper = viewDataHelper;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(2, 2, "Align"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristic id.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <param name="validationType">
        /// The validation type.
        /// </param>
        /// <param name="cyclicShift">
        /// The cyclic shift.
        /// </param>
        /// <param name="sort">
        /// The sort.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if validationType is unknown.
        /// Or if count of matters is not 2.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            short characteristicLinkId,
            Notation notation,
            Feature[] features,
            string validationType,
            bool cyclicShift,
            bool sort)
        {
            return CreateTask(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Count of selected matters must be 2.", nameof(matterIds));
                }

                string firstMatterName;
                string secondMatterName;
                long firstParentId;
                long secondParentId;

                long firstMatterId = matterIds[0];
                firstMatterName = Cache.GetInstance().Matters.Single(m => m.Id == firstMatterId).Name;
                firstParentId = db.CommonSequence.Single(c => c.MatterId == firstMatterId && c.Notation == notation).Id;

                long secondMatterId = matterIds[1];
                secondMatterName = Cache.GetInstance().Matters.Single(m => m.Id == secondMatterId).Name;
                secondParentId = db.CommonSequence.Single(c => c.MatterId == secondMatterId && c.Notation == notation).Id;


                double[] firstSequenceCharacteristics = CalculateSubsequencesCharacteristics(firstParentId, characteristicLinkId, features);
                double[] secondSequenceCharacteristics = CalculateSubsequencesCharacteristics(secondParentId, characteristicLinkId, features);

                if (sort)
                {
                    Array.Sort(firstSequenceCharacteristics, (x, y) => y.CompareTo(x));
                    Array.Sort(secondSequenceCharacteristics, (x, y) => y.CompareTo(x));
                }

                List<double> longer;
                List<double> shorter;
                if (firstSequenceCharacteristics.Length >= secondSequenceCharacteristics.Length)
                {
                    longer = firstSequenceCharacteristics.ToList();
                    shorter = secondSequenceCharacteristics.ToList();
                }
                else
                {
                    longer = secondSequenceCharacteristics.ToList();
                    shorter = firstSequenceCharacteristics.ToList();
                }

                if (!cyclicShift)
                {
                    int count = longer.Count;
                    for (int i = 0; i < count; i++)
                    {
                        longer.Add(0);
                    }
                }

                var distanceCalculator = GetDistanceCalculator(validationType);
                var distances = new List<double>();
                int optimalRotation = CalculateMeasureForRotation(longer, shorter, distances, distanceCalculator);

                string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

                var result = new Dictionary<string, object>
                {
                    { "firstSequenceName", firstMatterName },
                    { "secondSequenceName", secondMatterName },
                    { "characteristicName", characteristicName },
                    { "features", features.ConvertAll(p => p.GetDisplayValue()) },
                    { "optimalRotation", optimalRotation },
                    { "distances", distances.Select(el => new {Value = el}) },
                    { "validationType", validationType },
                    { "cyclicShift", cyclicShift },
                    { "sort", sort }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }

        /// <summary>
        /// The get distance calculator.
        /// </summary>
        /// <param name="validationType">
        /// The validation type.
        /// </param>
        /// <returns>
        /// The <see cref="Func{Double, Double, Double}"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if validation type is unknown.
        /// </exception>
        [NonAction]
        private Func<double, double, double> GetDistanceCalculator(string validationType)
        {
            switch (validationType)
            {
                case "Similarity":
                    return (first, second) => Math.Abs(Math.Min(first, second));
                case "Difference":
                    return (first, second) => -Math.Abs(first - second);
                case "NormalizedDifference":
                    return (first, second) => -Math.Abs((first - second) / (first + second));
                case "Equality":
                    return (first, second) => Math.Abs(first - second) < (Math.Abs(first + second) / 20) ? 1 : 0;
                default:
                    throw new ArgumentException("unknown validation type", nameof(validationType));
            }
        }

        /// <summary>
        /// The calculate measure for rotation.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="distances">
        /// The distances.
        /// </param>
        /// <param name="measure">
        /// The measure.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [NonAction]
        private int CalculateMeasureForRotation(List<double> first, List<double> second, List<double> distances, Func<double, double, double> measure)
        {
            int optimalRotation = 0;
            double maximumEquality = 0;
            for (int i = 0; i < first.Count; i++)
            {
                var distance = Measure(first, second, measure);
                if (maximumEquality < distance)
                {
                    optimalRotation = i;
                    maximumEquality = distance;
                }

                distances.Add(distance);
                first = Rotate(first);
            }

            return optimalRotation;
        }

        /// <summary>
        /// The rotate.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The <see cref="List{Double}"/>.
        /// </returns>

        [NonAction]
        private List<double> Rotate(List<double> list)
        {
            var first = list[0];
            list.RemoveAt(0);
            list.Add(first);
            return list;
        }

        /// <summary>
        /// The measure.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="measure">
        /// The measurer.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        [NonAction]
        private double Measure(List<double> first, List<double> second, Func<double, double, double> measure)
        {
            double result = 0;
            for (int i = 0; i < second.Count; i++)
            {
                if ((first[i] * second[i]) > 0)
                {
                    result += measure(first[i], second[i]);
                }
            }

            return result;
        }
    }
}
