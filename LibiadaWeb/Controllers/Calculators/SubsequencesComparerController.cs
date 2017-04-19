namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences comparer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesComparerController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesComparerController"/> class.
        /// </summary>
        public SubsequencesComparerController() : base("Subsequences comparer")
        {
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
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="subsequencesCharacteristicLinkId">
        /// The subsequences characteristic type link id.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <param name="maxPercentageDifference">
        /// The precision.
        /// </param>
        /// <param name="characteristicValueFrom">
        /// Minimum value for calculating characteristic
        /// </param>
        /// <param name="characteristicValueTo">
        /// Maximum value for calculating characteristic
        /// </param>
        /// <param name="filters">
        /// Filters for the subsequences.
        /// Filters are applied in "OR" logic (if subsequence corresponds to any filter it is added to calculation).
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
            short characteristicLinkId,
            short subsequencesCharacteristicLinkId,
            Feature[] features,
            string maxPercentageDifference,
            double characteristicValueFrom,
            double characteristicValueTo,
            string[] filters)
        {
            return Action(() =>
            {
                var attributeValues = new List<AttributeValue>();
                var characteristics = new SubsequenceData[matterIds.Length][];
                string characteristicName;

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];

                string sequenceCharacteristicName;

                int mattersCount = matterIds.Length;
                var subsequencesCount = new int[mattersCount];
                List<CharacteristicData> localCharacteristicsType;

                using (var db = new LibiadaWebEntities())
                {
                    // Sequences characteristic
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    Chain[] chains = commonSequenceRepository.GetNucleotideChains(matterIds);

                    var sequencesCharacteristicTypeLinkRepository = new FullCharacteristicRepository(db);
                    sequenceCharacteristicName = sequencesCharacteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkId);

                    // Sequences characterstic
                    double[] completeGenomesCharacteristics = SequencesCharacteristicsCalculator.Calculate(chains, characteristicLinkId);

                    var matterCharacteristics = new KeyValuePair<long, double>[matterIds.Length];

                    for (int i = 0; i < completeGenomesCharacteristics.Length; i++)
                    {
                        matterCharacteristics[i] = new KeyValuePair<long, double>(matterIds[i], completeGenomesCharacteristics[i]);
                    }

                    matterIds = matterCharacteristics.OrderBy(mc => mc.Value).Select(mc => mc.Key).ToArray();

                    // Subsequences characteristics
                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                            .Select(s => new { s.Id, s.MatterId, MatterName = s.Matter.Name })
                                            .ToDictionary(s => s.Id);
                    parentSequenceIds = parentSequences.OrderBy(ps => Array.IndexOf(matterIds, ps.Value.MatterId)).Select(ps => ps.Key).ToArray();

                    for (int n = 0; n < parentSequenceIds.Length; n++)
                    {
                        matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    }

                    var subsequencesCharacteristicTypeLinkRepository = new FullCharacteristicRepository(db);

                    characteristicName = subsequencesCharacteristicTypeLinkRepository.GetFullCharacteristicName(subsequencesCharacteristicLinkId);

                    var viewDataHelper = new ViewDataHelper(db);

                    localCharacteristicsType = viewDataHelper.GetFullCharacteristicTypes();
                }

                // cycle through matters; first level of characteristics array
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            new[] { subsequencesCharacteristicLinkId },
                            features,
                            parentSequenceIds[i],
                            attributeValues,
                            filters);

                    subsequencesCount[i] = subsequencesData.Length;

                    subsequencesData = subsequencesData.Where(c => (characteristicValueFrom == 0 && characteristicValueTo == 0)
                        || (c.CharacteristicsValues[0] >= characteristicValueFrom && c.CharacteristicsValues[0] <= characteristicValueTo))
                        .OrderBy(c => c.CharacteristicsValues[0]).ToArray();

                    characteristics[i] = subsequencesData;
                }

                double decimalDifference = double.Parse(maxPercentageDifference, CultureInfo.InvariantCulture) / 100;

                var similarities = new object[mattersCount, mattersCount];

                var equalElements = new List<SubsequenceComparisonData>[mattersCount, mattersCount];

                for (int i = 0; i < characteristics.Length; i++)
                {
                    for (int j = 0; j < characteristics.Length; j++)
                    {
                        int firstAbsolutelyEqualElementsCount = 0;
                        int firstExeptableEqualElementsCount = 0;

                        int secondAbsolutelyEqualElementsCount = 0;
                        int secondExeptableEqualElementsCount = 0;

                        equalElements[i, j] = new List<SubsequenceComparisonData>();
                        double similarSequencesCharacteristicValueFirst = 0;
                        var similarSequencesCharacteristicValueSecond = new Dictionary<int, double>();

                        int secondArrayStartPosition = 0;
                        double differenceSum = 0;

                        int equalElementsCountFromFirst = 0;
                        var equalElementsCountFromSecond = new Dictionary<int, bool>();

                        int equalPairsCount = 0;
                        double difference = 0;

                        for (int k = 0; k < characteristics[i].Length; k++)
                        {
                            bool? equalFoundFromFirstAbsolutely = null;
                            bool? equalFoundFromSecondAbsolutely = null;

                            double first = characteristics[i][k].CharacteristicsValues[0];

                            for (int l = secondArrayStartPosition; l < characteristics[j].Length; l++)
                            {
                                double second = characteristics[j][l].CharacteristicsValues[0];

                                difference = CalculateAverageDifference(first, second);

                                if (difference <= decimalDifference)
                                {
                                    if (!equalFoundFromFirstAbsolutely.HasValue || !equalFoundFromFirstAbsolutely.Value)
                                    {
                                        equalFoundFromFirstAbsolutely = difference == 0;
                                    }

                                    equalPairsCount++;

                                    if (!equalElementsCountFromSecond.ContainsKey(l))
                                    {
                                        equalElementsCountFromSecond.Add(l, difference == 0);
                                        differenceSum += difference;
                                    }
                                    else
                                    {
                                        if (!equalElementsCountFromSecond[l])
                                        {
                                            equalElementsCountFromSecond[l] = difference == 0;
                                        }
                                    }

                                    if (!similarSequencesCharacteristicValueSecond.ContainsKey(l))
                                    {
                                        similarSequencesCharacteristicValueSecond.Add(l, second);
                                    }

                                    if (i != j)
                                    {
                                        equalElements[i, j].Add(new SubsequenceComparisonData
                                        {
                                            Difference = difference,
                                            FirstSubsequenceIndex = k,
                                            SecondSubsequenceIndex = l
                                        });
                                    }

                                    if (l < characteristics[j].Length - 1)
                                    {
                                        bool nextElementInSecondArrayIsEqual = CalculateAverageDifference(second, characteristics[j][l + 1].CharacteristicsValues[0]) <= decimalDifference;

                                        if (!nextElementInSecondArrayIsEqual)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (second < first)
                                {
                                    secondArrayStartPosition++;
                                }
                            }

                            if (equalFoundFromFirstAbsolutely.HasValue)
                            {
                                equalElementsCountFromFirst++;
                                similarSequencesCharacteristicValueFirst += first;

                                // fill equal elements count for first chain
                                if (equalFoundFromFirstAbsolutely.Value)
                                {
                                    firstAbsolutelyEqualElementsCount++;
                                }
                                else
                                {
                                    firstExeptableEqualElementsCount++;
                                }
                            }
                        }

                        secondAbsolutelyEqualElementsCount = equalElementsCountFromSecond.Count(e => e.Value);
                        secondExeptableEqualElementsCount = equalElementsCountFromSecond.Count(e => !e.Value);

                        double differenceSecondFinal = equalElementsCountFromSecond.Count;
                        double differenceFinal = equalElementsCountFromFirst < differenceSecondFinal ? equalElementsCountFromFirst * 2d : differenceSecondFinal * 2d;

                        double formula1 = differenceFinal / (subsequencesCount[i] + subsequencesCount[j]);

                        double formula2 = 0;
                        if (equalPairsCount != 0 && formula1 != 0)
                        {
                            formula2 = (differenceSum / equalPairsCount) / formula1;
                        }

                        double similarSequencesCharacteristicValueSecondFinal = similarSequencesCharacteristicValueSecond.Sum(s => s.Value);
                        double similarSequencesCharacteristicValue = similarSequencesCharacteristicValueFirst < similarSequencesCharacteristicValueSecondFinal ?
                                            similarSequencesCharacteristicValueFirst * 2d : similarSequencesCharacteristicValueSecondFinal * 2d;

                        double formula3 = similarSequencesCharacteristicValue / (characteristics[i].Sum(c => c.CharacteristicsValues[0]) + characteristics[j].Sum(c => c.CharacteristicsValues[0]));

                        const int digits = 5;

                        similarities[i, j] = new
                        {
                            formula1 = Math.Round(formula1, digits),
                            formula2 = Math.Round(formula2, digits),
                            formula3 = Math.Round(formula3, digits),

                            firstAbsolutelyEqualElementsCount,
                            firstExeptableEqualElementsCount,
                            firstNotEqualElementsCount = characteristics[i].Length - (firstAbsolutelyEqualElementsCount + firstExeptableEqualElementsCount),

                            secondAbsolutelyEqualElementsCount,
                            secondExeptableEqualElementsCount,
                            secondNotEqualElementsCount = characteristics[j].Length - (secondAbsolutelyEqualElementsCount + secondExeptableEqualElementsCount),
                        };

                        equalElements[i, j] = equalElements[i, j].OrderBy(e => e.Difference).ToList();
                    }
                }

                var result = new Dictionary<string, object>
                {
                    { "mattersNames", matterNames },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    { "characteristics", characteristics },
                    { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                    { "attributeValues", attributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                    { "attributes", ArrayExtensions.ToArray<LibiadaWeb.Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                    { "maxPercentageDifference", maxPercentageDifference },
                    { "sequenceCharacteristicName", sequenceCharacteristicName },
                    { "characteristicTypes", localCharacteristicsType }
                };

                return new Dictionary<string, object>
                           {
                               { "additionalData", equalElements },
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }

        /// <summary>
        /// Calculates average difference.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double CalculateAverageDifference(double first, double second)
        {
            return Math.Abs((first - second) / ((first + second) / 2));
        }
    }
}
