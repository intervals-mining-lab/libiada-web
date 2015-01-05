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
    using LibiadaWeb.Models.Repositories.Chains;

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
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlignmentController"/> class.
        /// </summary>
        public AlignmentController() : base("Alignment", "Genes alignment")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
            chainRepository = new ChainRepository(db);
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

            var chainIds = db.gene.Select(g => g.chain_id).Distinct();
            var chains = db.dna_chain.Where(c => chainIds.Contains(c.id));
            var matterIds = chains.Select(c => c.matter_id);

            var matters = db.matter.Where(m => matterIds.Contains(m.id));

            var characteristicsList = db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var pieceTypeIds = db.piece_type.Where(p => p.nature_id == Aliases.Nature.Genetic
                                         && p.id != Aliases.PieceType.FullGenome
                                         && p.id != Aliases.PieceType.ChloroplastGenome
                                         && p.id != Aliases.PieceType.MitochondrionGenome).Select(p => p.id);

            var links = new SelectList(db.link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", new SelectList(matters, "id", "name") }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", new SelectList(db.notation.Where(n => n.nature_id == Aliases.Nature.Genetic), "id", "name") }, 
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
                var firstChainCharacteristics = new List<KeyValuePair<int, double>>();
                var firstChainName = db.matter.Single(m => m.id == matterId1).name;
                var firstChainProducts = new List<string>();
                var firstChainPositions = new List<long>();
                var firstChainPieceTypes = new List<string>();

                var secondChainCharacteristics = new List<KeyValuePair<int, double>>();
                var secondChainName = db.matter.Single(m => m.id == matterId1).name;
                var secondChainProducts = new List<string>();
                var secondChainPositions = new List<long>();
                var secondChainPieceTypes = new List<string>();

                CalculateCharacteristic(matterId1, characteristicId, linkId, notationId, pieceTypeIds, firstChainCharacteristics, firstChainProducts, firstChainPositions, firstChainPieceTypes);
                CalculateCharacteristic(matterId2, characteristicId, linkId, notationId, pieceTypeIds, secondChainCharacteristics, secondChainProducts, secondChainPositions, secondChainPieceTypes);

                int optimalRotation = 0;

                if (firstChainCharacteristics.Count >= secondChainCharacteristics.Count)
                {
                    switch (validationType)
                    {
                        case "Equality":
                            optimalRotation = FindMaximumEqualityRotation(firstChainCharacteristics, secondChainCharacteristics);
                            break;
                        case "Difference":
                            optimalRotation = FindMinimumDifferenceRotation(firstChainCharacteristics, secondChainCharacteristics);
                            break;
                        case "NormalizedDifference":
                            optimalRotation = FindMinimumNormalizedDifferenceRotation(firstChainCharacteristics, secondChainCharacteristics);
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
                            optimalRotation = FindMaximumEqualityRotation(secondChainCharacteristics, firstChainCharacteristics);
                            break;
                        case "Difference":
                            optimalRotation = FindMinimumDifferenceRotation(secondChainCharacteristics, firstChainCharacteristics);
                            break;
                        case "NormalizedDifference":
                            optimalRotation = FindMinimumNormalizedDifferenceRotation(secondChainCharacteristics, firstChainCharacteristics);
                            break;
                        default:
                            throw new ArgumentException("unknown validation type");
                    }
                }

                string characteristicName = db.characteristic_type.Single(c => c.id == characteristicId).name;

                return new Dictionary<string, object>
                {
                    { "firstChainCharacteristics", firstChainCharacteristics },
                    { "firstChainName", firstChainName },
                    { "firstChainProducts", firstChainProducts },
                    { "firstChainPositions", firstChainPositions },
                    { "firstChainPieceTypes", firstChainPieceTypes },
                    { "secondChainCharacteristics", secondChainCharacteristics },
                    { "secondChainName", secondChainName },
                    { "secondChainProducts", secondChainProducts },
                    { "secondChainPositions", secondChainPositions },
                    { "secondChainPieceTypes", secondChainPieceTypes },
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
            var chainId = db.dna_chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;

            var genes =
                db.gene.Where(g => g.chain_id == chainId && pieceTypeIds.Contains(g.piece_type_id)).Include("piece").ToArray();

            var pieces = genes.Select(g => g.piece.First()).ToList();

            var chains = ExtractChains(pieces, chainId);

            string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = linkId != null ? (Link)db.link.Single(l => l.id == linkId).id : Link.None;

            for (int j = 0; j < chains.Count; j++)
            {
                long geneId = genes[j].id;

                if (!db.characteristic.Any(c => c.chain_id == geneId &&
                                                c.characteristic_type_id == characteristicId &&
                                                ((linkId == null && c.link_id == null) || (linkId == c.link_id))))
                {
                    double value = calculator.Calculate(chains[j], link);
                    var currentCharacteristic = new characteristic
                    {
                        chain_id = geneId,
                        characteristic_type_id = characteristicId,
                        link_id = linkId,
                        value = value,
                        value_string = value.ToString()
                    };

                    db.characteristic.Add(currentCharacteristic);
                    db.SaveChanges();
                }
            }

            for (int d = 0; d < chains.Count; d++)
            {
                long geneId = genes[d].id;
                double? characteristic = db.characteristic.Single(c =>
                    c.chain_id == geneId &&
                    c.characteristic_type_id == characteristicId &&
                    ((linkId == null && c.link_id == null) || (linkId == c.link_id))).value;

                characteristics.Add(new KeyValuePair<int, double>(d, (double)characteristic));

                var productId = genes[d].product_id;
                var pieceTypeId = genes[d].piece_type_id;

                chainProducts.Add(productId == null ? string.Empty : db.product.Single(p => productId == p.id).name);
                chainPositions.Add(pieces[d].start);

                chainPieceTypes.Add(db.piece_type.Single(p => pieceTypeId == p.id).name);
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
        private List<Chain> ExtractChains(List<piece> pieces, long chainId)
        {
            var starts = pieces.Select(p => p.start).ToList();

            var stops = pieces.Select(p => p.start + p.length).ToList();

            BaseChain parentChain = chainRepository.ToLibiadaBaseChain(chainId);

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
