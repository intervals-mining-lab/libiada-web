namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

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
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesSimilarityController"/> class.
        /// </summary>
        public SubsequencesSimilarityController() : base(TaskType.SubsequencesSimilarity)
        {
            db = new LibiadaWebEntities();
            subsequenceExtractor = new SubsequenceExtractor(db);
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
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
        /// <param name="characteristicLinkId">
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
            short characteristicLinkId,
            Notation notation,
            Feature[] features,
            string maxDifference,
            string excludeType)
        {
            return CreateTask(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Count of selected matters must be 2.", nameof(matterIds));
                }

                long firstMatterId = matterIds[0];
                long firstParentSequenceId = db.CommonSequence.Single(c => c.MatterId == firstMatterId && c.Notation == notation).Id;
                Subsequence[] firstSequenceSubsequences = subsequenceExtractor.GetSubsequences(firstParentSequenceId, features);
                List<double> firstSequenceCharacteristics = CalculateCharacteristic(characteristicLinkId, firstSequenceSubsequences);
                Dictionary<long, AttributeValue[]> firstDbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(firstSequenceSubsequences.Select(s => s.Id));
                var firstSequenceAttributes = new List<AttributeValue[]>();
                foreach (Subsequence subsequence in firstSequenceSubsequences)
                {
                    firstDbSubsequencesAttributes.TryGetValue(subsequence.Id, out AttributeValue[] attributes);
                    attributes = attributes ?? new AttributeValue[0];
                    firstSequenceAttributes.Add(attributes);
                }

                long secondMatterId = matterIds[1];
                long secondParentSequenceId = db.CommonSequence.Single(c => c.MatterId == secondMatterId && c.Notation == notation).Id;
                Subsequence[] secondSequenceSubsequences = subsequenceExtractor.GetSubsequences(secondParentSequenceId, features);
                List<double> secondSequenceCharacteristics = CalculateCharacteristic(characteristicLinkId, secondSequenceSubsequences);
                Dictionary<long, AttributeValue[]> secondDbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(secondSequenceSubsequences.Select(s => s.Id));
                var secondSequenceAttributes = new List<AttributeValue[]>();
                foreach (Subsequence subsequence in secondSequenceSubsequences)
                {
                    secondDbSubsequencesAttributes.TryGetValue(subsequence.Id, out AttributeValue[] attributes);
                    attributes = attributes ?? new AttributeValue[0];
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

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

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
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="subsequences">
        /// The subsequences.
        /// </param>
        /// <returns>
        /// The <see cref="List{Subsequence}"/>.
        /// </returns>
        private List<double> CalculateCharacteristic(short characteristicLinkId, Subsequence[] subsequences)
        {
            var characteristics = new List<double>();
            var newCharacteristics = new List<CharacteristicValue>();
            FullCharacteristic fullCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(fullCharacteristic);
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            var subsequenceIds = subsequences.Select(s => s.Id);
            var existingCharacteristics = db.CharacteristicValue
                                            .Where(cv => cv.CharacteristicLinkId == characteristicLinkId && subsequenceIds.Contains(cv.SequenceId))
                                            .ToDictionary(cv=> cv.SequenceId);
            Dictionary<long, Chain> sequences = subsequenceExtractor.GetSubsequencesSequences(subsequences);
            for (int i = 0; i < subsequences.Length; i++)
            {
                if (existingCharacteristics.ContainsKey(subsequences[i].Id))
                {
                    characteristics.Add(existingCharacteristics[subsequences[i].Id].Value);
                }
                else
                {
                    double value = calculator.Calculate(sequences[subsequences[i].Id], link);
                    var currentCharacteristic = new CharacteristicValue
                    {
                        SequenceId = subsequences[i].Id,
                        CharacteristicLinkId = characteristicLinkId,
                        Value = value
                    };
                    newCharacteristics.Add(currentCharacteristic);
                    characteristics.Add(value);
                }
            }

            db.CharacteristicValue.AddRange(newCharacteristics);
            db.SaveChanges();

            return characteristics;
        }
    }
}
