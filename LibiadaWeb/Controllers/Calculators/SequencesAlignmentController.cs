namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The alignment controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequencesAlignmentController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The subsequence extractor.
        /// </summary>
        private readonly SubsequenceExtractor subsequenceExtractor;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesAlignmentController"/> class.
        /// </summary>
        public SequencesAlignmentController() : base(TaskType.SequencesAlignment)
        {
            db = new LibiadaWebEntities();
            subsequenceExtractor = new SubsequenceExtractor(db);
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
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
            return Action(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Count of selected matters must be 2.", "matterIds");
                }

                long firstMatterId = matterIds[0];
                long secondMatterId = matterIds[1];

                var firstSequenceCharacteristics = CalculateCharacteristic(firstMatterId, characteristicLinkId, notation, features);
                var secondSequenceCharacteristics = CalculateCharacteristic(secondMatterId, characteristicLinkId, notation, features);

                if (sort)
                {
                    firstSequenceCharacteristics = firstSequenceCharacteristics.OrderByDescending(v => v).ToList();
                    secondSequenceCharacteristics = secondSequenceCharacteristics.OrderByDescending(v => v).ToList();
                }

                List<double> longer;
                List<double> shorter;
                if (firstSequenceCharacteristics.Count >= secondSequenceCharacteristics.Count)
                {
                    longer = firstSequenceCharacteristics;
                    shorter = secondSequenceCharacteristics;
                }
                else
                {
                    longer = secondSequenceCharacteristics;
                    shorter = firstSequenceCharacteristics;
                }

                if (!cyclicShift)
                {
                    int length = longer.Count;
                    for (int i = 0; i < length; i++)
                    {
                        longer.Add(0);
                    }
                }

                var distanceCalculator = GetDistanceCalculator(validationType);
                var distances = new List<double>();
                int optimalRotation = CalculateMeasureForRotation(longer, shorter, distances, distanceCalculator);

                string characteristicName = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkId, notation);

                return new Dictionary<string, object>
                {
                    { "firstSequenceName", db.Matter.Single(m => m.Id == firstMatterId).Name },
                    { "secondSequenceName", db.Matter.Single(m => m.Id == secondMatterId).Name },
                    { "characteristicName", characteristicName },
                    { "features", features.Select(p => p.GetDisplayValue()).ToList() },
                    { "optimalRotation", optimalRotation },
                    { "distances", distances },
                    { "validationType", validationType },
                    { "cyclicShift", cyclicShift },
                    { "sort", sort }
                };
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
        private Func<double, double, double> GetDistanceCalculator(string validationType)
        {
            Func<double, double, double> distanceCalculator;
            switch (validationType)
            {
                case "Similarity":
                    distanceCalculator = (first, second) => Math.Abs(Math.Min(first, second));
                    break;
                case "Difference":
                    distanceCalculator = (first, second) => -Math.Abs(first - second);
                    break;
                case "NormalizedDifference":
                    distanceCalculator = (first, second) => -Math.Abs((first - second) / (first + second));
                    break;
                case "Equality":
                    distanceCalculator = (first, second) => Math.Abs(first - second) < (Math.Abs(first + second) / 20) ? 1 : 0;
                    break;
                default:
                    throw new ArgumentException("unknown validation type");
            }

            return distanceCalculator;
        }

        /// <summary>
        /// The calculate characteristic.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="List{Double}"/>.
        /// </returns>
        private List<double> CalculateCharacteristic(long matterId, short characteristicLinkId, Notation notation, Feature[] features)
        {
            var characteristics = new List<double>();
            var newCharacteristics = new List<CharacteristicValue>();
            long parentSequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;

            Subsequence[] subsequences = subsequenceExtractor.GetSubsequences(parentSequenceId, features);

            Chain[] sequences = subsequenceExtractor.ExtractChains(parentSequenceId, subsequences);

            FullCharacteristic fullCharacteristic = characteristicTypeLinkRepository.GetFullCharacteristic(characteristicLinkId);
            IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(fullCharacteristic);
            Link link = characteristicTypeLinkRepository.GetLinkForFullCharacteristic(characteristicLinkId);

            for (int j = 0; j < sequences.Length; j++)
            {
                long subsequenceId = subsequences[j].Id;

                if (!db.CharacteristicValue.Any(c => c.SequenceId == subsequenceId && c.CharacteristicLinkId == characteristicLinkId))
                {
                    double value = calculator.Calculate(sequences[j], link);
                    var currentCharacteristic = new CharacteristicValue
                    {
                        SequenceId = subsequenceId,
                        CharacteristicLinkId = characteristicLinkId,
                        Value = value
                    };
                    newCharacteristics.Add(currentCharacteristic);
                }
            }

            db.CharacteristicValue.AddRange(newCharacteristics);
            db.SaveChanges();

            for (int d = 0; d < sequences.Length; d++)
            {
                long subsequenceId = subsequences[d].Id;
                double characteristic = db.CharacteristicValue.Single(c => c.SequenceId == subsequenceId && c.CharacteristicLinkId == characteristicLinkId).Value;

                characteristics.Add(characteristic);
            }

            return characteristics;
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
