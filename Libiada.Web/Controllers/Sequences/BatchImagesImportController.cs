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
    private readonly Cache cache;

    public BatchImagesImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, ITaskManager taskManager, Cache cache) 
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
    [ValidateAntiForgeryToken]
    public ActionResult Index(List<IFormFile> files)
    {
        var fileStreams = files.Select(Helpers.FileHelper.GetFileStream).ToList();
        return CreateTask(() =>
        {
            using var db = dbFactory.CreateDbContext();
            List<MatterImportResult> importResults = [];

            Matter[] matters = db.Matters.Where(m => m.Nature == Nature.Image).ToArray();
            var matterRepository = new MatterRepository(db, cache);

            for (int i = 0; i < files.Count; i++)
            {
                IFormFile file = files[i];
                string sequenceName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));

                var importResult = new MatterImportResult()
                {
                    MatterName = sequenceName
                };

                try
                {
                    if (file == null)
                    {
                        throw new FileNotFoundException($"No image file is provided. Iteration: {i}");
                    }

                    if (matters.Any(m => m.Name == sequenceName))
                    {
                        importResult.Result = "Image already exists";
                        continue;
                    }

                    using Stream sequenceStream = fileStreams[i];
                    byte[] fileBytes = new byte[sequenceStream.Length];
                    sequenceStream.Read(fileBytes, 0, (int)sequenceStream.Length);

                    var matter = new Matter
                    {
                        Name = sequenceName,
                        Group = Group.Picture,
                        Nature = Nature.Image,
                        Source = fileBytes,
                        SequenceType = SequenceType.CompleteImage
                    };

                    matterRepository.SaveToDatabase(matter);
                    importResult.Result = "Successfully imported image and created matter";
                    importResult.Status = "Success";
                    importResult.SequenceType = matter.SequenceType.GetDisplayValue();
                    importResult.Group = matter.Group.GetDisplayValue();
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
