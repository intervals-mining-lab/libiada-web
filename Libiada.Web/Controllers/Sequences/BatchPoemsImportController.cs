namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Extensions;

using Libiada.Web.Extensions;
using Libiada.Web.Tasks;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class BatchPoemsImportController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly Cache cache;

    public BatchPoemsImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, ITaskManager taskManager, Cache cache)
        : base(TaskType.BatchPoemsImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.cache = cache;
    }

    public ActionResult Index()
    {
        var viewData = new Dictionary<string, object>
        {
            { "notations", new [] { Notation.Letters, Notation.Consonance }.ToSelectListWithNature() }
        };
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    [HttpPost]
    public ActionResult Index(Notation notation, bool dropPunctuation, List<IFormFile> files)
    {
        var fileStreams = files.Select(Helpers.FileHelper.GetFileStream).ToList();

        return CreateTask(() =>
        {
            List<ResearchObjectImportResult> importResults = [];

            ResearchObject[] researchObjects = cache.ResearchObjects.Where(m => m.Nature == Nature.Literature).ToArray();

            for (int i = 0; i < files.Count; i++)
            {
                string sequenceName = files[i].FileName.Substring(0, files[i].FileName.LastIndexOf('.'));

                var importResult = new ResearchObjectImportResult()
                {
                    ResearchObjectName = sequenceName
                };

                try
                {
                    var sequence = new LiteratureSequence
                    {
                        Notation = notation,
                        Language = Language.Russian,
                        Original = true,
                        Translator = Translator.NoneOrManual
                    };


                    if (researchObjects.Any(m => m.Name == sequenceName))
                    {
                        ResearchObject researchObject = researchObjects.Single(m => m.Name == sequenceName);
                        sequence.ResearchObjectId = researchObject.Id;
                        importResult.ResearchObjectName = researchObject.Name;
                        importResult.SequenceType = researchObject.SequenceType.GetDisplayValue();
                        importResult.Group = researchObject.Group.GetDisplayValue();
                        importResult.Result = "Successfully imported poem for existing research object";
                    }
                    else
                    {
                        sequence.ResearchObject = new ResearchObject
                        {
                            Name = sequenceName,
                            Group = Group.ClassicalLiterature,
                            Nature = Nature.Literature,
                            SequenceType = SequenceType.CompleteText
                        };

                        importResult.ResearchObjectName = sequence.ResearchObject.Name;
                        importResult.SequenceType = sequence.ResearchObject.SequenceType.GetDisplayValue();
                        importResult.Group = sequence.ResearchObject.Group.GetDisplayValue();
                        importResult.Result = "Successfully imported poem and created research object";
                    }

                    var repository = new LiteratureSequenceRepository(dbFactory, cache);

                    repository.Create(sequence, fileStreams[i], dropPunctuation);
                    importResult.Status = "Success";

                    importResults.Add(importResult);
                }
                catch (Exception exception)
                {
                    importResult.Result = $"Failed to import poem: {exception.Message}";
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
