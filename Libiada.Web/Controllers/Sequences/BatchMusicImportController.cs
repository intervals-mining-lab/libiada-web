namespace Libiada.Web.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using LibiadaCore.Extensions;
    using Libiada.Web.Helpers;
    using Libiada.Web.Tasks;

    [Authorize(Roles = "Admin")]
    public class BatchMusicImportController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly Cache cache;

        /// <summary>
        /// The batch music import controller.
        /// </summary>
        public BatchMusicImportController(LibiadaDatabaseEntities db, ITaskManager taskManager, Cache cache) : base(TaskType.BatchMusicImport, taskManager)
        {
            this.db = db;
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
        [ValidateAntiForgeryToken]
        public ActionResult Index(IFormFileCollection files)
        {
            return CreateTask(() =>
            {
                var importResults = new List<MatterImportResult>();

                Matter[] matters = cache.Matters.Where(m => m.Nature == Nature.Music).ToArray();

                for (int i = 0; i < files.Count; i++)
                {
                    string sequenceName = files[i].FileName.Substring(0, files[i].FileName.LastIndexOf('.'));

                    var importResult = new MatterImportResult()
                    {
                        MatterName = sequenceName
                    };

                    try
                    {
                        var sequence = new CommonSequence();

                        if (matters.Any(m => m.Name == sequenceName))
                        {
                            var matter = matters.Single(m => m.Name == sequenceName);
                            sequence.MatterId = matter.Id;
                            importResult.MatterName = matter.Name;
                            importResult.SequenceType = matter.SequenceType.GetDisplayValue();
                            importResult.Group = matter.Group.GetDisplayValue();
                            importResult.Result = "Successfully imported music for existing matter";
                        }
                        else
                        {
                            sequence.Matter = new Matter
                            {
                                Name = sequenceName,
                                Group = Group.ClassicalMusic,
                                Nature = Nature.Music,
                                SequenceType = SequenceType.CompleteMusicalComposition
                            };

                            importResult.MatterName = sequence.Matter.Name;
                            importResult.SequenceType = sequence.Matter.SequenceType.GetDisplayValue();
                            importResult.Group = sequence.Matter.Group.GetDisplayValue();
                            importResult.Result = "Successfully imported music and created matter";
                        }

                        var repository = new MusicSequenceRepository(db, cache);

                        repository.Create(sequence, FileHelper.GetFileStream(files[i]));
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
}
