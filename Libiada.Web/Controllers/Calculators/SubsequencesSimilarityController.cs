namespace Libiada.Web.Controllers.Calculators;

using System.Globalization;

using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;

using Libiada.Database.Models.Calculators;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

/// <summary>
/// The subsequences similarity controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SubsequencesSimilarityController : AbstractResultController
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// The sequence attribute repository.
    /// </summary>
    private readonly SequenceAttributeRepository sequenceAttributeRepository;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsequencesSimilarityController"/> class.
    /// </summary>
    public SubsequencesSimilarityController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                            IViewDataHelper viewDataHelper,
                                            ITaskManager taskManager,
                                            IFullCharacteristicRepository characteristicTypeLinkRepository,
                                            ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                            Cache cache)
        : base(TaskType.SubsequencesSimilarity, taskManager)
    {
        this.dbFactory = dbFactory;
        this.db = dbFactory.CreateDbContext();
        this.viewDataHelper = viewDataHelper;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
        this.cache = cache;
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
            long firstParentSequenceId = db.CommonSequences.Single(c => c.MatterId == firstMatterId && c.Notation == notation).Id;
            SubsequenceData[] firstSequenceSubsequences = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                                                                [characteristicLinkId],
                                                                features,
                                                                firstParentSequenceId);
            List<double> firstSequenceCharacteristics = firstSequenceSubsequences.Select(s => s.CharacteristicsValues[0]).ToList();
            Dictionary<long, AttributeValue[]> firstDbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(firstSequenceSubsequences.Select(s => s.Id));
            List<AttributeValue[]> firstSequenceAttributes = [];
            foreach (SubsequenceData subsequence in firstSequenceSubsequences)
            {
                firstDbSubsequencesAttributes.TryGetValue(subsequence.Id, out AttributeValue[]? attributes);
                attributes ??= [];
                firstSequenceAttributes.Add(attributes);
            }

            long secondMatterId = matterIds[1];
            long secondParentSequenceId = db.CommonSequences.Single(c => c.MatterId == secondMatterId && c.Notation == notation).Id;
            SubsequenceData[] secondSequenceSubsequences = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                                                                [characteristicLinkId],
                                                                features,
                                                                secondParentSequenceId);
            List<double> secondSequenceCharacteristics = secondSequenceSubsequences.Select(s => s.CharacteristicsValues[0]).ToList();
            Dictionary<long, AttributeValue[]> secondDbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(secondSequenceSubsequences.Select(s => s.Id));
            List<AttributeValue[]> secondSequenceAttributes = [];
            foreach (SubsequenceData subsequence in secondSequenceSubsequences)
            {
                secondDbSubsequencesAttributes.TryGetValue(subsequence.Id, out AttributeValue[]? attributes);
                attributes ??= [];
                secondSequenceAttributes.Add(attributes);
            }

            double difference = double.Parse(maxDifference, CultureInfo.InvariantCulture);

            List<(int, int)> similarSubsequences = [];

            for (int i = 0; i < firstSequenceCharacteristics.Count; i++)
            {
                for (int j = 0; j < secondSequenceCharacteristics.Count; j++)
                {
                    if (System.Math.Abs(firstSequenceCharacteristics[i] - secondSequenceCharacteristics[j]) <= difference)
                    {
                        similarSubsequences.Add((i, j));

                        if (excludeType == "Exclude")
                        {
                            firstSequenceCharacteristics[i] = double.NaN;
                            secondSequenceCharacteristics[j] = double.NaN;
                        }
                    }
                }
            }

            string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

            double similarity = similarSubsequences.Count * 200d / (firstSequenceSubsequences.Length + secondSequenceSubsequences.Length);

            double firstSequenceSimilarity = similarSubsequences.Count * 100d / firstSequenceSubsequences.Length;

            double secondSequenceSimilarity = similarSubsequences.Count * 100d / secondSequenceSubsequences.Length;

            var result = new Dictionary<string, object>
            {
                { "firstSequenceName", cache.Matters.Single(m => m.Id == firstMatterId).Name },
                { "secondSequenceName", cache.Matters.Single(m => m.Id == secondMatterId).Name },
                { "characteristicName", characteristicName },
                { "similarSubsequences", similarSubsequences },
                { "similarity", similarity },
                { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                { "firstSequenceSimilarity", firstSequenceSimilarity },
                { "secondSequenceSimilarity", secondSequenceSimilarity },
                { "firstSequenceSubsequences", firstSequenceSubsequences },
                { "secondSequenceSubsequences", secondSequenceSubsequences },
                { "firstSequenceAttributes", firstSequenceAttributes },
                { "secondSequenceAttributes", secondSequenceAttributes }
            };

            string json = JsonConvert.SerializeObject(result);

            return new Dictionary<string, string> { { "data", json } };
        });
    }
}
