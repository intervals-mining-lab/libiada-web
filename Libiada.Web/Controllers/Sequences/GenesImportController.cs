namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Tasks;
using Libiada.Database.Helpers;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Newtonsoft.Json;
using Libiada.Database.Models.Repositories.Sequences;

/// <summary>
/// The genes import controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class GenesImportController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly INcbiHelper ncbiHelper;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenesImportController"/> class.
    /// </summary>
    public GenesImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                 IViewDataHelper viewDataHelper,
                                 ITaskManager taskManager,
                                 INcbiHelper ncbiHelper,
                                 IResearchObjectsCache cache)
        : base(TaskType.GenesImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.ncbiHelper = ncbiHelper;
        this.cache = cache;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        using var db = dbFactory.CreateDbContext();
        var genesSequenceIds = db.Subsequences.Select(s => s.SequenceId).Distinct();

        var researchObjectIds = db.CombinedSequenceEntities
                          .Include(c => c.ResearchObject)
                          .Where(c => !string.IsNullOrEmpty(c.RemoteId)
                                   && !genesSequenceIds.Contains(c.Id)
                                   && StaticCollections.SequenceTypesWithSubsequences.Contains(c.ResearchObject.SequenceType))
                          .Select(c => c.ResearchObjectId).ToList();

        var data = viewDataHelper.FillViewData(1, 1, m => researchObjectIds.Contains(m.Id), "Import");
        data.Add("nature", (byte)Nature.Genetic);
        ViewBag.data = JsonConvert.SerializeObject(data);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectId">
    /// The research object id.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(long researchObjectId)
    {
        return CreateTask(() =>
        {
            Dictionary<string, object> result;
            using var db = dbFactory.CreateDbContext();
            GeneticSequence parentSequence = db.CombinedSequenceEntities.Single(d => d.ResearchObjectId == researchObjectId).ToGeneticSequence();
            var subsequenceImporter = new SubsequenceImporter(db, parentSequence, ncbiHelper);
            subsequenceImporter.CreateSubsequences();


            var features = EnumExtensions.ToArray<Feature>().ToDictionary(f => (byte)f, f => f.GetDisplayValue());
            string researchObjectName = cache.ResearchObjects.Single(m => m.Id == researchObjectId).Name;
            SubsequenceData[] sequenceSubsequences = db.Subsequences
                .Where(s => s.SequenceId == parentSequence.Id)
                .Include(s => s.Position)
                .ToArray()
                .Select(s => new SubsequenceData(s))
                .ToArray();

            result = new Dictionary<string, object>
            {
                { "researchObjectName", researchObjectName },
                { "genes", sequenceSubsequences },
                { "features", features }
            };


            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
