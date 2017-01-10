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
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences similarity controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesSimilarityController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The subsequence extractor.
        /// </summary>
        private readonly SubsequenceExtractor subsequenceExtractor;

        /// <summary>
        /// The sequence attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesSimilarityController"/> class.
        /// </summary>
        public SubsequencesSimilarityController() : base("Subsequences similarity")
        {
            db = new LibiadaWebEntities();
            subsequenceExtractor = new SubsequenceExtractor(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(2, 2, "Compare"));
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
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <param name="features">
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
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int characteristicTypeLinkId,
            Notation notation,
            Feature[] features,
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
                var firstParentSequenceId = db.CommonSequence.Single(c => c.MatterId == firstMatterId && c.Notation == notation).Id;
                Subsequence[] firstSequenceSubsequences = subsequenceExtractor.GetSubsequences(firstParentSequenceId, features);
                var firstSequences = subsequenceExtractor.ExtractChains(firstSequenceSubsequences, firstParentSequenceId);
                var firstSequenceCharacteristics = CalculateCharacteristic(characteristicTypeLinkId, firstSequences, firstSequenceSubsequences);
                var firstDbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(firstSequenceSubsequences.Select(s => s.Id));
                var firstSequenceAttributes = new List<AttributeValue[]>();
                foreach (var subsequence in firstSequenceSubsequences)
                {
                    AttributeValue[] attributes;
                    if (!firstDbSubsequencesAttributes.TryGetValue(subsequence.Id, out attributes))
                    {
                        attributes = new AttributeValue[0];
                    }

                    firstSequenceAttributes.Add(attributes);
                }

                var secondMatterId = matterIds[1];
                var secondParentSequenceId = db.CommonSequence.Single(c => c.MatterId == secondMatterId && c.Notation == notation).Id;
                Subsequence[] secondSequenceSubsequences = subsequenceExtractor.GetSubsequences(secondParentSequenceId, features);
                var secondSequences = subsequenceExtractor.ExtractChains(secondSequenceSubsequences, secondParentSequenceId);
                var secondSequenceCharacteristics = CalculateCharacteristic(characteristicTypeLinkId, secondSequences, secondSequenceSubsequences);
                var secondDbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(secondSequenceSubsequences.Select(s => s.Id));
                var secondSequenceAttributes = new List<AttributeValue[]>();
                foreach (var subsequence in secondSequenceSubsequences)
                {
                    AttributeValue[] attributes;
                    if (!secondDbSubsequencesAttributes.TryGetValue(subsequence.Id, out attributes))
                    {
                        attributes = new AttributeValue[0];
                    }

                    secondSequenceAttributes.Add(attributes);
                }

                double difference = double.Parse(maxDifference, CultureInfo.InvariantCulture);

                var similarSubsequences = new List<IntPair>();

                for (int i = 0; i < firstSequenceCharacteristics.Count; i++)
                {
                    for (int j = 0; j < secondSequenceCharacteristics.Count; j++)
                    {
                        if (Math.Abs(firstSequenceCharacteristics[i] - secondSequenceCharacteristics[j]) <= difference)
                        {
                            similarSubsequences.Add(new IntPair(i, j));

                            if (excludeType == "Exclude")
                            {
                                firstSequenceCharacteristics[i] = double.NaN;
                                secondSequenceCharacteristics[j] = double.NaN;
                            }
                        }
                    }
                }

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notation);

                var similarity = similarSubsequences.Count * 200d / (firstSequenceSubsequences.Length + secondSequenceSubsequences.Length);

                var firstSequenceSimilarity = similarSubsequences.Count * 100d / firstSequenceSubsequences.Length;

                var secondSequenceSimilarity = similarSubsequences.Count * 100d / secondSequenceSubsequences.Length;

                return new Dictionary<string, object>
                {
                    { "firstSequenceName", db.Matter.Single(m => m.Id == firstMatterId).Name },
                    { "secondSequenceName", db.Matter.Single(m => m.Id == secondMatterId).Name },
                    { "characteristicName", characteristicName },
                    { "similarSubsequences", similarSubsequences },
                    { "similarity", similarity },
                    { "firstSequenceSimilarity", firstSequenceSimilarity },
                    { "secondSequenceSimilarity", secondSequenceSimilarity },
                    { "firstSequenceSubsequences", firstSequenceSubsequences },
                    { "secondSequenceSubsequences", secondSequenceSubsequences },
                    { "firstSequenceAttributes", firstSequenceAttributes },
                    { "secondSequenceAttributes", secondSequenceAttributes }
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
        /// <param name="subsequences">
        /// The subsequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        private List<double> CalculateCharacteristic(int characteristicTypeLinkId, Chain[] sequences, Subsequence[] subsequences)
        {
            var characteristics = new List<double>();
            var newCharacteristics = new List<Characteristic>();
            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

            for (int j = 0; j < sequences.Length; j++)
            {
                long subsequenceId = subsequences[j].Id;

                if (!db.Characteristic.Any(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId))
                {
                    double value = calculator.Calculate(sequences[j], link);
                    var currentCharacteristic = new Characteristic
                    {
                        SequenceId = subsequenceId,
                        CharacteristicTypeLinkId = characteristicTypeLinkId,
                        Value = value
                    };
                    newCharacteristics.Add(currentCharacteristic);
                }
            }

            db.Characteristic.AddRange(newCharacteristics);
            db.SaveChanges();

            for (int d = 0; d < sequences.Length; d++)
            {
                long subsequenceId = subsequences[d].Id;
                double characteristic = db.Characteristic.Single(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value;

                characteristics.Add(characteristic);
            }

            return characteristics;
        }
    }
}
