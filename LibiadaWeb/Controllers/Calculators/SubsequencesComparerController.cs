namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using System.Data.Entity;

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
    /// The subsequences comparer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesComparerController : AbstractResultController
    {
        /// <summary>
        /// The subsequence extractor.
        /// </summary>
        //private readonly SubsequenceExtractor subsequenceExtractor;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        //private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesComparerController"/> class.
        /// </summary>
        public SubsequencesComparerController() : base("Subsequences comparer")
        {
            //subsequenceExtractor = new SubsequenceExtractor(db);
            //characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(2, int.MaxValue, "Compare"));
            }

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
            int[] featureIds,
            string maxPercentageDifference,
            double characteristicValueFrom,
            double characteristicValueTo)
        {
            return Action(() =>
            {
                Dictionary<int, string> features;
                var attributeValues = new List<AttributeValue>();
                var characteristics = new SubsequenceData[matterIds.Length][];
                string characteristicName;

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];
                IFullCalculator calculator;
                Link link;

                int mattersCount = matterIds.Length;
                int[] subsequencesCount = new int[mattersCount];

                using (var db = new LibiadaWebEntities())
                {
                    var featureRepository = new FeatureRepository(db);
                    features = featureRepository.Features.ToDictionary(f => f.Id, f => f.Name);

                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                            .Select(s => new { s.Id, MatterName = s.Matter.Name })
                                            .ToDictionary(s => s.Id);
                    parentSequenceIds = parentSequences.Keys.ToArray();

                    for (int n = 0; n < parentSequenceIds.Length; n++)
                    {
                        matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    }

                    var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);

                    characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId);
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                    calculator = CalculatorsFactory.CreateFullCalculator(className);
                    link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                }

                // cycle through matters; first level of characteristics array
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            new[] { characteristicTypeLinkId },
                            featureIds,
                            parentSequenceIds[i],
                            new[] { calculator },
                            new[] { link },
                            attributeValues);

                    subsequencesCount[i] = subsequencesData.Length;

                    subsequencesData = subsequencesData.Where(c => (characteristicValueFrom == 0 && characteristicValueTo == 0)
                        || (c.CharacteristicsValues[0] >= characteristicValueFrom && c.CharacteristicsValues[0] <= characteristicValueTo)).
                        OrderBy(c => c.CharacteristicsValues[0]).ToArray();

                    characteristics[i] = subsequencesData;
                }

                double percentageDifference = double.Parse(maxPercentageDifference, CultureInfo.InvariantCulture);

                var similarities = new object[mattersCount, mattersCount];

                var equalElements = new List<SubsequenceComparisonData>();
                int comparisonNumber = 0;

                for (int i = 0; i < characteristics.Length; i++)
                {
                    for (int j = 0; j < characteristics.Length; j++)
                    {
                        comparisonNumber++;
                        int similarSubsequencesCount = 0;

                        double similarSequencesCharacteristicValue = 0;
                        double similarFirstSequencesCharacteristicValue = 0;
                        double similarSecondSequencesCharacteristicValue = 0;

                        int secondArrayStartPosition = 0;
                        double differenceSum = 0;

                        for (int k = 0; k < characteristics[i].Length; k++)
                        {
                            for (int l = secondArrayStartPosition; l < characteristics[j].Length; l++)
                            {
                                // if (excludeType == "Exclude")
                                // {
                                //     allSubsequencesCharacteristics[i][k] = double.NaN;
                                //     allSubsequencesCharacteristics[j][l] = double.NaN;
                                // }

                                double first = characteristics[i][k].CharacteristicsValues[0];
                                double second = characteristics[j][l].CharacteristicsValues[0];

                                double difference = Math.Abs(first - second) / ((first + second) / 2);

                                if (difference <= percentageDifference)
                                {
                                    if (i != j)
                                    {
                                        equalElements.Add(new SubsequenceComparisonData
                                        {
                                            Difference = difference,
                                            FirstMatterId = i,
                                            SecondMatterId = j,
                                            FirstSubsequenceId = k,
                                            SecondSubsequenceId = l,
                                        });
                                    }

                                    differenceSum += difference;
                                    similarSubsequencesCount++;
                                    similarSequencesCharacteristicValue += first + second;
                                    similarFirstSequencesCharacteristicValue += first;
                                    similarSecondSequencesCharacteristicValue += second;

                                    secondArrayStartPosition++;
                                    break;
                                }

                                if (second < first)
                                {
                                    secondArrayStartPosition++;
                                    break;
                                }
                            }
                        }

                        double formula1 = similarSubsequencesCount * 2d / (subsequencesCount[i] + subsequencesCount[j]);
                        double formula2 = (differenceSum / similarSubsequencesCount) / formula1;
                        double formula3 = similarSequencesCharacteristicValue * 100d / (characteristics[i].Sum(c => c.CharacteristicsValues[0]) + characteristics[j].Sum(c => c.CharacteristicsValues[0]));

                        similarities[i, j] = new
                        {
                            formula1 = Math.Round(formula1 * 100d, 3),
                            formula2 = Math.Round(formula2, 3),
                            formula3 = Math.Round(formula3, 3)
                        };
                    }
                }

                var result = new Dictionary<string, object>
                {
                    { "mattersNames", matterNames },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    { "characteristics", characteristics },
                    { "equalElements", equalElements.OrderBy(e => e.Difference).ToList() },
                    { "features", features },
                    { "attributeValues", attributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                    { "attributes", EnumExtensions.ToArray<LibiadaWeb.Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) }
                };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
