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
            string excludeType)
        {
            return Action(() =>
            {
                var allSubsequencesCharacteristics = new List<double[]>();
                int mattersCount = matterIds.Length;
                int[] subsequencesCount = new int[mattersCount];

                for (int i = 0; i < mattersCount; i++)
                {
                    long matterId = matterIds[i];
                    var parentSequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                    Subsequence[] sequenceSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds);
                    var subsequences = subsequenceExtractor.ExtractChains(sequenceSubsequences, parentSequenceId);
                    subsequencesCount[i] = subsequences.Length;
                    allSubsequencesCharacteristics.Add(CalculateCharacteristic(characteristicTypeLinkId, subsequences, sequenceSubsequences).OrderBy(c => c).ToArray());
                }

                double difference = double.Parse(maxDifference, CultureInfo.InvariantCulture);

                var similarities = new double[mattersCount, mattersCount];
                var firstSequenceSimilarities = new double[mattersCount, mattersCount];
                var secondSequenceSimilarities = new double[mattersCount, mattersCount];

                for (int i = 0; i < allSubsequencesCharacteristics.Count; i++)
                {
                    for (int j = 0; j < allSubsequencesCharacteristics.Count; j++)
                    {
                        var similarSubsequences = new List<IntPair>();

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

                                if (Math.Abs(allSubsequencesCharacteristics[i][k] - allSubsequencesCharacteristics[j][l]) <= difference)
                                {
                                    similarSubsequences.Add(new IntPair(k, l));
                                    secondArrayStartPosition++;
                                    break;
                                }

                                if (allSubsequencesCharacteristics[j][l] < allSubsequencesCharacteristics[i][k])
                                {
                                    secondArrayStartPosition++;
                                    break;
                                }
                            }
                        }

                        similarities[i, j] = similarSubsequences.Count * 200d / (subsequencesCount[i] + subsequencesCount[j]);

                        firstSequenceSimilarities[i, j] = similarSubsequences.Count * 100d / subsequencesCount[i];

                        secondSequenceSimilarities[i, j] = similarSubsequences.Count * 100d / subsequencesCount[j];
                    }
                }

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notationId);

                return new Dictionary<string, object>
                {
                    { "mattersNames", db.Matter.Where(m => matterIds.Contains(m.Id)).Select(m => m.Name).ToArray() },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    { "firstSequenceSimilarities", firstSequenceSimilarities },
                    { "secondSequenceSimilarities", secondSequenceSimilarities }
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
        private double[] CalculateCharacteristic(int characteristicTypeLinkId, Chain[] sequences, Subsequence[] subsequences)
        {
            double[] values = new double[sequences.Length];
            var newCharacteristics = new List<Characteristic>();

            var subsequenceIds = subsequences.Select(s => s.Id).ToList();

            Dictionary<long, double> dbCharacteristics = db.Characteristic
                                              .Where(c => characteristicTypeLinkId == c.CharacteristicTypeLinkId && subsequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.Single().Value);

            for (int j = 0; j < sequences.Length; j++)
            {
                if (!dbCharacteristics.TryGetValue(subsequences[j].Id, out values[j])
                    // && newCharacteristics.All(c => c.SequenceId != subsequences[j].SequenceId)
                   )
                {
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                    IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                    var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                    values[j] = calculator.Calculate(sequences[j], link);
                    var currentCharacteristic = new Characteristic
                    {
                        SequenceId = subsequences[j].Id,
                        CharacteristicTypeLinkId = characteristicTypeLinkId,
                        Value = values[j]
                    };

                    newCharacteristics.Add(currentCharacteristic);
                }
            }

            db.Characteristic.AddRange(newCharacteristics);
            db.SaveChanges();

            return values;
        }
    }
}
