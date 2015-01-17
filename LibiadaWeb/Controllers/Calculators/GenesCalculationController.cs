namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The genes calculation controller.
    /// </summary>
    public class GenesCalculationController : AbstractResultController
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
        /// Initializes a new instance of the <see cref="GenesCalculationController"/> class.
        /// </summary>
        public GenesCalculationController() : base("GenesCalculation", "Genes calculation")
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
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="pieceTypeIds">
        /// The piece type ids.
        /// </param>
        /// <param name="sort">
        /// The is sort.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicIds,
            int?[] linkIds,
            int[] notationIds,
            int[] pieceTypeIds,
            bool sort)
        {
            return Action(() =>
            {
                var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
                var matterNames = new List<string>();
                var sequenceProducts = new List<List<string>>();
                var sequencesPositions = new List<List<long>>();
                var sequencePieceTypes = new List<List<string>>();
                var characteristicNames = new List<string>();

                // Перебор всех цепочек; первый уровень массива характеристик
                for (int w = 0; w < matterIds.Length; w++)
                {
                    long matterId = matterIds[w];
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    sequenceProducts.Add(new List<string>());
                    sequencesPositions.Add(new List<long>());
                    sequencePieceTypes.Add(new List<string>());
                    characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                    var notationId = notationIds[w];

                    var sequenceId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;

                    var genes = db.Gene.Where(g => g.SequenceId == sequenceId && pieceTypeIds.Contains(g.PieceTypeId)).Include(g => g.Piece).ToArray();

                    var pieces = genes.Select(g => g.Piece.First()).ToList();

                    var chains = ExtractChains(pieces, sequenceId);

                    // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                    for (int i = 0; i < characteristicIds.Length; i++)
                    {
                        characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                        int characteristicId = characteristicIds[i];
                        int? linkId = linkIds[i];

                        string className = db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;
                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = (Link)(linkId ?? 0);

                        for (int j = 0; j < chains.Count; j++)
                        {
                            long geneId = genes[j].Id;

                            if (!db.Characteristic.Any(c => c.SequenceId == geneId &&
                                                            c.CharacteristicTypeId == characteristicId &&
                                                            ((linkId == null && c.LinkId == null) ||
                                                             (linkId == c.LinkId))))
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

                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(d, (double)characteristic));

                            if (i == 0)
                            {
                                var productId = genes[d].ProductId;
                                var pieceTypeId = genes[d].PieceTypeId;

                                sequenceProducts.Last().Add(productId == null
                                        ? string.Empty
                                        : db.Product.Single(p => productId == p.Id).Name);
                                sequencesPositions.Last().Add(pieces[d].Start);

                                sequencePieceTypes.Last().Add(db.PieceType.Single(p => pieceTypeId == p.Id).Name);
                            }
                        }
                    }
                }

                // подписи для характеристик
                for (int k = 0; k < characteristicIds.Length; k++)
                {
                    int characteristicId = characteristicIds[k];

                    string characteristicType = db.CharacteristicType.Single(c => c.Id == characteristicId).Name;
                    characteristicNames.Add(characteristicType);
                }

                // ранговая сортировка
                if (sort)
                {
                    for (int f = 0; f < matterIds.Length; f++)
                    {
                        for (int p = 0; p < characteristics[f].Count; p++)
                        {
                            SortKeyValuePairList(characteristics[f][p]);
                        }
                    }
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    });
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "sequenceProducts", sequenceProducts },
                    { "sequencesPositions", sequencesPositions },
                    { "sequencePieceTypes", sequencePieceTypes },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds },
                    { "characteristicsList", characteristicsList }
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
