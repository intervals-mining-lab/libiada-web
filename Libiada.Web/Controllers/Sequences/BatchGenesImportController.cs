namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Helpers;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Sequences;
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
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly INcbiHelper ncbiHelper;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchGenesImportController"/> class.
    /// </summary>
    public BatchGenesImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                      IViewDataBuilder viewDataBuilder,
                                      ITaskManager taskManager,
                                      INcbiHelper ncbiHelper,
                                      IResearchObjectsCache cache)
        : base(TaskType.BatchGenesImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
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

        long[] researchObjectIds = db.CombinedSequenceEntities.Include(c => c.ResearchObject)
                                     .Where(c => !string.IsNullOrEmpty(c.RemoteId)
                                              && !sequencesWithSubsequencesIds.Contains(c.Id)
                                              && StaticCollections.SequenceTypesWithSubsequences.Contains(c.ResearchObject.SequenceType))
                                     .Select(c => c.ResearchObjectId)
                                     .ToArray();

        var data = viewDataBuilder.AddMinMaxResearchObjects()
                                  .SetNature(Nature.Genetic)
                                  .AddSequenceTypes(onlyGenetic: true)
                                  .AddGroups(onlyGenetic: true)
                                  .Build();
        ViewBag.data = JsonConvert.SerializeObject(data);

        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The research objects ids.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(long[] researchObjectIds)
    {
        return CreateTask(() =>
        {
            using var db = dbFactory.CreateDbContext();
            string[] researchObjectNames;
            List<ResearchObjectImportResult> importResults = new(researchObjectIds.Length);

            researchObjectNames = cache.ResearchObjects
                                .Where(m => researchObjectIds.Contains(m.Id))
                                .OrderBy(m => m.Id)
                                .Select(m => m.Name)
                                .ToArray();
            GeneticSequence[] parentSequences = db.CombinedSequenceEntities
                                                .Where(s => researchObjectIds.Contains(s.ResearchObjectId))
                                                .OrderBy(s => s.ResearchObjectId)
                                                .Select(s => s.ToGeneticSequence())
                                                .ToArray();

            for (int i = 0; i < parentSequences.Length; i++)
            {
                var importResult = new ResearchObjectImportResult()
                {
                    ResearchObjectName = researchObjectNames[i]
                };

                try
                {
                    GeneticSequence parentSequence = parentSequences[i];
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
