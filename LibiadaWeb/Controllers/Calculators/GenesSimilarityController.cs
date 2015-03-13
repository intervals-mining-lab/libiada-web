namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The genes similarity controller.
    /// </summary>
    public class GenesSimilarityController : AbstractResultController
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
        /// Initializes a new instance of the <see cref="GenesSimilarityController"/> class.
        /// </summary>
        public GenesSimilarityController() : base("GenesSimilarity", "Genes similarity")
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
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <param name="maxDifference">
        /// The precision.
        /// </param>
        /// <param name="excludeType">
        /// The exclude type
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if count of matters is not 2.
        /// </exception>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int characteristicTypeLinkId,
            int notationId,
            int[] featureIds,
            string maxDifference,
            string excludeType)
        {
            return Action(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Count of selected matters must be 2.", "matterIds");
                }

                var firstMatterId = matterIds[0];
                var secondMatterId = matterIds[1];

                List<Fragment> firstSequenceFragments;
                List<Fragment> secondSequenceFragments;

                var firstSequences = geneRepository.ExtractSequences(firstMatterId, notationId, featureIds, out firstSequenceFragments);
                var secondSequences = geneRepository.ExtractSequences(secondMatterId, notationId, featureIds, out secondSequenceFragments);

                var firstSequenceCharacteristics = CalculateCharacteristic(characteristicTypeLinkId, firstSequences, firstSequenceFragments);
                var secondSequenceCharacteristics = CalculateCharacteristic(characteristicTypeLinkId, secondSequences, secondSequenceFragments);

                var similarGenes = new List<IntPair>();

                for (int i = 0; i < firstSequenceCharacteristics.Count; i++)
                {
                    for (int j = 0; j < secondSequenceCharacteristics.Count; j++)
                    {
                        if (Math.Abs(firstSequenceCharacteristics[i] - secondSequenceCharacteristics[j]) <= double.Parse(maxDifference, CultureInfo.InvariantCulture))
                        {
                            similarGenes.Add(new IntPair(i, j));

                            if (excludeType == "Exclude")
                            {
                                firstSequenceCharacteristics.RemoveAt(i);
                                secondSequenceCharacteristics.RemoveAt(j);
                            }
                        }
                    }
                }

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notationId);

                return new Dictionary<string, object>
                {
                    { "firstSequenceName", db.Matter.Single(m => m.Id == firstMatterId).Name },
                    { "secondSequenceName", db.Matter.Single(m => m.Id == secondMatterId).Name },
                    { "characteristicName", characteristicName },
                    { "features", db.Feature.Where(p => featureIds.Contains(p.Id)).Select(p => p.Name).ToList() },
                    { "similarGenes", similarGenes },
                    { "firstSequenceGenes", firstSequenceFragments },
                    { "secondSequenceGenes", secondSequenceFragments }
                };
            });
        }

        /// <summary>
        /// The calculate characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="sequences">
        /// The sequences.
        /// </param>
        /// <param name="fragments">
        /// The genes.
        /// </param>
        /// <returns>
        /// The <see cref="List{Fragment}"/>.
        /// </returns>
        private List<double> CalculateCharacteristic(
            int characteristicTypeLinkId,
            List<Chain> sequences,
            List<Fragment> fragments)
        {
            var characteristics = new List<double>();

            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

            for (int j = 0; j < sequences.Count; j++)
            {
                long geneId = fragments[j].Id;

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
                long geneId = fragments[d].Id;
                double characteristic = db.Characteristic.Single(c => c.SequenceId == geneId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value;

                characteristics.Add(characteristic);
            }

            return characteristics;
        }
    }
}
