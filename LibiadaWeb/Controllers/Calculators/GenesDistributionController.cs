namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The genes distribution controller.
    /// </summary>
    public class GenesDistributionController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The gene repository.
        /// </summary>
        private readonly GeneRepository geneRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesDistributionController"/> class.
        /// </summary>
        public GenesDistributionController() : base("GenesDistribution", "Genes distribution")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            geneRepository = new GeneRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var data = geneRepository.GetGenesCalculationData();
            data.Add("minimumSelectedMatters", 1);
            data.Add("maximumSelectedMatters", int.MaxValue);
            ViewBag.data = data;
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="firstCharacteristicId">
        /// The first characteristic id.
        /// </param>
        /// <param name="firstLinkId">
        /// The first link id.
        /// </param>
        /// <param name="firstNotationId">
        /// The first notation id.
        /// </param>
        /// <param name="secondCharacteristicId">
        /// The second characteristic id.
        /// </param>
        /// <param name="secondLinkId">
        /// The second link id.
        /// </param>
        /// <param name="secondNotationId">
        /// The second notation id.
        /// </param>
        /// <param name="pieceTypeIds">
        /// The piece type ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int firstCharacteristicId,
            int? firstLinkId,
            int firstNotationId,
            int secondCharacteristicId,
            int? secondLinkId,
            int secondNotationId,
            int[] pieceTypeIds)
        {
            return Action(() =>
            {
                var matterNames = new List<string>();

                var fullCharacteristics = new List<double>();

                foreach (var matterId in matterIds)
                {
                    long sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == firstNotationId).Id;

                    if (db.Characteristic.Any(c => ((firstLinkId == null && c.LinkId == null) || (firstLinkId == c.LinkId)) &&
                                              c.SequenceId == sequenceId &&
                                              c.CharacteristicTypeId == firstCharacteristicId))
                    {
                        double dataBaseCharacteristic = db.Characteristic.Single(c => ((firstLinkId == null && c.LinkId == null) || firstLinkId == c.LinkId) &&
                                                                                        c.SequenceId == sequenceId &&
                                                                                        c.CharacteristicTypeId == firstCharacteristicId).Value;
                        fullCharacteristics.Add(dataBaseCharacteristic);
                    }
                    else
                    {
                        Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                        tempChain.FillIntervalManagers();
                        string className =
                            db.CharacteristicType.Single(ct => ct.Id == firstCharacteristicId).ClassName;
                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = (Link)(firstLinkId ?? 0);
                        var characteristicValue = calculator.Calculate(tempChain, link);

                        var dataBaseCharacteristic = new Characteristic
                        {
                            SequenceId = sequenceId,
                            CharacteristicTypeId = firstCharacteristicId,
                            LinkId = firstLinkId,
                            Value = characteristicValue,
                            ValueString = characteristicValue.ToString()
                        };
                        db.Characteristic.Add(dataBaseCharacteristic);
                        db.SaveChanges();
                        fullCharacteristics.Add(characteristicValue);
                    }
                }

                string linkName = firstLinkId.HasValue ? db.Link.Single(l => l.Id == firstLinkId).Name : string.Empty;
                var fullCharacteristicName = string.Join("  ", db.CharacteristicType.Single(c => c.Id == firstCharacteristicId).Name, linkName, db.Notation.Single(n => n.Id == firstNotationId).Name);

                var sequenceProducts = new List<List<string>>();
                var sequencesPositions = new List<List<long>>();
                var sequencePieceTypes = new List<List<string>>();
                var genesCharacteristics = new List<List<KeyValuePair<int, double>>>();

                for (int w = 0; w < matterIds.Length; w++)
                {
                    long matterId = matterIds[w];
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    sequenceProducts.Add(new List<string>());
                    sequencesPositions.Add(new List<long>());
                    sequencePieceTypes.Add(new List<string>());

                    var sequenceId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == secondNotationId).Id;

                    var genes = db.Gene.Where(g => g.SequenceId == sequenceId && pieceTypeIds.Contains(g.PieceTypeId)).Include(g => g.Piece).ToArray();

                    var pieces = genes.Select(g => g.Piece.First()).ToList();

                    var chains = ExtractChains(pieces, sequenceId);

                    genesCharacteristics.Add(new List<KeyValuePair<int, double>>());

                    string className = db.CharacteristicType.Single(c => c.Id == secondCharacteristicId).ClassName;
                    IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                    var link = (Link)(secondLinkId ?? 0);

                    for (int j = 0; j < chains.Count; j++)
                    {
                        long geneId = genes[j].Id;

                        if (!db.Characteristic.Any(c => c.SequenceId == geneId &&
                                                        c.CharacteristicTypeId == secondCharacteristicId &&
                                                        ((secondLinkId == null && c.LinkId == null) ||
                                                         (secondLinkId == c.LinkId))))
                        {
                            double value = calculator.Calculate(chains[j], link);
                            var currentCharacteristic = new Characteristic
                            {
                                SequenceId = geneId,
                                CharacteristicTypeId = secondCharacteristicId,
                                LinkId = secondLinkId,
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
                            c.CharacteristicTypeId == secondCharacteristicId &&
                            ((secondLinkId == null && c.LinkId == null) || (secondLinkId == c.LinkId))).Value;

                        genesCharacteristics.Last().Add(new KeyValuePair<int, double>(d, (double)characteristic));
                        var productId = genes[d].ProductId;
                        var pieceTypeId = genes[d].PieceTypeId;

                        sequenceProducts.Last().Add(productId == null
                                ? string.Empty
                                : db.Product.Single(p => productId == p.Id).Name);
                        sequencesPositions.Last().Add(pieces[d].Start);

                        sequencePieceTypes.Last().Add(db.PieceType.Single(p => pieceTypeId == p.Id).Name);
                    }
                }

                for (int f = 0; f < matterIds.Length; f++)
                {
                    SortKeyValuePairList(genesCharacteristics[f]);
                }

                return new Dictionary<string, object>
                {
                    { "genesCharacteristics", genesCharacteristics },
                    { "matterNames", matterNames },
                    { "sequenceProducts", sequenceProducts },
                    { "sequencesPositions", sequencesPositions },
                    { "sequencePieceTypes", sequencePieceTypes },
                    { "secondCharacteristicName", db.CharacteristicType.Single(c => c.Id == secondCharacteristicId).Name },
                    { "fullCharacteristics", fullCharacteristics }, 
                    { "fullCharacteristicName", fullCharacteristicName }, 
                    { "matterIds", matterIds }
                };
            });
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
        /// The sort key value pair list.
        /// </summary>
        /// <param name="arrayForSort">
        /// The array for sort.
        /// </param>
        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }
    }
}
