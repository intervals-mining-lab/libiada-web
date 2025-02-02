namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Core.Extensions;

using Libiada.Web.Tasks;

using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class BatchImagesImportController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly ResearchObjectsCache cache;

    public BatchImagesImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, ITaskManager taskManager, ResearchObjectsCache cache)
        : base(TaskType.BatchImagesImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.cache = cache;
    }

    public ActionResult Index()
    {
        ViewBag.data = JsonConvert.SerializeObject("");
        return View();
    }

    [HttpPost]
    public ActionResult Index(List<IFormFile> files)
    {
        var fileStreams = files.Select(Helpers.FileHelper.GetFileStream).ToList();
        return CreateTask(() =>
        {
            using var db = dbFactory.CreateDbContext();
            List<ResearchObjectImportResult> importResults = [];

            ResearchObject[] researchObjects = db.ResearchObjects.Where(m => m.Nature == Nature.Image).ToArray();
            var researchObjectRepository = new ResearchObjectRepository(db, cache);

            for (int i = 0; i < files.Count; i++)
            {
                IFormFile file = files[i];
                string sequenceName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));

                var importResult = new ResearchObjectImportResult()
                {
                    ResearchObjectName = sequenceName
                };

                try
                {
                    if (file == null)
                    {
                        throw new FileNotFoundException($"No image file is provided. Iteration: {i}");
                    }

                    if (researchObjects.Any(m => m.Name == sequenceName))
                    {
                        importResult.Result = "Image already exists";
                        continue;
                    }

                    using Stream sequenceStream = fileStreams[i];
                    byte[] fileBytes = new byte[sequenceStream.Length];
                    sequenceStream.Read(fileBytes, 0, (int)sequenceStream.Length);

                    var researchObject = new ResearchObject
                    {
                        Name = sequenceName,
                        Group = Group.Picture,
                        Nature = Nature.Image,
                        Source = fileBytes,
                        SequenceType = SequenceType.CompleteImage
                    };

                    researchObjectRepository.SaveToDatabase(researchObject);
                    importResult.Result = "Successfully imported image and created research object";
                    importResult.Status = "Success";
                    importResult.SequenceType = researchObject.SequenceType.GetDisplayValue();
                    importResult.Group = researchObject.Group.GetDisplayValue();
                    importResults.Add(importResult);
                }
                catch (Exception exception)
                {
                    importResult.Result = $"Failed to import image: {exception.Message}";
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
