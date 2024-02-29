namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Helpers;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

/// <summary>
/// The batch genes import controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class BatchGenesImportController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly INcbiHelper ncbiHelper;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchGenesImportController"/> class.
    /// </summary>
    public BatchGenesImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                      IViewDataHelper viewDataHelper, 
                                      ITaskManager taskManager,
                                      INcbiHelper ncbiHelper,
                                      Cache cache)
        : base(TaskType.BatchGenesImport, taskManager)
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
        var sequencesWithSubsequencesIds = db.Subsequences.Select(s => s.SequenceId).Distinct();

        var matterIds = db.DnaSequences.Include(c => c.Matter)
            .Where(c => !string.IsNullOrEmpty(c.RemoteId)
                     && !sequencesWithSubsequencesIds.Contains(c.Id)
                     && StaticCollections.SequenceTypesWithSubsequences.Contains(c.Matter.SequenceType))
            .Select(c => c.MatterId).ToArray();

        var data = viewDataHelper.FillViewData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Import");
        data.Add("nature", (byte)Nature.Genetic);
        ViewBag.data = JsonConvert.SerializeObject(data);

        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterIds">
    /// The matter ids.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(long[] matterIds)
    {
        return CreateTask(() =>
            {
                using var db = dbFactory.CreateDbContext();
                string[] matterNames;
                var importResults = new List<MatterImportResult>(matterIds.Length);

                matterNames = cache.Matters
                                   .Where(m => matterIds.Contains(m.Id))
                                   .OrderBy(m => m.Id)
                                   .Select(m => m.Name)
                                   .ToArray();
                var parentSequences = db.DnaSequences
                                        .Where(c => matterIds.Contains(c.MatterId))
                                        .OrderBy(c => c.MatterId)
                                        .ToArray();

                for (int i = 0; i < parentSequences.Length; i++)
                {
                    var importResult = new MatterImportResult()
                    {
                        MatterName = matterNames[i]
                    };

                    try
                    {
                        DnaSequence parentSequence = parentSequences[i];
                        var subsequenceImporter = new SubsequenceImporter(db, parentSequence, ncbiHelper);

                        subsequenceImporter.CreateSubsequences();


                        int featuresCount = db.Subsequences.Count(s => s.SequenceId == parentSequence.Id
                                                                     && s.Feature != Feature.NonCodingSequence);
                        int nonCodingCount = db.Subsequences.Count(s => s.SequenceId == parentSequence.Id
                                                                    && s.Feature == Feature.NonCodingSequence);

                        importResult.Status = "Success";
                        importResult.Result = $"Successfully imported {featuresCount} features and {nonCodingCount} non coding subsequences";
                        importResults.Add(importResult);
                    }
                    catch (Exception exception)
                    {
                        importResult.Status = "Error";
                        importResult.Result = exception.Message;
                        while (exception.InnerException != null)
                        {
                            importResult.Result += $" {exception.InnerException.Message}";

                            exception = exception.InnerException;
                        }
                        importResults.Add(importResult);
                    }
                }

                var result = new Dictionary<string, object> { { "result", importResults } };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
    }
}
