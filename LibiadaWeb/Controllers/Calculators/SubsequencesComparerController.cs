﻿namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Models.Repositories.Sequences;

    using Newtonsoft.Json;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    using static LibiadaWeb.Models.Calculators.SubsequencesCharacteristicsCalculator;

    /// <summary>
    /// The subsequences comparer controller.
    /// </summary>
    [Authorize]
    public class SubsequencesComparerController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesComparerController"/> class.
        /// </summary>
        public SubsequencesComparerController() : base(TaskType.SubsequencesComparer)
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
        /// <param name="filters">
        /// Filters for the subsequences.
        /// Filters are applied in "OR" logic (if subsequence corresponds to any filter it is added to calculation).
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
            short subsequencesCharacteristicLinkId,
            Feature[] features,
            string maxPercentageDifference,
            string[] filters)
        {
            return CreateTask(() =>
            {
                double percentageDifference = double.Parse(maxPercentageDifference, CultureInfo.InvariantCulture) / 100;

                var attributeValuesCache = new AttributeValueCacheManager();
                var characteristics = new SubsequenceData[matterIds.Length][];

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];

                int mattersCount = matterIds.Length;
                Dictionary<string, object> characteristicsTypesData;

                using (var db = new LibiadaWebEntities())
                {
                    // Sequences characteristic
                    var geneticSequenceRepository = new GeneticSequenceRepository(db);
                    long[] chains = geneticSequenceRepository.GetNucleotideSequenceIds(matterIds);

                    // Sequences characteristic
                    matterIds = OrderMatterIds(matterIds, characteristicLinkId, chains);

                    // Subsequences characteristics
                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                            .Select(s => new { s.Id, s.MatterId, MatterName = s.Matter.Name })
                                            .ToDictionary(s => s.Id);

                    parentSequenceIds = parentSequences
                                        .OrderBy(ps => Array.IndexOf(matterIds, ps.Value.MatterId))
                                        .Select(ps => ps.Key)
                                        .ToArray();

                    for (int n = 0; n < parentSequenceIds.Length; n++)
                    {
                        matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    }

                    var viewDataHelper = new ViewDataHelper(db);
                    characteristicsTypesData = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);
                }

                FullCharacteristicRepository fullCharacteristicRepository = FullCharacteristicRepository.Instance;
                string sequenceCharacteristicName = fullCharacteristicRepository.GetCharacteristicName(characteristicLinkId);
                string characteristicName = fullCharacteristicRepository.GetCharacteristicName(subsequencesCharacteristicLinkId);

                var characteristicValueSubsequences = new Dictionary<double, List<(int matterIndex, int subsequenceIndex)>>();

                // cycle through matters
                for (int i = 0; i < mattersCount; i++)
                {
                    SubsequenceData[] subsequencesData = CalculateSubsequencesCharacteristics(
                            new[] { subsequencesCharacteristicLinkId },
                            features,
                            parentSequenceIds[i],
                            filters);

                    characteristics[i] = subsequencesData;
                    attributeValuesCache.FillAttributeValues(subsequencesData);

                    for (int j = 0; j < subsequencesData.Length; j++)
                    {
                        double value = subsequencesData[j].CharacteristicsValues[0];
                        if (characteristicValueSubsequences.TryGetValue(value, out List<(int matterIndex, int subsequenceIndex)> matterAndSubsequenceIdsList))
                        {
                            matterAndSubsequenceIdsList.Add((i, j));
                        }
                        else
                        {
                            matterAndSubsequenceIdsList = new List<(int matterIndex, int subsequenceIndex)> { (matterIndex: i, subsequenceIndex: j) };
                            characteristicValueSubsequences.Add(value, matterAndSubsequenceIdsList);
                        }
                    }
                }

                List<List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>> similarPairs =
                    ExtractSimilarPairs(characteristicValueSubsequences, percentageDifference);

                List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] similarityMatrix =
                    FillSimilarityMatrix(mattersCount, similarPairs);

                object[,] similarities = Similarities(similarityMatrix, characteristics, mattersCount);

                List<AttributeValue> allAttributeValues = attributeValuesCache.AllAttributeValues;

                var result = new Dictionary<string, object>
                {
                    { "mattersNames", matterNames },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    { "characteristics", characteristics },
                    { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                    { "attributeValues", allAttributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                    { "attributes", EnumExtensions.ToArray<LibiadaWeb.Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                    { "maxPercentageDifference", maxPercentageDifference },
                    { "sequenceCharacteristicName", sequenceCharacteristicName },
                    { "nature", (byte)Nature.Genetic },
                    { "notations", EnumExtensions.ToArray<Notation>().Where(n => n.GetNature() == Nature.Genetic).ToSelectListWithNature() }
                };

                foreach ((string key, object value) in characteristicsTypesData)
                {
                    result.Add(key, value);
                }

                return new Dictionary<string, object>
                           {
                               { "additionalData", similarityMatrix },
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }

        /// <summary>
        /// Orders matter ids by characteristic values.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristic link id.
        /// </param>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <returns>
        /// The <see cref="T:long[]"/>.
        /// </returns>
        private long[] OrderMatterIds(long[] matterIds, short characteristicLinkId, long[] chains)
        {
            double[] completeSequencesCharacteristics = SequencesCharacteristicsCalculator.Calculate(chains, characteristicLinkId);

            var matterCharacteristics = new (long matterId, double charcterisitcValue)[matterIds.Length];

            for (int i = 0; i < completeSequencesCharacteristics.Length; i++)
            {
                matterCharacteristics[i] = (matterIds[i], completeSequencesCharacteristics[i]);
            }

            return matterCharacteristics.OrderBy(mc => mc.charcterisitcValue).Select(mc => mc.matterId).ToArray();
        }

        /// <summary>
        /// Calculates similarities.
        /// </summary>
        /// <param name="similarityMatrix">
        /// The similarity matrix.
        /// </param>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        /// <param name="mattersCount">
        /// The matters count.
        /// </param>
        /// <returns>
        /// The <see cref="T:object[,]"/>.
        /// </returns>
        private object[,] Similarities(
            List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] similarityMatrix,
            SubsequenceData[][] characteristics,
            int mattersCount)
        {
            var similarities = new object[mattersCount, mattersCount];
            for (int i = 0; i < mattersCount; i++)
            {
                for (int j = 0; j < mattersCount; j++)
                {
                    int firstEqualCount = similarityMatrix[i, j]
                                          .Select(s => s.firstSubsequenceIndex)
                                          .Distinct()
                                           .Count();
                    int firstAbsolutelyEqualCount = similarityMatrix[i, j]
                                                    .Where(s => s.difference == 0)
                                                    .Select(s => s.firstSubsequenceIndex)
                                                    .Distinct()
                                                    .Count();
                    int firstNearlyEqualCount = similarityMatrix[i, j]
                                                .Where(s => s.difference > 0)
                                                .Select(s => s.firstSubsequenceIndex)
                                                .Distinct()
                                                .Count();

                    int secondEqualCount = similarityMatrix[i, j]
                                           .Select(s => s.secondSubsequenceIndex)
                                           .Distinct()
                                           .Count();
                    int secondAbsolutelyEqualCount = similarityMatrix[i, j]
                                                     .Where(s => s.difference == 0)
                                                     .Select(s => s.secondSubsequenceIndex)
                                                     .Distinct()
                                                     .Count();
                    int secondNearlyEqualCount = similarityMatrix[i, j]
                                                 .Where(s => s.difference > 0)
                                                 .Select(s => s.secondSubsequenceIndex)
                                                 .Distinct()
                                                 .Count();

                    double equalSequencesCount = Math.Min(firstEqualCount, secondEqualCount) * 2d;
                    double formula1 = equalSequencesCount / (characteristics[i].Length + characteristics[j].Length);

                    double formula2 = 0;
                    if (similarityMatrix[i, j].Count != 0 && formula1 > 0)
                    {
                        double differenceSum = similarityMatrix[i, j].Select(s => s.difference).Sum();
                        formula2 = differenceSum / (similarityMatrix[i, j].Count * formula1);
                    }

                    double firstCharacteristicSum = similarityMatrix[i, j]
                                                    .Select(s => s.firstSubsequenceIndex)
                                                    .Distinct()
                                                    .Sum(s => characteristics[i][s].CharacteristicsValues[0]);

                    double secondCharacteristicSum = similarityMatrix[i, j]
                                                     .Select(s => s.secondSubsequenceIndex)
                                                     .Distinct()
                                                     .Sum(s => characteristics[j][s].CharacteristicsValues[0]);

                    double similarSequencesCharacteristicSum = Math.Min(firstCharacteristicSum, secondCharacteristicSum) * 2d;

                    double fistSequenceCharacteristicSum = characteristics[i].Sum(c => c.CharacteristicsValues[0]);
                    double secondSequenceCharacteristicSum = characteristics[j].Sum(c => c.CharacteristicsValues[0]);
                    double allSequencesCharacteristicSum = fistSequenceCharacteristicSum + secondSequenceCharacteristicSum;
                    double formula3 = similarSequencesCharacteristicSum / allSequencesCharacteristicSum;

                    const int digits = 5;
                    similarities[i, j] = new
                    {
                        formula1 = Math.Round(formula1, digits),
                        formula2 = Math.Round(formula2, digits),
                        formula3 = Math.Round(formula3, digits),
                        firstAbsolutelyEqualElementsCount = firstAbsolutelyEqualCount,
                        firstNearlyEqualElementsCount = firstNearlyEqualCount,
                        firstNotEqualElementsCount = characteristics[i].Length - firstEqualCount,
                        secondAbsolutelyEqualElementsCount = secondAbsolutelyEqualCount,
                        secondNearlyEqualElementsCount = secondNearlyEqualCount,
                        secondNotEqualElementsCount = characteristics[j].Length - secondEqualCount,
                    };
                }
            }

            return similarities;
        }

        /// <summary>
        /// Extracts similar pairs.
        /// </summary>
        /// <param name="characteristicValueSubsequences">
        /// The characteristic value subsequences.
        /// </param>
        /// <param name="percentageDifference">
        /// The percentage difference.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{((int, int), (int, int), double)}"/>.
        /// </returns>
        private List<List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>> ExtractSimilarPairs(
            Dictionary<double, List<(int matterId, int subsequenceIndex)>> characteristicValueSubsequences,
            double percentageDifference)
        {
            var similarPairs = new List<List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>>(characteristicValueSubsequences.Count);
            foreach (double key in characteristicValueSubsequences.Keys)
            {
                similarPairs.Add(ExtractAllPossiblePairs(characteristicValueSubsequences[key]));
            }

            double[] orderedCharacteristicValue = characteristicValueSubsequences.Keys.OrderBy(v => v).ToArray();
            for (int i = 0; i < orderedCharacteristicValue.Length - 1; i++)
            {
                int j = i + 1;
                double difference = CalculateAverageDifference(orderedCharacteristicValue[i], orderedCharacteristicValue[j]);
                while (difference <= percentageDifference)
                {
                    List<(int matterIndex, int subsequenceIndex)> firstComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[i]];
                    List<(int matterIndex, int subsequenceIndex)> secondComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[j]];
                    similarPairs.Add(ExtractAllPossiblePairs(firstComponentIndex, secondComponentIndex, difference));

                    j++;
                    if (j == orderedCharacteristicValue.Length) break;
                    difference = CalculateAverageDifference(orderedCharacteristicValue[i], orderedCharacteristicValue[j]);
                }
            }

            return similarPairs;
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

        /// <summary>
        /// Extract all possible unique pairs from given list.
        /// (calculates Cartesian product)
        /// </summary>
        /// <param name="list">
        /// The list for pairs extraction.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{((int,int), (int,int),double)}"/>.
        /// </returns>
        private List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> ExtractAllPossiblePairs(
            List<(int matterIndex, int subsequenceIndex)> list)
        {
            var result = new List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>();
            if (list.Count < 2)
            {
                return result;
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    result.Add((list[i], list[j], 0));
                }
            }

            return result;
        }

        /// <summary>
        /// Extract all possible pairs from given lists.
        /// (calculates Cartesian product)
        /// </summary>
        /// <param name="firstList">
        /// First list for Cartesian product.
        /// </param>
        /// <param name="secondList">
        /// Second list for Cartesian product.
        /// </param>
        /// <param name="difference">
        /// Distance between sets.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{((int,int), (int,int),double)}"/>.
        /// </returns>
        private List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> ExtractAllPossiblePairs(
            List<(int matterIndex, int subsequenceIndex)> firstList,
            List<(int matterIndex, int subsequenceIndex)> secondList,
            double difference)
        {
            var result = new List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>();

            foreach ((int, int) firstElement in firstList)
            {
                foreach ((int, int) secondElement in secondList)
                {
                    result.Add((firstElement, secondElement, difference));
                }
            }

            return result;
        }

        /// <summary>
        /// Fills similarity matrix from similarity pairs list.
        /// </summary>
        /// <param name="mattersCount">
        /// The matters count.
        /// </param>
        /// <param name="similarPairs">
        /// The similar pairs.
        /// </param>
        /// <returns>
        /// The <see cref="T:List{(int, int, double)}[,]"/>.
        /// </returns>
        private List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] FillSimilarityMatrix(
            int mattersCount,
            List<List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>> similarPairs)
        {
            var similarityMatrix = new List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[mattersCount, mattersCount];
            for (int i = 0; i < mattersCount; i++)
            {
                for (int j = 0; j < mattersCount; j++)
                {
                    similarityMatrix[i, j] = new List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>();
                }
            }
            foreach (var similarPairsList in similarPairs)
            {
                foreach (((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference) in similarPairsList)
                {
                    (int firstMatter, int firstSubsequence) = firstSequence;
                    (int secondMatter, int secondSubsequence) = secondSequence;

                    (int firstSubsequenceIndex, int secondSubsequenceIndex, double difference) similarityData = (firstSubsequence, secondSubsequence, difference);
                    similarityMatrix[firstMatter, secondMatter].Add(similarityData);

                    //  TODO: get rid of duplicate data
                    (int firstSubsequenceIndex, int secondSubsequenceIndex, double difference) symmetricalSimilarityData = (secondSubsequence, firstSubsequence, difference);
                    similarityMatrix[secondMatter, firstMatter].Add(symmetricalSimilarityData);
                }
            }
            return similarityMatrix;
        }
    }
}
