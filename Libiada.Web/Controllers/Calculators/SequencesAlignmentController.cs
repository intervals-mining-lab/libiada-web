namespace Libiada.Web.Controllers.Calculators;

using Bio.Extensions;

using Libiada.Core.Extensions;
using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;
using Libiada.Web.Helpers;
using Libiada.Web.Math;
using Libiada.Web.Tasks;

using Newtonsoft.Json;

/// <summary>
/// The alignment controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequencesAlignmentController : AbstractResultController
{
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
    private readonly IResearchObjectsCache cache;
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly IArrayAligner arrayAligner;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequencesAlignmentController"/> class.
    /// </summary>
    public SequencesAlignmentController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                        IViewDataBuilder viewDataBuilder,
                                        ITaskManager taskManager,
                                        IFullCharacteristicRepository characteristicTypeLinkRepository,
                                        ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                        IResearchObjectsCache cache,
                                        IArrayAligner arrayAligner)
        : base(TaskType.SequencesAlignment, taskManager)
    {
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
        this.cache = cache;
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
        this.arrayAligner = arrayAligner;
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
    /// The characteristic id.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <param name="features">
    /// The feature ids.
    /// </param>
    /// <param name="validationType">
    /// The validation type.
    /// </param>
    /// <param name="cyclicShift">
    /// The cyclic shift.
    /// </param>
    /// <param name="sort">
    /// The sort.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if validationType is unknown.
    /// Or if count of research objects is not 2.
    /// </exception>
    [HttpPost]
    public ActionResult Index(
        long[] researchObjectIds,
        short characteristicLinkId,
        Notation notation,
        Feature[] features,
        string validationType,
        bool cyclicShift,
        bool sort)
    {
        return CreateTask(() =>
        {
            if (researchObjectIds.Length != 2)
            {
                throw new ArgumentException("Number of selected research objects must be 2.", nameof(researchObjectIds));
            }

            using var db = dbFactory.CreateDbContext();

            long firstResearchObjectId = researchObjectIds[0];
            string firstResearchObjectName = cache.ResearchObjects.Single(m => m.Id == firstResearchObjectId).Name;
            long firstParentId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == firstResearchObjectId && c.Notation == notation).Id;

            long secondResearchObjectId = researchObjectIds[1];
            string secondResearchObjectName = cache.ResearchObjects.Single(m => m.Id == secondResearchObjectId).Name;
            long secondParentId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == secondResearchObjectId && c.Notation == notation).Id;

            double[] firstSequenceCharacteristics = subsequencesCharacteristicsCalculator
                .CalculateSubsequencesCharacteristics(firstParentId, characteristicLinkId, features);
            double[] secondSequenceCharacteristics = subsequencesCharacteristicsCalculator
                .CalculateSubsequencesCharacteristics(secondParentId, characteristicLinkId, features);

            if (sort)
            {
                Array.Sort(firstSequenceCharacteristics, (x, y) => y.CompareTo(x));
                Array.Sort(secondSequenceCharacteristics, (x, y) => y.CompareTo(x));
            }

            List<double> longer;
            List<double> shorter;
            if (firstSequenceCharacteristics.Length >= secondSequenceCharacteristics.Length)
            {
                longer = firstSequenceCharacteristics.ToList();
                shorter = secondSequenceCharacteristics.ToList();
            }
            else
            {
                longer = secondSequenceCharacteristics.ToList();
                shorter = firstSequenceCharacteristics.ToList();
            }

            if (!cyclicShift)
            {
                int count = longer.Count;
                for (int i = 0; i < count; i++)
                {
                    longer.Add(0);
                }
            }

            var distanceCalculator = arrayAligner.GetDistanceCalculator(validationType);
            (double[] distances, int optimalRotation) = arrayAligner.CalculateMeasureForAllRotations(longer, shorter, distanceCalculator);

            string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

            var result = new Dictionary<string, object>
            {
                { "firstSequenceName", firstResearchObjectName },
                { "secondSequenceName", secondResearchObjectName },
                { "characteristicName", characteristicName },
                { "features", features.ConvertAll(p => p.GetDisplayValue()) },
                { "optimalRotation", optimalRotation },
                { "distances", distances.Select(el => new { Value = el }) },
                { "validationType", validationType },
                { "cyclicShift", cyclicShift },
                { "sort", sort }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
