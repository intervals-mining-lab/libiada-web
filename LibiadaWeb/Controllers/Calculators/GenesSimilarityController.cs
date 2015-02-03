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

    using LibiadaWeb.Models;
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
        /// Initializes a new instance of the <see cref="GenesSimilarityController"/> class.
        /// </summary>
        public GenesSimilarityController()
            : base("GenesSimilarity", "Genes similarity")
        {
            db = new LibiadaWebEntities();
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
        /// <param name="maxDifference">
        /// The precision.
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
            int characteristicId,
            int? linkId,
            int notationId,
            int[] pieceTypeIds,
            string maxDifference)
        {
            return Action(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Count of selected matters must be 2.", "matterIds");
                }

                var firstMatterId = matterIds[0];
                var secondMatterId = matterIds[1];

                List<Gene> firstSequenceGenes;
                List<Gene> secondSequenceGenes;

                var firstSequences = geneRepository.ExtractSequences(firstMatterId, notationId, pieceTypeIds, out firstSequenceGenes);
                var secondSequences = geneRepository.ExtractSequences(secondMatterId, notationId, pieceTypeIds, out secondSequenceGenes);

                var firstSequenceCharacteristics = CalculateCharacteristic(characteristicId, linkId, firstSequences, firstSequenceGenes);
                var secondSequenceCharacteristics = CalculateCharacteristic(characteristicId, linkId, secondSequences, secondSequenceGenes);

                var similarGenes = new List<IntPair>();

                for (int i = 0; i < firstSequenceCharacteristics.Count; i++)
                {
                    for (int j = 0; j < secondSequenceCharacteristics.Count; j++)
                    {
                        if (Math.Abs(firstSequenceCharacteristics[i] - secondSequenceCharacteristics[j]) < double.Parse(maxDifference, CultureInfo.InvariantCulture))
                        {
                            similarGenes.Add(new IntPair(i, j));
                        }
                    }
                }

                return new Dictionary<string, object>
                {
                    { "firstSequenceName", db.Matter.Single(m => m.Id == firstMatterId).Name },
                    { "secondSequenceName", db.Matter.Single(m => m.Id == secondMatterId).Name },
                    { "characteristicName", db.CharacteristicType.Single(c => c.Id == characteristicId).Name },
                    { "pieceTypes", db.PieceType.Where(p => pieceTypeIds.Contains(p.Id)).Select(p => p.Name).ToList() },
                    { "similarGenes", similarGenes },
                    { "firstSequenceGenes", firstSequenceGenes },
                    { "secondSequenceGenes", secondSequenceGenes }
                };
            });
        }

        /// <summary>
        /// The calculate characteristic.
        /// </summary>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
        /// </param>
        /// <param name="sequences">
        /// The sequences.
        /// </param>
        /// <param name="genes">
        /// The genes.
        /// </param>
        /// <returns>
        /// The <see cref="List{Gene}"/>.
        /// </returns>
        private List<double> CalculateCharacteristic(
            int characteristicId,
            int? linkId,
            List<Chain> sequences,
            List<Gene> genes)
        {
            var characteristics = new List<double>();

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
    }
}
