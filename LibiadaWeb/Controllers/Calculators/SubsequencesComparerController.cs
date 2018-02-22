namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Models.Repositories.Sequences;

    using Newtonsoft.Json;

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
            string[] filters)
        {
            return CreateTask(() =>
            {
                double percentageDifference = double.Parse(maxPercentageDifference, CultureInfo.InvariantCulture) / 100;

                var attributeValues = new List<AttributeValue>();
                var characteristics = new SubsequenceData[matterIds.Length][];
                string characteristicName;

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];

                string sequenceCharacteristicName;

                int mattersCount = matterIds.Length;
                List<CharacteristicTypeData> localCharacteristicsType;

                using (var db = new LibiadaWebEntities())
                {
                    // Sequences characteristic
                    var geneticSequenceRepository = new GeneticSequenceRepository(db);
                    long[] chains = geneticSequenceRepository.GetNucleotideSequenceIds(matterIds);

                    var sequencesCharacteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                    sequenceCharacteristicName = sequencesCharacteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId);

                    // Sequences characteristic
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

                    var subsequencesCharacteristicTypeLinkRepository = FullCharacteristicRepository.Instance;

                    characteristicName = subsequencesCharacteristicTypeLinkRepository.GetCharacteristicName(subsequencesCharacteristicLinkId);

                    var fullCharacteristicRepository = FullCharacteristicRepository.Instance;
                    localCharacteristicsType = fullCharacteristicRepository.GetCharacteristicTypes();
                }

                var characteristicValueSubsequences = new Dictionary<double, List<(int, int)>>();

                // cycle through matters
                for (int i = 0; i < mattersCount; i++)
                {
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            new[] { subsequencesCharacteristicLinkId },
                            features,
                            parentSequenceIds[i],
                            attributeValues,
                            filters);

                    characteristics[i] = subsequencesData;

                    for (int j = 0; j < subsequencesData.Length; j++)
                    {
                        SubsequenceData value = subsequencesData[j];
                        if (characteristicValueSubsequences.TryGetValue(value.CharacteristicsValues[0], out List<(int, int)> subsequencesList))
                        {
                            subsequencesList.Add((i, j));
                        }
                        else
                        {
                            characteristicValueSubsequences.Add(value.CharacteristicsValues[0], new List<(int, int)> { (i, j) });
                        }
                    }
                }

                var similarPairs = new List<((int, int),(int, int), double)>();
                foreach (double key in characteristicValueSubsequences.Keys)
                {
                    similarPairs.AddRange(ExtractAllPossiblePairs(characteristicValueSubsequences[key]));
                }

                double[] orderedCharacteristicValue = characteristicValueSubsequences.Keys.OrderBy(v => v).ToArray();
                for (int i = 0; i < orderedCharacteristicValue.Length - 1; i++)
                {
                    int j = i + 1;
                    double difference = CalculateAverageDifference(orderedCharacteristicValue[i], orderedCharacteristicValue[j]);
                    while (difference < percentageDifference)
                    {
                        List<(int, int)> firstComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[i]];
                        List<(int, int)> secondComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[j]];
                        similarPairs.AddRange(ExtractAllPossiblePairs(firstComponentIndex, secondComponentIndex, difference));

                        j++;
                        if (j == orderedCharacteristicValue.Length) break;
                        difference = CalculateAverageDifference(orderedCharacteristicValue[i], orderedCharacteristicValue[j]);
                    }
                }

                List<(int, int, double)>[,] similarityMatrix = FillSimilarityMatrix(mattersCount, similarPairs);

                var similarities = new object[mattersCount, mattersCount];
                for (int i = 0; i < mattersCount; i++)
                {
                    for (int j = 0; j < mattersCount; j++)
                    {
                        int firstEqualCount = similarityMatrix[i, j].Select(s => s.Item1).Distinct().Count();
                        int secondEqualCount = similarityMatrix[i, j].Select(s => s.Item2).Distinct().Count();

                        double equalSequencesCount = firstEqualCount < secondEqualCount ?
                                                         firstEqualCount * 2d :
                                                         secondEqualCount * 2d;
                        double formula1 = equalSequencesCount / (characteristics[i].Length + characteristics[j].Length);

                        double formula2 = 0;
                        //if (equalElements[i, j].Count != 0 && formula1 != 0)
                        //{
                        //    formula2 = (differenceSum / equalElements[i, j].Count) / formula1;
                        //}

                        double firstCharacteristicSum = similarityMatrix[i, j]
                                                            .Select(s => s.Item1)
                                                            .Distinct()
                                                            .Sum(s => characteristics[i][s].CharacteristicsValues[0]);

                        double secondCharacteristicSum = similarityMatrix[i, j]
                                                            .Select(s => s.Item2)
                                                            .Distinct()
                                                            .Sum(s => characteristics[j][s].CharacteristicsValues[0]);

                        double similarSequencesCharacteristicSum = firstCharacteristicSum < secondCharacteristicSum ?
                                                                       firstCharacteristicSum * 2d :
                                                                       secondCharacteristicSum * 2d;

                        double fistSequenceCharacteristicSum = characteristics[i].Sum(c => c.CharacteristicsValues[0]);
                        double secondSequenceCharacteristicSum = characteristics[j].Sum(c => c.CharacteristicsValues[0]);
                        double formula3 = similarSequencesCharacteristicSum / (fistSequenceCharacteristicSum + secondSequenceCharacteristicSum);

                        const int digits = 5;
                        similarities[i, j] = new
                        {
                            formula1 = Math.Round(formula1, digits),
                            formula2 = Math.Round(formula2, digits),
                            formula3 = Math.Round(formula3, digits),
                            firstAbsolutelyEqualElementsCount = firstEqualCount,
                            firstNearlyEqualElementsCount = 0,
                            firstNotEqualElementsCount = characteristics[i].Length - firstEqualCount,

                            secondAbsolutelyEqualElementsCount = secondEqualCount,
                            secondNearlyEqualElementsCount = 0,
                            secondNotEqualElementsCount = characteristics[j].Length - secondEqualCount,
                        };
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
                    { "attributes", EnumExtensions.ToArray<LibiadaWeb.Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                    { "maxPercentageDifference", maxPercentageDifference },
                    { "sequenceCharacteristicName", sequenceCharacteristicName },
                    { "characteristicTypes", localCharacteristicsType }
                };

                return new Dictionary<string, object>
                           {
                               { "additionalData", similarityMatrix },
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
        private List<((int, int), (int, int), double)> ExtractAllPossiblePairs(List<(int, int)> list)
        {
            var result = new List<((int, int),(int, int),double)> ();
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
        private List<((int, int), (int, int),double)> ExtractAllPossiblePairs(
            List<(int, int)> firstList,
            List<(int, int)> secondList,
            double difference)
        {
            var result = new List<((int, int), (int, int),double)>();

            foreach ((int, int) firstElement in firstList)
            {
                foreach ((int, int) secondElement in secondList)
                {
                    result.Add((firstElement, secondElement, difference));
                }
            }

            return result;
        }

        private List<(int, int, double)>[,] FillSimilarityMatrix(int mattersCount, List<((int,int),(int,int),double)> similarPairs)
        {
            var similarityMatrix = new List<(int, int, double)>[mattersCount, mattersCount];
            for (int i = 0; i < mattersCount; i++)
            {
                for (int j = 0; j < mattersCount; j++)
                {
                    similarityMatrix[i, j] = new List<(int, int, double)>();
                }
            }

            foreach (((int, int) firstIndex, (int, int) secondIndex, double difference) in similarPairs)
            {
                (int firstMatter, int firstSubsequence) = firstIndex;
                (int secondMatter, int secondSubsequence) = secondIndex;

                (int, int, double) similarityData = (firstSubsequence, secondSubsequence, difference);
                similarityMatrix[firstMatter, secondMatter].Add(similarityData);

                (int, int, double) symmetricalSimilarityData = (secondSubsequence, firstSubsequence, difference);
                similarityMatrix[secondMatter, firstMatter].Add(symmetricalSimilarityData);
            }

            return similarityMatrix;
        }
    }
}
