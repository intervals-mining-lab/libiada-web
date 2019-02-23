namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using LibiadaWeb.Extensions;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;
    using Newtonsoft.Json;
    using Microsoft.Ajax.Utilities;
    using LibiadaCore.Music;

    public class BatchMusicImportController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchMusicImportController"/> class.
        /// </summary>
        public BatchMusicImportController() : base(TaskType.BatchMusicImport)
        {
        }

        // GET: BatchMusicImport
        public ActionResult Index()
        {
            var viewData = new Dictionary<string, object>
            {
                { "notations", new [] { Notation.Notes, Notation.Measures, Notation.FormalMotifs }.ToSelectListWithNature() },
                { "pauseParams", new [] { ParamPauseTreatment.Ignore, ParamPauseTreatment.NoteTrace, ParamPauseTreatment.SilenceNote} }
            };

            ViewBag.data = JsonConvert.SerializeObject(viewData);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Notation notation)
        {
            return CreateTask(() =>
            {
                var importResults = new List<MatterImportResult>();

                using (var db = new LibiadaWebEntities())
                {
                    Matter[] matters = db.Matter.Where(m => m.Nature == Nature.Music).ToArray();

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        string sequenceName = Request.Files[i].FileName.Substring(0, Request.Files[i].FileName.LastIndexOf('.'));

                        var importResult = new MatterImportResult()
                        {
                            MatterName = sequenceName
                        };


                        try
                        {
                            CommonSequence sequence = new CommonSequence
                            {
                                Notation = notation
                            };


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

                            MusicSequenceRepository repository = new MusicSequenceRepository(db);

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
