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
        /// <param name="firstMatterId">
        /// Id of first matter.
        /// </param>
        /// <param name="secondMatterId">
        /// Id of second matter.
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
        /// </exception>
        [HttpPost]
        public ActionResult Index(
            long firstMatterId,
            long secondMatterId,
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
                var firstSequenceCharacteristics = new List<KeyValuePair<int, double>>();
                var firstSequenceName = db.Matter.Single(m => m.Id == firstMatterId).Name;
                var firstSequenceProducts = new List<string>();
                var firstSequencePositions = new List<long>();
                var firstSequencePieceTypes = new List<string>();

                var secondSequenceCharacteristics = new List<KeyValuePair<int, double>>();
                var secondSequenceName = db.Matter.Single(m => m.Id == firstMatterId).Name;
                var secondSequenceProducts = new List<string>();
                var secondSequencePositions = new List<long>();
                var secondSequencePieceTypes = new List<string>();

                CalculateCharacteristic(firstMatterId, characteristicId, linkId, notationId, pieceTypeIds, firstSequenceCharacteristics, firstSequenceProducts, firstSequencePositions, firstSequencePieceTypes);
                CalculateCharacteristic(secondMatterId, characteristicId, linkId, notationId, pieceTypeIds, secondSequenceCharacteristics, secondSequenceProducts, secondSequencePositions, secondSequencePieceTypes);

                if (sort)
                {
                    firstSequenceCharacteristics = firstSequenceCharacteristics.OrderByDescending(v => v.Value).ToList();
                    secondSequenceCharacteristics = secondSequenceCharacteristics.OrderByDescending(v => v.Value).ToList();
                }
                
                int optimalRotation;

                if (firstSequenceCharacteristics.Count >= secondSequenceCharacteristics.Count)
                {
                    var tempFirstSequenceCharacteristics = new List<KeyValuePair<int, double>>(firstSequenceCharacteristics);
                    if (!cyclicShift)
                    {
                        for (int i = 0; i < firstSequenceCharacteristics.Count; i++)
                        {
                            tempFirstSequenceCharacteristics.Add(new KeyValuePair<int, double>(0, 0));
                        }
                    }

                    switch (validationType)
                    {
                        case "Equality":
                            optimalRotation = FindMaximumEqualityRotation(tempFirstSequenceCharacteristics, secondSequenceCharacteristics);
                            break;
                        case "Difference":
                            optimalRotation = FindMinimumDifferenceRotation(tempFirstSequenceCharacteristics, secondSequenceCharacteristics);
                            break;
                        case "NormalizedDifference":
                            optimalRotation = FindMinimumNormalizedDifferenceRotation(tempFirstSequenceCharacteristics, secondSequenceCharacteristics);
                            break;
                        default:
                            throw new ArgumentException("unknown validation type");
                    }
                }
                else
                {
                    var tempSecondSequenceCharacteristics = new List<KeyValuePair<int, double>>(secondSequenceCharacteristics);
                    if (!cyclicShift)
                    {
                        for (int i = 0; i < secondSequenceCharacteristics.Count; i++)
                        {
                            tempSecondSequenceCharacteristics.Add(new KeyValuePair<int, double>(0, 0));
                        }
                    }

                    switch (validationType)
                    {
                        case "Equality":
                            optimalRotation = FindMaximumEqualityRotation(tempSecondSequenceCharacteristics, firstSequenceCharacteristics);
                            break;
                        case "Difference":
                            optimalRotation = FindMinimumDifferenceRotation(tempSecondSequenceCharacteristics, firstSequenceCharacteristics);
                            break;
                        case "NormalizedDifference":
                            optimalRotation = FindMinimumNormalizedDifferenceRotation(tempSecondSequenceCharacteristics, firstSequenceCharacteristics);
                            break;
                        default:
                            throw new ArgumentException("unknown validation type");
                    }
                }

                string characteristicName = db.CharacteristicType.Single(c => c.Id == characteristicId).Name;

                return new Dictionary<string, object>
                {
                    { "firstSequenceCharacteristics", firstSequenceCharacteristics },
                    { "firstSequenceName", firstSequenceName },
                    { "firstSequenceProducts", firstSequenceProducts },
                    { "firstSequencePositions", firstSequencePositions },
                    { "firstSequencePieceTypes", firstSequencePieceTypes },
                    { "secondSequenceCharacteristics", secondSequenceCharacteristics },
                    { "secondSequenceName", secondSequenceName },
                    { "secondSequenceProducts", secondSequenceProducts },
                    { "secondSequencePositions", secondSequencePositions },
                    { "secondSequencePieceTypes", secondSequencePieceTypes },
                    { "characteristicName", characteristicName },
                    { "firstMatterId", firstMatterId },
                    { "secondMatterId", secondMatterId },
                    { "optimalRotation", optimalRotation },
                    { "validationType", validationType }
                };
            });
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
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        /// <param name="sequenceProducts">
        /// The sequence products.
        /// </param>
        /// <param name="sequencePositions">
        /// The sequence positions.
        /// </param>
        /// <param name="sequencePieceTypes">
        /// The sequence piece types.
        /// </param>
        private void CalculateCharacteristic(
            long matterId,
            int characteristicId,
            int? linkId,
            int notationId,
            int[] pieceTypeIds,
            List<KeyValuePair<int, double>> characteristics,
            List<string> sequenceProducts,
            List<long> sequencePositions,
            List<string> sequencePieceTypes)
        {
            var sequenceId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;

            var genes =
                db.Gene.Where(g => g.SequenceId == sequenceId && pieceTypeIds.Contains(g.PieceTypeId)).Include(g => g.Piece).ToArray();

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
                    db.SaveChanges();
                }
            }

            for (int d = 0; d < sequences.Count; d++)
            {
                long geneId = genes[d].Id;
                double characteristic = db.Characteristic.Single(c =>
                    c.SequenceId == geneId &&
                    c.CharacteristicTypeId == characteristicId &&
                    ((linkId == null && c.LinkId == null) || (linkId == c.LinkId))).Value;

                characteristics.Add(new KeyValuePair<int, double>(d, characteristic));

                var productId = genes[d].ProductId;
                var pieceTypeId = genes[d].PieceTypeId;

                sequenceProducts.Add(productId == null ? string.Empty : db.Product.Single(p => productId == p.Id).Name);
                sequencePositions.Add(pieces[d].Start);

                sequencePieceTypes.Add(db.PieceType.Single(p => pieceTypeId == p.Id).Name);
            }
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
        /// The find maximum equality rotation.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int FindMaximumEqualityRotation(List<KeyValuePair<int, double>> first, List<KeyValuePair<int, double>> second)
        {
            int optimal = 0;
            double match = 0;
            for (int i = 0; i < first.Count; i++)
            {
                double currentMatch = CalculateMatch(first, second);
                if (currentMatch > match)
                {
                    match = currentMatch;
                    optimal = i;
                }

                first = Rotate(first);
            }

            return optimal;
        }

        /// <summary>
        /// The find minimum difference rotation.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int FindMinimumDifferenceRotation(List<KeyValuePair<int, double>> first, List<KeyValuePair<int, double>> second)
        {
            int optimal = 0;
            double difference = double.MaxValue;
            for (int i = 0; i < first.Count; i++)
            {
                double currentDifference = CalculateDifference(first, second);
                if (currentDifference < difference)
                {
                    difference = currentDifference;
                    optimal = i;
                }

                first = Rotate(first);
            }

            return optimal;
        }

        /// <summary>
        /// The find minimum normalized difference rotation.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int FindMinimumNormalizedDifferenceRotation(List<KeyValuePair<int, double>> first, List<KeyValuePair<int, double>> second)
        {
            int optimal = 0;
            double difference = double.MaxValue;
            for (int i = 0; i < first.Count; i++)
            {
                double currentDifference = CalculateNormalizedDifference(first, second);
                if (currentDifference < difference)
                {
                    difference = currentDifference;
                    optimal = i;
                }

                first = Rotate(first);
            }

            return optimal;
        }

        /// <summary>
        /// The rotate.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<KeyValuePair<int, double>> Rotate(List<KeyValuePair<int, double>> list)
        {
            KeyValuePair<int, double> first = list[0];
            list.RemoveAt(0);
            list.Add(first);
            return list;
        }

        /// <summary>
        /// The calculate match.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double CalculateMatch(List<KeyValuePair<int, double>> first, List<KeyValuePair<int, double>> second)
        {
            double result = 0;
            for (int i = 0; i < second.Count; i++)
            {
                if (first[i].Value * second[i].Value > 0)
                {
                    result += Math.Abs(Math.Min(first[i].Value, second[i].Value));
                }
            }

            return result;
        }

        /// <summary>
        /// The calculate difference.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double CalculateDifference(List<KeyValuePair<int, double>> first, List<KeyValuePair<int, double>> second)
        {
            double result = 0;
            for (int i = 0; i < second.Count; i++)
            {
                if (first[i].Value * second[i].Value > 0)
                {
                    result += Math.Abs(first[i].Value - second[i].Value);
                }
            }

            return result;
        }

        /// <summary>
        /// The calculate normalized difference.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double CalculateNormalizedDifference(List<KeyValuePair<int, double>> first, List<KeyValuePair<int, double>> second)
        {
            double result = 0;
            for (int i = 0; i < second.Count; i++)
            {
                if (first[i].Value * second[i].Value > 0)
                {
                    result += Math.Abs((first[i].Value - second[i].Value) / (first[i].Value + second[i].Value));
                }
            }

            return result;
        }
    }
}
