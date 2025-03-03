﻿namespace Libiada.Web.Controllers.Calculators;

using System.Globalization;

using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
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
    private readonly IViewDataBuilder viewDataBuilder;

    /// <summary>
    /// The sequence attribute repository.
    /// </summary>
    private readonly SequenceAttributeRepository sequenceAttributeRepository;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsequencesSimilarityController"/> class.
    /// </summary>
    public SubsequencesSimilarityController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                            IViewDataBuilder viewDataBuilder,
                                            ITaskManager taskManager,
                                            IFullCharacteristicRepository characteristicTypeLinkRepository,
                                            ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                            IResearchObjectsCache cache)
        : base(TaskType.SubsequencesSimilarity, taskManager)
    {
        this.dbFactory = dbFactory;
        this.db = dbFactory.CreateDbContext();
        this.viewDataBuilder = viewDataBuilder;
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
        var viewData = viewDataBuilder.AddMinMaxResearchObjects(2, 2)
                                      .AddCharacteristicsData(CharacteristicCategory.Full)
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
    /// Thrown if count of research objects is not 2.
    /// </exception>
    [HttpPost]
    public ActionResult Index(
        long[] researchObjectIds,
        short characteristicLinkId,
        Notation notation,
        Feature[] features,
        string maxDifference,
        string excludeType)
    {
        return CreateTask(() =>
        {
            if (researchObjectIds.Length != 2)
            {
                throw new ArgumentException("Number of selected research objects must be 2.", nameof(researchObjectIds));
            }

            long firstResearchObjectId = researchObjectIds[0];
            long firstParentSequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == firstResearchObjectId && c.Notation == notation).Id;
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

            long secondResearchObjectId = researchObjectIds[1];
            long secondParentSequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == secondResearchObjectId && c.Notation == notation).Id;
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
                { "firstSequenceName", cache.ResearchObjects.Single(m => m.Id == firstResearchObjectId).Name },
                { "secondSequenceName", cache.ResearchObjects.Single(m => m.Id == secondResearchObjectId).Name },
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
