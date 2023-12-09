namespace Libiada.Web.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using LibiadaCore.Extensions;

    using Libiada.Web.Extensions;
    using Libiada.Web.Helpers;
    using Libiada.Database.Models.Calculators;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Libiada.Database.Tasks;
    using Libiada.Database.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    using static LibiadaCore.Extensions.EnumExtensions;
    using LibiadaCore.TimeSeries.OneDimensional.DistanceCalculators;
    using LibiadaCore.TimeSeries.OneDimensional.Comparers;
    using LibiadaCore.TimeSeries.Aligners;
    using LibiadaCore.TimeSeries.Aggregators;
    using Microsoft.AspNetCore.Authorization;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The subsequences comparer controller.
    /// </summary>
    [Authorize]
    public class SubsequencesComparerController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;
        private readonly IFullCharacteristicRepository fullCharacteristicRepository;
        private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
        private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;
        private readonly ICommonSequenceRepository commonSequenceRepository;
        private readonly GeneticSequenceRepository geneticSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesComparerController"/> class.
        /// </summary>
        public SubsequencesComparerController(LibiadaDatabaseEntities db, 
                                              IViewDataHelper viewDataHelper, 
                                              ITaskManager taskManager,
                                              IFullCharacteristicRepository fullCharacteristicRepository,
                                              ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                              ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                              ICommonSequenceRepository commonSequenceRepository,
                                              Cache cache)
            : base(TaskType.SubsequencesComparer, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
            this.fullCharacteristicRepository = fullCharacteristicRepository;
            this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
            this.sequencesCharacteristicsCalculator = sequencesCharacteristicsCalculator;
            this.commonSequenceRepository = commonSequenceRepository;
            geneticSequenceRepository = new GeneticSequenceRepository(db, cache);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
                var viewData = viewDataHelper.FillSubsequencesViewData(2, int.MaxValue, "Compare");
                viewData.Add("percentageDifferenseNeeded", true);
                ViewBag.data = JsonConvert.SerializeObject(viewData);
                //ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(2, int.MaxValue, "Compare"));
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
            short[] characteristicLinkIds,
            Feature[] features,
            string[] maxPercentageDifferences, // массив
            string[] filters,
            bool filterMatrix
        )
        {
            return CreateTask(() =>
            {
                double[] percentageDifferences = maxPercentageDifferences.Select(item => double.Parse(item, CultureInfo.InvariantCulture) / 100).ToArray();
                //double percentageDifference = double.Parse(maxPercentageDifference, CultureInfo.InvariantCulture) / 100; // цикл

                var attributeValuesCache = new AttributeValueCacheManager(db);
                var characteristics = new SubsequenceData[matterIds.Length][];

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];

                int mattersCount = matterIds.Length;
                Dictionary<string, object> characteristicsTypesData;


                // Sequences characteristic
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

                characteristicsTypesData = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);

                string sequenceCharacteristicName = fullCharacteristicRepository.GetCharacteristicName(characteristicLinkId);
                string characteristicName = fullCharacteristicRepository.GetCharacteristicName(characteristicLinkIds[0]);

                var characteristicValueSubsequences = new Dictionary<double, List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)>>();

                // cycle through matters
                for (int i = 0; i < mattersCount; i++)
                {
                    SubsequenceData[] subsequencesData = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            characteristicLinkIds,
                            features,
                            parentSequenceIds[i],
                            filters);

                    characteristics[i] = subsequencesData;
                    attributeValuesCache.FillAttributeValues(subsequencesData);

                    for (int j = 0; j < subsequencesData.Length; j++)
                    {
                        double value = subsequencesData[j].CharacteristicsValues[0]; // Получаем значения характеристик?
                        if (characteristicValueSubsequences.TryGetValue(value, out List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)> matterAndSubsequenceIdsList))
                        {
                            matterAndSubsequenceIdsList.Add((i, j, subsequencesData[j].CharacteristicsValues.Skip(1).ToArray()));
                        }
                        else
                        {
                            matterAndSubsequenceIdsList = new List<(int matterIndex, int subsequenceIndex, double[] additionalCharacterisctics)> 
                            { 
                                (matterIndex: i, subsequenceIndex: j, additionalCharacterisctics: subsequencesData[j].CharacteristicsValues.Skip(1).ToArray()) 
                            }; // добавить в кортеж все характеристики кроме 0
                            characteristicValueSubsequences.Add(value, matterAndSubsequenceIdsList);
                        }
                    }
                }


                List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs =
                    ExtractSimilarPairs(characteristicValueSubsequences, percentageDifferences);

                List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] similarityMatrix =
                    FillSimilarityMatrix(mattersCount, similarPairs);

                object[,] similarities = Similarities(similarityMatrix, characteristics, mattersCount);

                List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> filteredSimilarPairs;
                List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] filteredSimilarityMatrix = null;
                object[,] filteredSimilarities = null;

                if (filterMatrix)
                {
                    filteredSimilarPairs = FilterSimilarityPairs(similarPairs, characteristics, characteristicLinkIds[0]);
                    filteredSimilarityMatrix = FillSimilarityMatrix(mattersCount, filteredSimilarPairs);
                    filteredSimilarities = Similarities(filteredSimilarityMatrix, characteristics, mattersCount);
                }

                List<AttributeValue> allAttributeValues = attributeValuesCache.AllAttributeValues;

                var result = new Dictionary<string, object>
                {
                    { "mattersNames", matterNames },
                    { "characteristicName", characteristicName },
                    { "similarities", similarities },
                    { "filteredSimilarities", filteredSimilarities },
                    { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                    { "attributes", ToArray<Libiada.Database.Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                    { "maxPercentageDifferences", maxPercentageDifferences },
                    { "sequenceCharacteristicName", sequenceCharacteristicName },
                    { "nature", (byte)Nature.Genetic },
                    { "notations", ToArray<Notation>().Where(n => n.GetNature() == Nature.Genetic).ToSelectListWithNature() }
                };

                foreach ((string key, object value) in characteristicsTypesData)
                {
                    result.Add(key, value);
                }

                return new Dictionary<string, string>
                {
                    { "similarityMatrix", JsonConvert.SerializeObject(similarityMatrix) },
                    { "filteredSimilarityMatrix", JsonConvert.SerializeObject(filteredSimilarityMatrix) },
                    { "characteristics", JsonConvert.SerializeObject(characteristics) },
                    { "attributeValues", JsonConvert.SerializeObject(allAttributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value })) },
                    { "data", JsonConvert.SerializeObject(result) }
                };
            });
        }

        [NonAction]
        private List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> FilterSimilarityPairs
            (
                List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs,
                SubsequenceData[][] characteristics,
                short subsequencesCharacteristicLinkId
            )
        {
            var localCharacteristicsCalculator = new LocalCharacteristicsCalculator(db, fullCharacteristicRepository, commonSequenceRepository);

            var cache = new Dictionary<(int matterIndex, int subsequenceIndex), double[]>();
            var result = new List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>();

            var alignersFactory = new AlignersFactory();
            var calculatorsFactory = new DistanceCalculatorsFactory();
            var aggregatorsFactory = new AggregatorsFactory();

            var aligner = alignersFactory.GetAligner(Aligner.AllOffsetsAligner);
            var distanceCalculator = calculatorsFactory.GetDistanceCalculator(DistanceCalculator.EuclideanDistanceBetweenOneDimensionalPointsCalculator);
            var aggregator = aggregatorsFactory.GetAggregator(Aggregator.Average);

            var timeSeriesComparer = new OneDimensionalTimeSeriesComparer(aligner, distanceCalculator, aggregator);

            for (int i = 0; i < similarPairs.Count; i++)
            {
                var similarPair = similarPairs[i];
                if (!cache.TryGetValue((similarPair.firstSequence.matterIndex, similarPair.firstSequence.subsequenceIndex), out double[] firstLocalCharacteristics))
                {
                    var subsequenceData = characteristics[similarPair.firstSequence.matterIndex][similarPair.firstSequence.subsequenceIndex];
                    firstLocalCharacteristics = localCharacteristicsCalculator.GetSubsequenceCharacteristic(subsequenceData.Id, subsequencesCharacteristicLinkId, 50, 1);
                    cache.Add((similarPair.firstSequence.matterIndex, similarPair.firstSequence.subsequenceIndex), firstLocalCharacteristics);
                    //TODO: get rid of hardcoded parameters
                }
                if (!cache.TryGetValue((similarPair.secondSequence.matterIndex, similarPair.secondSequence.subsequenceIndex), out double[] secondLocalCharacteristics))
                {
                    var subsequenceData = characteristics[similarPair.secondSequence.matterIndex][similarPair.secondSequence.subsequenceIndex];
                    secondLocalCharacteristics = localCharacteristicsCalculator.GetSubsequenceCharacteristic(subsequenceData.Id, subsequencesCharacteristicLinkId, 50, 1);
                    cache.Add((similarPair.secondSequence.matterIndex, similarPair.secondSequence.subsequenceIndex), secondLocalCharacteristics);
                    //TODO: get rid of hardcoded parameters
                }

                var distance = timeSeriesComparer.GetDistance(firstLocalCharacteristics, secondLocalCharacteristics);
                if (distance <= 1.5)
                {
                    result.Add((similarPair.firstSequence, similarPair.secondSequence, distance));
                }
            }

            return result;
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
        [NonAction]
        private long[] OrderMatterIds(long[] matterIds, short characteristicLinkId, long[] chains)
        {
            double[] completeSequencesCharacteristics = sequencesCharacteristicsCalculator.Calculate(chains, characteristicLinkId);

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
        [NonAction]
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
        [NonAction]
        private List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> ExtractSimilarPairs(
            Dictionary<double, List<(int matterId, int subsequenceIndex, double[] additionalCharacteristics /*все кроме 1*/)>> characteristicValueSubsequences,
            double[] percentageDifferences)
        {
            var similarPairs = new List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>(characteristicValueSubsequences.Count);
            
            foreach (double key in characteristicValueSubsequences.Keys)
            {
                similarPairs.AddRange(ExtractAllPossiblePairs(characteristicValueSubsequences[key], percentageDifferences));
            }

            double[] orderedCharacteristicValue = characteristicValueSubsequences.Keys.OrderBy(v => v).ToArray();
            for (int i = 0; i < orderedCharacteristicValue.Length - 1; i++)
            {
                int j = i + 1;
                double difference = CalculateAverageDifference(orderedCharacteristicValue[i], orderedCharacteristicValue[j]);
                while (difference <= percentageDifferences[0]) // Не понял.
                {
                    List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)> firstComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[i]];
                    List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)> secondComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[j]];
                    similarPairs.AddRange(ExtractAllPossiblePairs(firstComponentIndex, secondComponentIndex, percentageDifferences, difference));

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
        [NonAction]
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
        [NonAction]
        private List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> ExtractAllPossiblePairs(
            List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)> list, double[] differences) // добавить доп характеристики + массив dif
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
                    var areSimilar = true;
                    // Не понял
                    for (int k = 0; k < differences.Length - 1; k++)
                    {
                        var difference = CalculateAverageDifference(list[i].additionalCharacteristics[k], list[j].additionalCharacteristics[k]);
                        if (difference > differences[k + 1])
                        {
                            areSimilar = false;
                            break;
                        }
                    }
                    if (areSimilar)
                    {
                        result.Add(((list[i].matterIndex, list[i].subsequenceIndex), (list[j].matterIndex, list[j].subsequenceIndex), 0)); // добавить "если" разница не больше dif то добавляем, выкинуть нулевой элемент
                    }
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
        [NonAction]
        private List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> ExtractAllPossiblePairs(
            List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)> firstList,
            List<(int matterIndex, int subsequenceIndex, double[] additionalCharacteristics)> secondList,
            double[] differences,
            double primaryDifference
        ) // массив даблов
        {
            var result = new List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)>();


            foreach (var firstElement in firstList)
            {
                foreach (var secondElement in secondList)
                {
                    //result.Add((firstElement, secondElement, differences)); // аналогично

                    var areSimilar = true;
                    // Не понял
                    for (int k = 0; k < differences.Length - 1; k++)
                    {
                        var difference = CalculateAverageDifference(firstElement.additionalCharacteristics[k], secondElement.additionalCharacteristics[k]);
                        if (difference > differences[k + 1])
                        {
                            areSimilar = false;
                            break;
                        }
                    }
                    if (areSimilar)
                    {
                        result.Add(((firstElement.matterIndex, firstElement.subsequenceIndex), (secondElement.matterIndex, secondElement.subsequenceIndex), primaryDifference)); // добавить "если" разница не больше dif то добавляем, выкинуть нулевой элемент
                    }
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
        [NonAction]
        private List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] FillSimilarityMatrix(
            int mattersCount,
            List<((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs)
        {
            var similarityMatrix = new List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[mattersCount, mattersCount];
            for (int i = 0; i < mattersCount; i++)
            {
                for (int j = 0; j < mattersCount; j++)
                {
                    similarityMatrix[i, j] = new List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>();
                }
            }

            foreach (((int matterIndex, int subsequenceIndex) firstSequence, (int matterIndex, int subsequenceIndex) secondSequence, double difference) in similarPairs)
            {
                (int firstMatter, int firstSubsequence) = firstSequence;
                (int secondMatter, int secondSubsequence) = secondSequence;

                (int firstSubsequenceIndex, int secondSubsequenceIndex, double difference) similarityData = (firstSubsequence, secondSubsequence, difference);
                similarityMatrix[firstMatter, secondMatter].Add(similarityData);

                //  TODO: get rid of duplicate data
                (int firstSubsequenceIndex, int secondSubsequenceIndex, double difference) symmetricalSimilarityData = (secondSubsequence, firstSubsequence, difference);
                similarityMatrix[secondMatter, firstMatter].Add(symmetricalSimilarityData);
            }

            return similarityMatrix;
        }
    }
}
