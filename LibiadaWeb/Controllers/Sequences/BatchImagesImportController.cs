namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using System.IO;
    using System.Web;

    [Authorize(Roles = "Admin")]
    public class BatchImagesImportController : AbstractResultController
    {
        public BatchImagesImportController() : base(TaskType.BatchImagesImport)
        {
        }

        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject("");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HttpPostedFileBase[] files)
        {
            return CreateTask(() =>
            {
                var importResults = new List<MatterImportResult>();

                using (var db = new LibiadaWebEntities())
                {
                    Matter[] matters = db.Matter.Where(m => m.Nature == Nature.Image).ToArray();
                    var matterRepository = new MatterRepository(db);

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        string sequenceName = file?.FileName.Substring(0, file.FileName.LastIndexOf('.'));

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
                            int fileSize = file.ContentLength;
                            var fileBytes = new byte[fileSize];
                            file.InputStream.Read(fileBytes, 0, fileSize);

                            var matter = new Matter
                            {
                                Name = sequenceName,
                                Group = Group.Picture,
                                Nature = Nature.Image,
                                Source = fileBytes,
                                SequenceType = SequenceType.CompleteImage
                            };

                            matterRepository.CreateMatter(matter);
                            importResult.Result = "Successfully imported image and created matter";
                            importResult.Status = "Success";
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

                    var data = new Dictionary<string, object> { { "result", importResults } };

                    return new Dictionary<string, object>
                    {
                        {"data", JsonConvert.SerializeObject(data)}
                    };
                }
            });
        }
    }
}