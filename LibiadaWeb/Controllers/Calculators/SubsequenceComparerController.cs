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
    using Newtonsoft.Json;

    /// <summary>
    /// The subsequence comparer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequenceComparerController : AbstractResultController
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
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequenceComparerController"/> class.
        /// </summary>
        public SubsequenceComparerController()
            : base("Subsequence comparer")
        {
            db = new LibiadaWebEntities();
            subsequenceExtractor = new SubsequenceExtractor(db);
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
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(2, int.MaxValue, "Compare"));
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
        /// <param name="characteristicValueFrom">
        /// Minimum value for calculating characteristic
        /// </param>
        /// <param name="characteristicValueTo">
        /// Maximum value for calculating characteristic
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if count of matters is not 2.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int characteristicTypeLinkId,
            int notationId,
            int[] featureIds,
            string maxDifference,
            string excludeType,
            double characteristicValueFrom,
            double characteristicValueTo)
        {
            return Action(() =>
            {
                var allSubsequencesCharacteristics = new List<KeyValuePair<string, double>[]>();
                int mattersCount = matterIds.Length;
                int[] subsequencesCount = new int[mattersCount];
                string[] matterNames = db.Matter.Where(m => matterIds.Contains(m.Id)).Select(m => m.Name).ToArray();

                for (int i = 0; i < mattersCount; i++)
                {
                    long matterId = matterIds[i];
                    var parentSequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                    Subsequence[] sequenceSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds);
                    var subsequences = subsequenceExtractor.ExtractChains(sequenceSubsequences, parentSequenceId);
                    subsequencesCount[i] = subsequences.Length;
                    allSubsequencesCharacteristics.Add(CalculateCharacteristic(characteristicTypeLinkId, subsequences, sequenceSubsequences)
                        .Where(c => (characteristicValueFrom == 0 && characteristicValueTo == 0) || c.Value >= characteristicValueFrom && c.Value <= characteristicValueTo)
                       .OrderBy(c => c.Value).ToArray());
                }

                double difference = double.Parse(maxDifference, CultureInfo.InvariantCulture);

                var similarities = new MvcHtmlString[mattersCount, mattersCount];
                // var firstSequenceSimilarities = new double[mattersCount, mattersCount];
                // var secondSequenceSimilarities = new double[mattersCount, mattersCount];

                var equalElements = new List<MvcHtmlString>();
                int comparisonNumber = 0;

                for (int i = 0; i < allSubsequencesCharacteristics.Count; i++)
                {
                    for (int j = 0; j < allSubsequencesCharacteristics.Count; j++)
                    {
                        comparisonNumber++;
                        int similarSubsequences = 0;

                        double similarSequencesCharacteristicValue = 0;
                        double similarFirstSequencesCharacteristicValue = 0;
                        double similarSecondSequencesCharacteristicValue = 0;

                        int secondArrayStartPosition = 0;

                        for (int k = 0; k < allSubsequencesCharacteristics[i].Length; k++)
                        {
                            for (int l = secondArrayStartPosition; l < allSubsequencesCharacteristics[j].Length; l++)
                            {
                                // if (excludeType == "Exclude")
                                // {
                                //     allSubsequencesCharacteristics[i][k] = double.NaN;
                                //     allSubsequencesCharacteristics[j][l] = double.NaN;
                                // }

                                if (Math.Abs(allSubsequencesCharacteristics[i][k].Value - allSubsequencesCharacteristics[j][l].Value) <= difference)
                                {
                                    if (i != j)
                                    {
                                        equalElements.Add(
                                            new MvcHtmlString(
                                            string.Format("{0} with {1} {2} <b>{0}</b> {3}; <b>Characteristic = {4}</b> {2} <b>{1}</b> {2} {5}; <b>Characteristic = {6}</b>",
                                            matterNames[i], matterNames[j], "<br/>",
                                            allSubsequencesCharacteristics[i][k].Key, allSubsequencesCharacteristics[i][k].Value,
                                            allSubsequencesCharacteristics[j][l].Key, allSubsequencesCharacteristics[j][l].Value)));
                                    }

                                    similarSubsequences++;
                                    similarSequencesCharacteristicValue += allSubsequencesCharacteristics[i][k].Value + allSubsequencesCharacteristics[j][l].Value;
                                    similarFirstSequencesCharacteristicValue += allSubsequencesCharacteristics[i][k].Value;
                                    similarSecondSequencesCharacteristicValue += allSubsequencesCharacteristics[j][l].Value;

                                    secondArrayStartPosition++;
                                    break;
                                }

                                if (allSubsequencesCharacteristics[j][l].Value < allSubsequencesCharacteristics[i][k].Value)
                                {
                                    secondArrayStartPosition++;
                                    break;
                                }
                            }
                        }

                        similarities[i, j] = new MvcHtmlString(similarSubsequences * 200d / (subsequencesCount[i] + subsequencesCount[j]) + "%" + " <br/> "
                            + Math.Abs(similarFirstSequencesCharacteristicValue - similarSecondSequencesCharacteristicValue) / similarSequencesCharacteristicValue +  " <br/> " +
                        + similarSequencesCharacteristicValue / (allSubsequencesCharacteristics[i].Sum(c => c.Value) + allSubsequencesCharacteristics[j].Sum(c => c.Value)));

                        // firstSequenceSimilarities[i, j] = similarSubsequences * 100d / subsequencesCount[i];

                        // secondSequenceSimilarities[i, j] = similarSubsequences * 100d / subsequencesCount[j];
                    }
                }

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notationId);

                return new Dictionary<string, object>
                {
                    { "mattersNames", matterNames },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    // { "firstSequenceSimilarities", firstSequenceSimilarities },
                    // { "secondSequenceSimilarities", secondSequenceSimilarities },
                    {"equalElements", equalElements }
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
        private KeyValuePair<string, double>[] CalculateCharacteristic(int characteristicTypeLinkId, Chain[] sequences, Subsequence[] subsequences)
        {
            var values = new KeyValuePair<string, double>[sequences.Length];
            var newCharacteristics = new List<Characteristic>();

            var subsequenceIds = subsequences.Select(s => s.Id).ToList();

            Dictionary<long, double> dbCharacteristics = db.Characteristic
                                              .Where(c => characteristicTypeLinkId == c.CharacteristicTypeLinkId && subsequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.Single().Value);

            for (int j = 0; j < sequences.Length; j++)
            {
                double currentValue;
                if (!dbCharacteristics.TryGetValue(subsequences[j].Id, out currentValue)
                    // && newCharacteristics.All(c => c.SequenceId != subsequences[j].SequenceId)
                    )
                {
                    string className =
                        characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                    IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                    var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                    values[j] = new KeyValuePair<string, double>(subsequences[j].DnaSequence.ToString(), calculator.Calculate(sequences[j], link));
                    var currentCharacteristic = new Characteristic
                    {
                        SequenceId = subsequences[j].Id,
                        CharacteristicTypeLinkId = characteristicTypeLinkId,
                        Value = values[j].Value
                    };

                    newCharacteristics.Add(currentCharacteristic);
                }
                else
                {
                    values[j] = new KeyValuePair<string, double>("RemoteId = " + subsequences[j].RemoteId + "; " + "Attribute = " + string.Join(", ", subsequences[j].SequenceAttribute.Select(a => a.Attribute.Name)), currentValue);
                }
            }

            db.Characteristic.AddRange(newCharacteristics);
            db.SaveChanges();

            return values;
        }
    }
}
