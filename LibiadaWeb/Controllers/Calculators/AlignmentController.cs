namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The alignment controller.
    /// </summary>
    public class AlignmentController : AbstractResultController
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
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlignmentController"/> class.
        /// </summary>
        public AlignmentController()
            : base("Alignment", "Genes alignment")
        {
            db = new LibiadaWebEntities();
            geneRepository = new GeneRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);

            ViewBag.data = geneRepository.GetGenesCalculationData();
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
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
            int characteristicId,
            int? linkId,
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

                var firstSequenceCharacteristics = CalculateCharacteristic(firstMatterId, characteristicId, linkId, notationId, pieceTypeIds);
                var secondSequenceCharacteristics = CalculateCharacteristic(secondMatterId, characteristicId, linkId, notationId, pieceTypeIds);

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
                    for (int i = 0; i < longer.Count; i++)
                    {
                        longer.Add(0);
                    }
                }

                var distanceCalculator = GetDistanceCalculator(validationType);
                List<double> distances = new List<double>();
                var optimalRotation = CalculateMeasureForRotation(longer, shorter, distances, distanceCalculator);

                return new Dictionary<string, object>
                {
                    { "firstSequenceName", db.Matter.Single(m => m.Id == firstMatterId).Name },
                    { "secondSequenceName", db.Matter.Single(m => m.Id == secondMatterId).Name },
                    { "characteristicName", db.CharacteristicType.Single(c => c.Id == characteristicId).Name },
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
                case "Equality":
                    distanceCalculator = (first, second) => -Math.Abs(Math.Min(first, second));
                    break;
                case "Difference":
                    distanceCalculator = (first, second) => Math.Abs(first - second);
                    break;
                case "NormalizedDifference":
                    distanceCalculator = (first, second) => Math.Abs((first - second) / (first + second));
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
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
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
            int characteristicId,
            int? linkId,
            int notationId,
            int[] pieceTypeIds)
        {
            var characteristics = new List<double>();

            var sequenceId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;

            var genes = db.Gene.Where(g => g.SequenceId == sequenceId && pieceTypeIds.Contains(g.PieceTypeId)).Include(g => g.Piece).ToArray();

            var pieces = genes.Select(g => g.Piece.First()).ToList();

            var sequences = ExtractChains(pieces, sequenceId);

            string className = db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = (Link)(linkId ?? 0);

            for (int j = 0; j < sequences.Count; j++)
            {
                long geneId = genes[j].Id;

                if (!db.Characteristic.Any(c => c.SequenceId == geneId &&
                                                c.CharacteristicTypeId == characteristicId &&
                                                ((linkId == null && c.LinkId == null) || (linkId == c.LinkId))))
                {
                    double value = calculator.Calculate(sequences[j], link);
                    var currentCharacteristic = new Characteristic
                    {
                        SequenceId = geneId,
                        CharacteristicTypeId = characteristicId,
                        LinkId = linkId,
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
                double characteristic = db.Characteristic.Single(c =>
                    c.SequenceId == geneId &&
                    c.CharacteristicTypeId == characteristicId &&
                    ((linkId == null && c.LinkId == null) || (linkId == c.LinkId))).Value;

                characteristics.Add(characteristic);
            }

            return characteristics;
        }

        /// <summary>
        /// The extract chains.
        /// </summary>
        /// <param name="pieces">
        /// The pieces.
        /// </param>
        /// <param name="chainId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        private List<Chain> ExtractChains(List<Piece> pieces, long chainId)
        {
            var starts = pieces.Select(p => p.Start).ToList();

            var stops = pieces.Select(p => p.Start + p.Length).ToList();

            BaseChain parentChain = commonSequenceRepository.ToLibiadaBaseChain(chainId);

            var iterator = new DefaultCutRule(starts, stops);

            var stringChains = DiffCutter.Cut(parentChain.ToString(), iterator);

            var chains = new List<Chain>();

            for (int i = 0; i < stringChains.Count; i++)
            {
                chains.Add(new Chain(stringChains[i]));
            }

            return chains;
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
            double minimumDistance = double.MaxValue;
            for (int i = 0; i < first.Count; i++)
            {
                var distance = Measure(first, second, measure);
                if (minimumDistance > distance)
                {
                    optimalRotation = i;
                    minimumDistance = distance;
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
