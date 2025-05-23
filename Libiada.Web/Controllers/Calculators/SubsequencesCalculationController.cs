﻿namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Calculators;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;
using Libiada.Web.Extensions;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// The subsequences calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SubsequencesCalculationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
    /// </summary>
    public SubsequencesCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                             IViewDataBuilder viewDataBuilder,
                                             ITaskManager taskManager,
                                             IFullCharacteristicRepository characteristicTypeLinkRepository,
                                             ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator)
        : base(TaskType.SubsequencesCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var viewData = viewDataBuilder.AddMinMaxResearchObjects()
                                      .AddSequenceGroups()
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
    /// <param name="characteristicLinkIds">
    /// The characteristic type and link ids.
    /// </param>
    /// <param name="features">
    /// The feature ids.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(long[] researchObjectIds, short[] characteristicLinkIds, Feature[] features)
    {
        return CreateTask(() =>
        {
            var sequencesData = new SequenceData[researchObjectIds.Length];
            using var db = dbFactory.CreateDbContext();
            long[] parentSequenceIds;
            string[] researchObjectNames = new string[researchObjectIds.Length];
            string[] remoteIds = new string[researchObjectIds.Length];
                      
            var parentSequences = db.CombinedSequenceEntities.Include(s => s.ResearchObject)
                                    .Where(s => s.Notation == Notation.Nucleotides && researchObjectIds.Contains(s.ResearchObjectId))
                                    .Select(s => new { s.Id, ResearchObjectName = s.ResearchObject.Name, s.RemoteId })
                                    .ToDictionary(s => s.Id);
            parentSequenceIds = parentSequences.Keys.ToArray();

            for (int n = 0; n < parentSequenceIds.Length; n++)
            {
                researchObjectNames[n] = parentSequences[parentSequenceIds[n]].ResearchObjectName;
                remoteIds[n] = parentSequences[parentSequenceIds[n]].RemoteId;
            }

            var subsequencesCharacteristicsList = new SelectListItem[characteristicLinkIds.Length];
            string[] subsequencesCharacteristicsNames = new string[characteristicLinkIds.Length];
            for (int k = 0; k < characteristicLinkIds.Length; k++)
            {
                subsequencesCharacteristicsNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k]);
                subsequencesCharacteristicsList[k] = new SelectListItem
                {
                    Value = k.ToString(),
                    Text = subsequencesCharacteristicsNames[k],
                    Selected = false
                };
            }

            // TODO: Maybe AttributesValueCache should be created in the Subsequences calculator
            var attributeValuesCache = new AttributeValueCacheManager(db);
            for (int i = 0; i < parentSequenceIds.Length; i++)
            {
                SubsequenceData[] subsequencesData = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(characteristicLinkIds, features, parentSequenceIds[i]);
                subsequencesData = subsequencesData.OrderBy(sd => sd.Starts[0]).ToArray();
                attributeValuesCache.FillAttributeValues(subsequencesData);

                sequencesData[i] = new SequenceData(researchObjectIds[i], researchObjectNames[i], remoteIds[i], default, subsequencesData);
            }

            List<AttributeValue> allAttributeValues = attributeValuesCache.AllAttributeValues;

            var result = new Dictionary<string, object>
            {
                { "sequencesData", sequencesData },
                { "features", features.ToSelectList(features).ToDictionary(f => f.Value) },
                { "attributes", EnumExtensions.ToArray<AnnotationAttribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                { "attributeValues", allAttributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                { "characteristicNames", subsequencesCharacteristicsNames },
                { "characteristicsList", subsequencesCharacteristicsList }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
