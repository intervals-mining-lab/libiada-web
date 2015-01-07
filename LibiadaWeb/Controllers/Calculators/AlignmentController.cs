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
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
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
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

        /// <summary>
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlignmentController"/> class.
        /// </summary>
        public AlignmentController() : base("Alignment", "Genes alignment")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        // GET: Alignment
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);

            var sequenceIds = db.Gene.Select(g => g.SequenceId).Distinct();
            var sequences = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id));
            var matterIds = sequences.Select(c => c.MatterId);

            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var pieceTypeIds = db.PieceType.Where(p => p.NatureId == Aliases.Nature.Genetic
                                         && p.Id != Aliases.PieceType.FullGenome
                                         && p.Id != Aliases.PieceType.ChloroplastGenome
                                         && p.Id != Aliases.PieceType.MitochondrionGenome).Select(p => p.Id);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", new SelectList(matters, "id", "name") }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", new SelectList(db.Notation.Where(n => n.NatureId == Aliases.Nature.Genetic), "id", "name") }, 
                    { "links", links }, 
                    { "matterCheckBoxes", matterRepository.GetSelectListItems(matters, null) }, 
                    { "pieceTypesCheckBoxes", pieceTypeRepository.GetSelectListWithNature(pieceTypeIds, pieceTypeIds) }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId1">
        /// The matter id 1.
        /// </param>
        /// <param name="matterId2">
        /// The matter id 2.
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
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if validationType is unknown.
        /// </exception>
        [HttpPost]
        public ActionResult Index(
            long matterId1,
            long matterId2,
            int characteristicId,
            int? linkId,
            int notationId,
            int[] pieceTypeIds,
            string validationType)
        {
            return Action(() =>
            {
                var firstSequenceCharacteristics = new List<KeyValuePair<int, double>>();
                var firstSequenceName = db.Matter.Single(m => m.Id == matterId1).Name;
                var firstSequenceProducts = new List<string>();
                var firstSequencePositions = new List<long>();
                var firstSequencePieceTypes = new List<string>();

                var secondSequenceCharacteristics = new List<KeyValuePair<int, double>>();
                var secondSequenceName = db.Matter.Single(m => m.Id == matterId1).Name;
                var secondSequenceProducts = new List<string>();
                var secondSequencePositions = new List<long>();
                var secondSequencePieceTypes = new List<string>();

                CalculateCharacteristic(matterId1, characteristicId, linkId, notationId, pieceTypeIds, firstSequenceCharacteristics, firstSequenceProducts, firstSequencePositions, firstSequencePieceTypes);
                CalculateCharacteristic(matterId2, characteristicId, linkId, notationId, pieceTypeIds, secondSequenceCharacteristics, secondSequenceProducts, secondSequencePositions, secondSequencePieceTypes);

                int optimalRotation = 0;

                if (firstSequenceCharacteristics.Count >= secondSequenceCharacteristics.Count)
                {
                    switch (validationType)
                    {
                        case "Equality":
                            optimalRotation = FindMaximumEqualityRotation(firstSequenceCharacteristics, secondSequenceCharacteristics);
                            break;
                        case "Difference":
                            optimalRotation = FindMinimumDifferenceRotation(firstSequenceCharacteristics, secondSequenceCharacteristics);
                            break;
                        case "NormalizedDifference":
                            optimalRotation = FindMinimumNormalizedDifferenceRotation(firstSequenceCharacteristics, secondSequenceCharacteristics);
                            break;
                        default:
                            throw new ArgumentException("unknown validation type");
                    }
                }
                else
                {
                    switch (validationType)
                    {
                        case "Equality":
                            optimalRotation = FindMaximumEqualityRotation(secondSequenceCharacteristics, firstSequenceCharacteristics);
                            break;
                        case "Difference":
                            optimalRotation = FindMinimumDifferenceRotation(secondSequenceCharacteristics, firstSequenceCharacteristics);
                            break;
                        case "NormalizedDifference":
                            optimalRotation = FindMinimumNormalizedDifferenceRotation(secondSequenceCharacteristics, firstSequenceCharacteristics);
                            break;
                        default:
                            throw new ArgumentException("unknown validation type");
                    }
                }

                string characteristicName = db.CharacteristicType.Single(c => c.Id == characteristicId).Name;

                return new Dictionary<string, object>
                {
                    { "firstChainCharacteristics", firstSequenceCharacteristics },
                    { "firstChainName", firstSequenceName },
                    { "firstChainProducts", firstSequenceProducts },
                    { "firstChainPositions", firstSequencePositions },
                    { "firstChainPieceTypes", firstSequencePieceTypes },
                    { "secondChainCharacteristics", secondSequenceCharacteristics },
                    { "secondChainName", secondSequenceName },
                    { "secondChainProducts", secondSequenceProducts },
                    { "secondChainPositions", secondSequencePositions },
                    { "secondChainPieceTypes", secondSequencePieceTypes },
                    { "characteristicName", characteristicName },
                    { "matterId1", matterId1 },
                    { "matterId2", matterId2 },
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
        /// <param name="chainProducts">
        /// The chain products.
        /// </param>
        /// <param name="chainPositions">
        /// The chain positions.
        /// </param>
        /// <param name="chainPieceTypes">
        /// The chain piece types.
        /// </param>
        private void CalculateCharacteristic(
            long matterId,
            int characteristicId,
            int? linkId,
            int notationId,
            int[] pieceTypeIds,
            List<KeyValuePair<int, double>> characteristics,
            List<string> chainProducts,
            List<long> chainPositions,
            List<string> chainPieceTypes)
        {
            var chainId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;

            var genes =
                db.Gene.Where(g => g.SequenceId == chainId && pieceTypeIds.Contains(g.PieceTypeId)).Include("piece").ToArray();

            var pieces = genes.Select(g => g.Piece.First()).ToList();

            var chains = ExtractChains(pieces, chainId);

            string className = db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = (Link)(linkId ?? 0);

            for (int j = 0; j < chains.Count; j++)
            {
                long geneId = genes[j].Id;

                if (!db.Characteristic.Any(c => c.SequenceId == geneId &&
                                                c.CharacteristicTypeId == characteristicId &&
                                                ((linkId == null && c.LinkId == null) || (linkId == c.LinkId))))
                {
                    double value = calculator.Calculate(chains[j], link);
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

            for (int d = 0; d < chains.Count; d++)
            {
                long geneId = genes[d].Id;
                double? characteristic = db.Characteristic.Single(c =>
                    c.SequenceId == geneId &&
                    c.CharacteristicTypeId == characteristicId &&
                    ((linkId == null && c.LinkId == null) || (linkId == c.LinkId))).Value;

                characteristics.Add(new KeyValuePair<int, double>(d, (double)characteristic));

                var productId = genes[d].ProductId;
                var pieceTypeId = genes[d].PieceTypeId;

                chainProducts.Add(productId == null ? string.Empty : db.Product.Single(p => productId == p.Id).Name);
                chainPositions.Add(pieces[d].Start);

                chainPieceTypes.Add(db.PieceType.Single(p => pieceTypeId == p.Id).Name);
            }
        }

        /// <summary>
        /// The extract chains.
        /// </summary>
        /// <param name="pieces">
        /// The pieces.
        /// </param>
        /// <param name="chainId">
        /// The chain id.
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
        private int FindMaximumEqualityRotation(
            List<KeyValuePair<int, double>> first,
            List<KeyValuePair<int, double>> second)
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
        private int FindMinimumDifferenceRotation(
            List<KeyValuePair<int, double>> first,
            List<KeyValuePair<int, double>> second)
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
        private int FindMinimumNormalizedDifferenceRotation(
            List<KeyValuePair<int, double>> first,
            List<KeyValuePair<int, double>> second)
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
                    result += System.Math.Abs(System.Math.Min(first[i].Value, second[i].Value));
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
                    result += System.Math.Abs(first[i].Value - second[i].Value);
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
                    result += System.Math.Abs((first[i].Value - second[i].Value) / (first[i].Value + second[i].Value));
                }
            }

            return result;
        }
    }
}
