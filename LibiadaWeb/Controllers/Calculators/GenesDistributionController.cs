namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;
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
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicTypeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesDistributionController"/> class.
        /// </summary>
        public GenesDistributionController()
            : base("GenesDistribution", "Genes distribution")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            geneRepository = new GeneRepository(db);
            characteristicTypeRepository = new CharacteristicTypeRepository(db);
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
                var result = new List<SequenceCharacteristics>();

                var sequenceIds = db.DnaSequence.Where(c => matterIds.Contains(c.MatterId) && c.NotationId == secondNotationId).Select(c => c.Id).ToList();

                double maxGenes = 0;

                for (int w = 0; w < matterIds.Length; w++)
                {
                    long matterId = matterIds[w];
                    var matterName = db.Matter.Single(m => m.Id == matterId).Name;


                    long sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == firstNotationId).Id;

                    double sequenceCharacteristic;

                    if (db.Characteristic.Any(c => ((firstLinkId == null && c.LinkId == null) || (firstLinkId == c.LinkId)) &&
                                              c.SequenceId == sequenceId &&
                                              c.CharacteristicTypeId == firstCharacteristicId))
                    {
                        sequenceCharacteristic = db.Characteristic.Single(c => ((firstLinkId == null && c.LinkId == null) || firstLinkId == c.LinkId) &&
                                                                                        c.SequenceId == sequenceId &&
                                                                                        c.CharacteristicTypeId == firstCharacteristicId).Value;
                    }
                    else
                    {
                        Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                        tempChain.FillIntervalManagers();
                        string fullClassName = db.CharacteristicType.Single(ct => ct.Id == firstCharacteristicId).ClassName;
                        IFullCalculator fullCalculator = CalculatorsFactory.CreateFullCalculator(fullClassName);
                        var fullLink = (Link)(firstLinkId ?? 0);
                        sequenceCharacteristic = fullCalculator.Calculate(tempChain, fullLink);

                        var dataBaseCharacteristic = new Characteristic
                        {
                            SequenceId = sequenceId,
                            CharacteristicTypeId = firstCharacteristicId,
                            LinkId = firstLinkId,
                            Value = sequenceCharacteristic,
                            ValueString = sequenceCharacteristic.ToString()
                        };
                        db.Characteristic.Add(dataBaseCharacteristic);
                        db.SaveChanges();
                    }

                    List<Gene> genes;
                    var genesSequences = geneRepository.ExtractSequences(sequenceIds[w], pieceTypeIds, out genes);

                    if (maxGenes < genes.Count)
                    {
                        maxGenes = genes.Count;
                    }

                    var genesCharacteristics = new List<GeneCharacteristic>();
                    string className = db.CharacteristicType.Single(c => c.Id == secondCharacteristicId).ClassName;
                    IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                    var link = (Link)(secondLinkId ?? 0);

                    for (int j = 0; j < genesSequences.Count; j++)
                    {
                        long geneId = genes[j].Id;

                        if (!db.Characteristic.Any(c => c.SequenceId == geneId &&
                                                        c.CharacteristicTypeId == secondCharacteristicId &&
                                                        ((secondLinkId == null && c.LinkId == null) ||
                                                         (secondLinkId == c.LinkId))))
                        {
                            double value = calculator.Calculate(genesSequences[j], link);
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

                    for (int d = 0; d < genesSequences.Count; d++)
                    {
                        long geneId = genes[d].Id;
                        double characteristic = db.Characteristic.Single(c =>
                            c.SequenceId == geneId &&
                            c.CharacteristicTypeId == secondCharacteristicId &&
                            ((secondLinkId == null && c.LinkId == null) || (secondLinkId == c.LinkId))).Value;

                        var geneCharacteristic = new GeneCharacteristic(genes[d], characteristic);
                        genesCharacteristics.Add(geneCharacteristic);
                    }

                    var sequenceCharacteristics = new SequenceCharacteristics(matterName, sequenceCharacteristic, genesCharacteristics);
                    result.Add(sequenceCharacteristics);
                }

                var fullCharacteristicName = characteristicTypeRepository.GetCharacteristicName(firstCharacteristicId, firstLinkId, firstNotationId);
                var genesCharacteristicName = characteristicTypeRepository.GetCharacteristicName(secondCharacteristicId, secondLinkId, secondNotationId);

                return new Dictionary<string, object>
                {
                    { "result", result },
                    { "maxGenes", maxGenes },
                    { "genesCharacteristicName", genesCharacteristicName },
                    { "fullCharacteristicName", fullCharacteristicName }
                };
            });
        }
    }
}
