namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The alignment controller.
    /// </summary>
    public class GenesAlignmentController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The gene repository.
        /// </summary>
        private readonly GeneRepository geneRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesAlignmentController"/> class.
        /// </summary>
        public GenesAlignmentController() : base("GenesAlignment", "Genes alignment")
        {
            db = new LibiadaWebEntities();
            geneRepository = new GeneRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var calculatorsHelper = new CalculatorsHelper(db);
            var data = calculatorsHelper.GetGenesCalculationData(2, 2);
            data.Add("minimumSelectedMatters", 2);
            data.Add("maximumSelectedMatters", 2);
            ViewBag.data = data;
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="pieceTypeIds">
        /// The piece type ids.
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
        public ActionResult Index(
            long[] matterIds,
            int characteristicTypeLinkId,
            int notationId,
            int[] pieceTypeIds,
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

                var firstMatterId = matterIds[0];
                var secondMatterId = matterIds[1];

                var firstSequenceCharacteristics = CalculateCharacteristic(firstMatterId, characteristicTypeLinkId, notationId, pieceTypeIds);
                var secondSequenceCharacteristics = CalculateCharacteristic(secondMatterId, characteristicTypeLinkId, notationId, pieceTypeIds);

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
                    var length = longer.Count;
                    for (int i = 0; i < length; i++)
                    {
                        longer.Add(0);
                    }
                }

                var distanceCalculator = GetDistanceCalculator(validationType);
                List<double> distances = new List<double>();
                var optimalRotation = CalculateMeasureForRotation(longer, shorter, distances, distanceCalculator);

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notationId);

                return new Dictionary<string, object>
                {
                    { "firstSequenceName", db.Matter.Single(m => m.Id == firstMatterId).Name },
                    { "secondSequenceName", db.Matter.Single(m => m.Id == secondMatterId).Name },
                    { "characteristicName", characteristicName },
                    { "pieceTypes", db.PieceType.Where(p => pieceTypeIds.Contains(p.Id)).Select(p => p.Name).ToList() },
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
        private static Func<double, double, double> GetDistanceCalculator(string validationType)
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
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="pieceTypeIds">
        /// The piece type ids.
        /// </param>
        /// <returns>
        /// The <see cref="List{Double}"/>.
        /// </returns>
        private List<double> CalculateCharacteristic(
            long matterId,
            int characteristicTypeLinkId,
            int notationId,
            int[] pieceTypeIds)
        {
            var characteristics = new List<double>();

            List<Gene> genes;

            var sequences = geneRepository.ExtractSequences(matterId, notationId, pieceTypeIds, out genes);

            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

            for (int j = 0; j < sequences.Count; j++)
            {
                long geneId = genes[j].Id;

                if (!db.Characteristic.Any(c => c.SequenceId == geneId && c.CharacteristicTypeLinkId == characteristicTypeLinkId))
                {
                    double value = calculator.Calculate(sequences[j], link);
                    var currentCharacteristic = new Characteristic
                    {
                        SequenceId = geneId,
                        CharacteristicTypeLinkId = characteristicTypeLinkId,
                        Value = value,
                        ValueString = value.ToString()
                    };

                    db.Characteristic.Add(currentCharacteristic);
                }
            }

            db.SaveChanges();

            for (int d = 0; d < sequences.Count; d++)
            {
                long geneId = genes[d].Id;
                double characteristic = db.Characteristic.Single(c => c.SequenceId == geneId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value;

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
