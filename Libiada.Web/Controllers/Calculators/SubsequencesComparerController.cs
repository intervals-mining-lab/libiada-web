namespace Libiada.Web.Controllers.Calculators;

using System.Globalization;

using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Sequences;

using Newtonsoft.Json;

using Libiada.Core.Extensions;
using Libiada.Core.TimeSeries.OneDimensional.DistanceCalculators;
using Libiada.Core.TimeSeries.OneDimensional.Comparers;
using Libiada.Core.TimeSeries.Aligners;
using Libiada.Core.TimeSeries.Aggregators;

using Libiada.Web.Tasks;
using Libiada.Web.Extensions;
using Libiada.Web.Helpers;

using static Libiada.Core.Extensions.EnumExtensions;

/// <summary>
/// The subsequences comparer controller.
/// </summary>
[Authorize]
public class SubsequencesComparerController : AbstractResultController
{
    //TODO: replace all possible tupels with classes in this file 

    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IFullCharacteristicRepository fullCharacteristicRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
    private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly IViewDataHelperFactory viewDataHelperFactory;
    private readonly GeneticSequenceRepository geneticSequenceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsequencesComparerController"/> class.
    /// </summary>
    public SubsequencesComparerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                          ITaskManager taskManager,
                                          IFullCharacteristicRepository fullCharacteristicRepository,
                                          ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                          ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                          ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                          IResearchObjectsCache cache,
                                          IViewDataHelperFactory viewDataHelperFactory)
        : base(TaskType.SubsequencesComparer, taskManager)
    {
        this.dbFactory = dbFactory;
        this.fullCharacteristicRepository = fullCharacteristicRepository;
        this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
        this.sequencesCharacteristicsCalculator = sequencesCharacteristicsCalculator;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.viewDataHelperFactory = viewDataHelperFactory;
        geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index([FromServices] IViewDataHelper viewDataHelper)
    {
        var viewData = viewDataHelper.AddResearchObjectsWithSubsequences()
                                     .AddMinMaxResearchObjects(2)
                                     .AddCharacteristicsData(CharacteristicCategory.Full)
                                     .AddMaxPercentageDifferenceRequiredFlag()
                                     .AddSubmitName("Compare")
                                     .SetNature(Nature.Genetic)
                                     .AddNotations(onlyGenetic: true)
                                     .AddSequenceTypes(onlyGenetic: true)
                                     .AddGroups(onlyGenetic: true)
                                     .AddFeatures()
                                     .Build();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The research objects ids.
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
    [HttpPost]
    public ActionResult Index(
        long[] researchObjectIds,
        short characteristicLinkId,
        short[] characteristicLinkIds,
        Feature[] features,
        string[] maxPercentageDifferences,
        string[] filters,
        bool filterMatrix
    )
    {
        var user = User;
        return CreateTask(() =>
        {
            double[] percentageDifferences = maxPercentageDifferences.Select(item => double.Parse(item, CultureInfo.InvariantCulture) / 100).ToArray();
            using var db = dbFactory.CreateDbContext();
            var attributeValuesCache = new AttributeValueCacheManager(db);
            var characteristics = new SubsequenceData[researchObjectIds.Length][];

            long[] parentSequenceIds;
            string[] researchObjectNames = new string[researchObjectIds.Length];

            int researchObjectsCount = researchObjectIds.Length;
            Dictionary<string, object> characteristicsTypesData;


            // Sequences characteristic
            long[] sequences = geneticSequenceRepository.GetNucleotideSequenceIds(researchObjectIds);

            // Sequences characteristic
            researchObjectIds = OrderResearchObjectIds(researchObjectIds, characteristicLinkId, sequences);

            // Subsequences characteristics
            var parentSequences = db.CombinedSequenceEntities.Include(s => s.ResearchObject)
                                    .Where(s => s.Notation == Notation.Nucleotides && researchObjectIds.Contains(s.ResearchObjectId))
                                    .Select(s => new { s.Id, s.ResearchObjectId, ResearchObjectName = s.ResearchObject.Name })
                                    .ToDictionary(s => s.Id);

            parentSequenceIds = parentSequences
                                .OrderBy(ps => Array.IndexOf(researchObjectIds, ps.Value.ResearchObjectId))
                                .Select(ps => ps.Key)
                                .ToArray();

            for (int n = 0; n < parentSequenceIds.Length; n++)
            {
                researchObjectNames[n] = parentSequences[parentSequenceIds[n]].ResearchObjectName;
            }

            // TODO: refactor this
            using var viewDataHelper = viewDataHelperFactory.Create(user);
            characteristicsTypesData = viewDataHelper.AddCharacteristicsData(CharacteristicCategory.Full)
                                                     .Build();


            string sequenceCharacteristicName = fullCharacteristicRepository.GetCharacteristicName(characteristicLinkId);
            string characteristicName = fullCharacteristicRepository.GetCharacteristicName(characteristicLinkIds[0]);

            var characteristicValueSubsequences = new Dictionary<double, List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)>>();

            // cycle through research objects
            for (int i = 0; i < researchObjectsCount; i++)
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
                    if (characteristicValueSubsequences.TryGetValue(value, out List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)> researchObjectAndSubsequenceIdsList))
                    {
                        researchObjectAndSubsequenceIdsList.Add((i, j, subsequencesData[j].CharacteristicsValues.Skip(1).ToArray()));
                    }
                    else
                    {
                        researchObjectAndSubsequenceIdsList =
                        [
                            (researchObjectIndex: i, subsequenceIndex: j, additionalCharacterisctics: subsequencesData[j].CharacteristicsValues.Skip(1).ToArray())
                        ]; // добавить в кортеж все характеристики кроме 0
                        characteristicValueSubsequences.Add(value, researchObjectAndSubsequenceIdsList);
                    }
                }
            }


            List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs =
                ExtractSimilarPairs(characteristicValueSubsequences, percentageDifferences);

            List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] similarityMatrix =
                FillSimilarityMatrix(researchObjectsCount, similarPairs);

            object[,] similarities = Similarities(similarityMatrix, characteristics, researchObjectsCount);

            List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> filteredSimilarPairs;
            List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] filteredSimilarityMatrix = null;
            object[,] filteredSimilarities = null;

            if (filterMatrix)
            {
                filteredSimilarPairs = FilterSimilarityPairs(similarPairs, characteristics, characteristicLinkIds[0]);
                filteredSimilarityMatrix = FillSimilarityMatrix(researchObjectsCount, filteredSimilarPairs);
                filteredSimilarities = Similarities(filteredSimilarityMatrix, characteristics, researchObjectsCount);
            }

            List<AttributeValue> allAttributeValues = attributeValuesCache.AllAttributeValues;

            var result = new Dictionary<string, object>
            {
                { "researchObjectsNames", researchObjectNames },
                { "characteristicName", characteristicName },
                { "similarities", similarities },
                { "filteredSimilarities", filteredSimilarities },
                { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                { "attributes", ToArray<AnnotationAttribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
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
    private List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> FilterSimilarityPairs
        (
            List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs,
            SubsequenceData[][] characteristics,
            short subsequencesCharacteristicLinkId
        )
    {
        using var db = dbFactory.CreateDbContext();
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        var localCharacteristicsCalculator = new LocalCharacteristicsCalculator(db, fullCharacteristicRepository, sequenceRepository);

        var cache = new Dictionary<(int researchObjectIndex, int subsequenceIndex), double[]>();
        List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> result = [];

        var alignersFactory = new AlignersFactory();
        var calculatorsFactory = new DistanceCalculatorsFactory();
        var aggregatorsFactory = new AggregatorsFactory();

        ITimeSeriesAligner aligner = alignersFactory.GetAligner(Aligner.AllOffsetsAligner);
        IOneDimensionalPointsDistance distanceCalculator = calculatorsFactory.GetDistanceCalculator(DistanceCalculator.EuclideanDistanceBetweenOneDimensionalPointsCalculator);
        IDistancesAggregator aggregator = aggregatorsFactory.GetAggregator(Aggregator.Average);

        var timeSeriesComparer = new OneDimensionalTimeSeriesComparer(aligner, distanceCalculator, aggregator);

        for (int i = 0; i < similarPairs.Count; i++)
        {
            var similarPair = similarPairs[i];
            if (!cache.TryGetValue((similarPair.firstSequence.researchObjectIndex, similarPair.firstSequence.subsequenceIndex), out double[] firstLocalCharacteristics))
            {
                var subsequenceData = characteristics[similarPair.firstSequence.researchObjectIndex][similarPair.firstSequence.subsequenceIndex];
                firstLocalCharacteristics = localCharacteristicsCalculator.GetSubsequenceCharacteristic(subsequenceData.Id, subsequencesCharacteristicLinkId, 50, 1);
                cache.Add((similarPair.firstSequence.researchObjectIndex, similarPair.firstSequence.subsequenceIndex), firstLocalCharacteristics);
                //TODO: get rid of hardcoded parameters
            }
            if (!cache.TryGetValue((similarPair.secondSequence.researchObjectIndex, similarPair.secondSequence.subsequenceIndex), out double[] secondLocalCharacteristics))
            {
                var subsequenceData = characteristics[similarPair.secondSequence.researchObjectIndex][similarPair.secondSequence.subsequenceIndex];
                secondLocalCharacteristics = localCharacteristicsCalculator.GetSubsequenceCharacteristic(subsequenceData.Id, subsequencesCharacteristicLinkId, 50, 1);
                cache.Add((similarPair.secondSequence.researchObjectIndex, similarPair.secondSequence.subsequenceIndex), secondLocalCharacteristics);
                //TODO: get rid of hardcoded parameters
            }

            double distance = timeSeriesComparer.GetDistance(firstLocalCharacteristics, secondLocalCharacteristics);
            if (distance <= 1.5)
            {
                result.Add((similarPair.firstSequence, similarPair.secondSequence, distance));
            }
        }

        return result;
    }

    /// <summary>
    /// Orders research objects ids by characteristic values.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The research objects ids.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic link id.
    /// </param>
    /// <param name="sequences">
    /// The sequences.
    /// </param>
    /// <returns>
    /// The <see cref="T:long[]"/>.
    /// </returns>
    [NonAction]
    private long[] OrderResearchObjectIds(long[] researchObjectIds, short characteristicLinkId, long[] sequences)
    {
        double[] completeSequencesCharacteristics = sequencesCharacteristicsCalculator.Calculate(sequences, characteristicLinkId);

        var researchObjectCharacteristics = new (long researchObjectId, double charcterisitcValue)[researchObjectIds.Length];

        for (int i = 0; i < completeSequencesCharacteristics.Length; i++)
        {
            researchObjectCharacteristics[i] = (researchObjectIds[i], completeSequencesCharacteristics[i]);
        }

        return researchObjectCharacteristics.OrderBy(mc => mc.charcterisitcValue).Select(mc => mc.researchObjectId).ToArray();
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
    /// <param name="researchObjectsCount">
    /// The research objects count.
    /// </param>
    /// <returns>
    /// The <see cref="T:object[,]"/>.
    /// </returns>
    [NonAction]
    private object[,] Similarities(
        List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] similarityMatrix,
        SubsequenceData[][] characteristics,
        int researchObjectsCount)
    {
        object[,] similarities = new object[researchObjectsCount, researchObjectsCount];
        for (int i = 0; i < researchObjectsCount; i++)
        {
            for (int j = 0; j < researchObjectsCount; j++)
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

                double equalSequencesCount = System.Math.Min(firstEqualCount, secondEqualCount) * 2d;
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

                double similarSequencesCharacteristicSum = System.Math.Min(firstCharacteristicSum, secondCharacteristicSum) * 2d;

                double fistSequenceCharacteristicSum = characteristics[i].Sum(c => c.CharacteristicsValues[0]);
                double secondSequenceCharacteristicSum = characteristics[j].Sum(c => c.CharacteristicsValues[0]);
                double allSequencesCharacteristicSum = fistSequenceCharacteristicSum + secondSequenceCharacteristicSum;
                double formula3 = similarSequencesCharacteristicSum / allSequencesCharacteristicSum;

                const int digits = 5;
                similarities[i, j] = new
                {
                    formula1 = System.Math.Round(formula1, digits),
                    formula2 = System.Math.Round(formula2, digits),
                    formula3 = System.Math.Round(formula3, digits),
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
    /// The <see cref="List{((int, int), (int, int), double)}"/>.
    /// </returns>
    [NonAction]
    private List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> ExtractSimilarPairs(
        Dictionary<double, List<(int researchObjectId, int subsequenceIndex, double[] additionalCharacteristics /*все кроме 1*/)>> characteristicValueSubsequences,
        double[] percentageDifferences)
    {
        List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs = new(characteristicValueSubsequences.Count);

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
                List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)> firstComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[i]];
                List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)> secondComponentIndex = characteristicValueSubsequences[orderedCharacteristicValue[j]];
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
        return System.Math.Abs((first - second) / ((first + second) / 2));
    }

    /// <summary>
    /// Extract all possible unique pairs from given list.
    /// (calculates Cartesian product)
    /// </summary>
    /// <param name="list">
    /// The list for pairs extraction.
    /// </param>
    /// <returns>
    /// The <see cref="List{((int,int), (int,int),double)}"/>.
    /// </returns>
    [NonAction]
    private List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> ExtractAllPossiblePairs(
        List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)> list, double[] differences) // добавить доп характеристики + массив dif
    {
        List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> result = [];
        if (list.Count < 2)
        {
            return result;
        }

        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                bool areSimilar = true;
                // Не понял
                for (int k = 0; k < differences.Length - 1; k++)
                {
                    double difference = CalculateAverageDifference(list[i].additionalCharacteristics[k], list[j].additionalCharacteristics[k]);
                    if (difference > differences[k + 1])
                    {
                        areSimilar = false;
                        break;
                    }
                }
                if (areSimilar)
                {
                    result.Add(((list[i].researchObjectIndex, list[i].subsequenceIndex), (list[j].researchObjectIndex, list[j].subsequenceIndex), 0)); // добавить "если" разница не больше dif то добавляем, выкинуть нулевой элемент
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
    /// The <see cref="List{((int,int), (int,int),double)}"/>.
    /// </returns>
    [NonAction]
    private List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> ExtractAllPossiblePairs(
        List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)> firstList,
        List<(int researchObjectIndex, int subsequenceIndex, double[] additionalCharacteristics)> secondList,
        double[] differences,
        double primaryDifference
    )
    {
        List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> result = [];


        foreach (var firstElement in firstList)
        {
            foreach (var secondElement in secondList)
            {
                //result.Add((firstElement, secondElement, differences)); // аналогично

                bool areSimilar = true;
                // Не понял
                for (int k = 0; k < differences.Length - 1; k++)
                {
                    double difference = CalculateAverageDifference(firstElement.additionalCharacteristics[k], secondElement.additionalCharacteristics[k]);
                    if (difference > differences[k + 1])
                    {
                        areSimilar = false;
                        break;
                    }
                }
                if (areSimilar)
                {
                    result.Add(((firstElement.researchObjectIndex, firstElement.subsequenceIndex), (secondElement.researchObjectIndex, secondElement.subsequenceIndex), primaryDifference)); // добавить "если" разница не больше dif то добавляем, выкинуть нулевой элемент
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Fills similarity matrix from similarity pairs list.
    /// </summary>
    /// <param name="researchObjectsCount">
    /// The research objects count.
    /// </param>
    /// <param name="similarPairs">
    /// The similar pairs.
    /// </param>
    /// <returns>
    /// The <see cref="T:List{(int, int, double)}[,]"/>.
    /// </returns>
    [NonAction]
    private List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[,] FillSimilarityMatrix(
        int researchObjectsCount,
        List<((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference)> similarPairs)
    {
        var similarityMatrix = new List<(int firstSubsequenceIndex, int secondSubsequenceIndex, double difference)>[researchObjectsCount, researchObjectsCount];
        for (int i = 0; i < researchObjectsCount; i++)
        {
            for (int j = 0; j < researchObjectsCount; j++)
            {
                similarityMatrix[i, j] = [];
            }
        }

        foreach (((int researchObjectIndex, int subsequenceIndex) firstSequence, (int researchObjectIndex, int subsequenceIndex) secondSequence, double difference) in similarPairs)
        {
            (int firstResearchObject, int firstSubsequence) = firstSequence;
            (int secondResearchObject, int secondSubsequence) = secondSequence;

            (int firstSubsequenceIndex, int secondSubsequenceIndex, double difference) similarityData = (firstSubsequence, secondSubsequence, difference);
            similarityMatrix[firstResearchObject, secondResearchObject].Add(similarityData);

            //  TODO: get rid of duplicate data
            (int firstSubsequenceIndex, int secondSubsequenceIndex, double difference) symmetricalSimilarityData = (secondSubsequence, firstSubsequence, difference);
            similarityMatrix[secondResearchObject, firstResearchObject].Add(symmetricalSimilarityData);
        }

        return similarityMatrix;
    }
}
