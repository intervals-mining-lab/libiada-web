namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Core.Extensions;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;
using Libiada.Web.Extensions;

[Authorize(Roles = "Admin")]
public class BatchMusicImportController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ResearchObjectsCache cache;

    /// <summary>
    /// The batch music import controller.
    /// </summary>
    public BatchMusicImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, ITaskManager taskManager, ResearchObjectsCache cache)
        : base(TaskType.BatchMusicImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.cache = cache;
    }

    // GET: BatchMusicImport
    public ActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Imports sequences from uploaded MusicXML files.
    /// </summary>
    /// <param name="files">
    /// Uploaded MusicXML files.</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Index(List<IFormFile> files)
    {
        // TODO: use temp files instead of this
        var fileStreams = files.Select(FileHelper.GetFileStream).ToList();

        return CreateTask(() =>
        {
            List<ResearchObjectImportResult> importResults = [];

            ResearchObject[] researchObjects = cache.ResearchObjects.Where(m => m.Nature == Nature.Music).ToArray();

            for (int i = 0; i < files.Count; i++)
            {
                string sequenceName = files[i].FileName.Substring(0, files[i].FileName.LastIndexOf('.'));

                var importResult = new ResearchObjectImportResult()
                {
                    ResearchObjectName = sequenceName
                };

                try
                {
                    var sequence = new MusicSequence()
                    {
                        CreatorId = User.GetUserId(),
                        ModifierId = User.GetUserId()
                    };

                    if (researchObjects.Any(m => m.Name == sequenceName))
                    {
                        ResearchObject researchObject = researchObjects.Single(m => m.Name == sequenceName);
                        sequence.ResearchObjectId = researchObject.Id;
                        importResult.ResearchObjectName = researchObject.Name;
                        importResult.SequenceType = researchObject.SequenceType.GetDisplayValue();
                        importResult.Group = researchObject.Group.GetDisplayValue();
                        importResult.Result = "Successfully imported music for existing research object";
                    }
                    else
                    {
                        sequence.ResearchObject = new ResearchObject
                        {
                            Name = sequenceName,
                            Group = Group.ClassicalMusic,
                            Nature = Nature.Music,
                            SequenceType = SequenceType.CompleteMusicalComposition
                        };

                        importResult.ResearchObjectName = sequence.ResearchObject.Name;
                        importResult.SequenceType = sequence.ResearchObject.SequenceType.GetDisplayValue();
                        importResult.Group = sequence.ResearchObject.Group.GetDisplayValue();
                        importResult.Result = "Successfully imported music and created research object";
                    }

                    var repository = new MusicSequenceRepository(dbFactory, cache);
                    repository.Create(sequence, fileStreams[i]);
                    importResult.Status = "Success";
                    importResults.Add(importResult);
                }
                catch (Exception exception)
                {
                    importResult.Result = $"Failed to import music: {exception.Message}";
                    while (exception.InnerException != null)
                    {
                        importResult.Result += $" {exception.InnerException.Message}";

                        exception = exception.InnerException;
                    }

                    importResult.Status = "Error";
                    importResults.Add(importResult);
                }
            }

            var result = new Dictionary<string, object> { { "result", importResults } };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };

        });
    }
}
