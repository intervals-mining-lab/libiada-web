﻿namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;
    using Newtonsoft.Json;

    using System.Web;

    [Authorize(Roles = "Admin")]
    public class BatchMusicImportController : AbstractResultController
    {
        /// <summary>
        /// The batch music import controller.
        /// </summary>
        public BatchMusicImportController() : base(TaskType.BatchMusicImport)
        {
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
        public ActionResult Index(HttpPostedFileBase[] files)
        {
            return CreateTask(() =>
            {
                var importResults = new List<MatterImportResult>();

                using (var db = new LibiadaWebEntities())
                {
                    Matter[] matters = Cache.GetInstance().Matters.Where(m => m.Nature == Nature.Music).ToArray();

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        string sequenceName = Request.Files[i].FileName.Substring(0, Request.Files[i].FileName.LastIndexOf('.'));

                        var importResult = new MatterImportResult()
                        {
                            MatterName = sequenceName
                        };

                        try
                        {
                            var sequence = new CommonSequence();

                            if (matters.Any(m => m.Name == sequenceName))
                            {
                                sequence.MatterId = matters.Single(m => m.Name == sequenceName).Id;
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

                                importResult.Result = "Successfully imported music and created matter";
                            }

                            var repository = new MusicSequenceRepository(db);

                            repository.Create(sequence, Request.Files[i].InputStream);
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
