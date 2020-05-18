﻿namespace LibiadaWeb.Controllers.Sequences
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

    [Authorize(Roles = "Admin")]
    public class BatchPoemsImportController : AbstractResultController
    {
        public BatchPoemsImportController() : base(TaskType.BatchPoemsImport)
        {
        }

        // GET: BatchPoemsImport
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
        [ValidateAntiForgeryToken]
        public ActionResult Index(Notation notation, bool dropPunctuation)
        {
            return CreateTask(() =>
            {
                var importResults = new List<MatterImportResult>();

                using (var db = new LibiadaWebEntities())
                {
                    Matter[] matters = Cache.GetInstance().Matters.Where(m => m.Nature == Nature.Literature).ToArray();

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        string sequenceName = Request.Files[i].FileName.Substring(0, Request.Files[i].FileName.LastIndexOf('.'));

                        var importResult = new MatterImportResult()
                                               {
                                                   MatterName = sequenceName
                                               };


                        try
                        {
                            var sequence = new CommonSequence
                            {
                                Notation = notation
                            };


                            if (matters.Any(m => m.Name == sequenceName))
                            {
                                sequence.MatterId = matters.Single(m => m.Name == sequenceName).Id;
                                importResult.Result = "Successfully imported poem for existing matter";
                            }
                            else
                            {
                                sequence.Matter = new Matter
                                {
                                    Name = sequenceName,
                                    Group = Group.ClassicalLiterature,
                                    Nature = Nature.Literature,
                                    SequenceType = SequenceType.CompleteText
                                };

                                importResult.Result = "Successfully imported poem and created matter";
                            }

                            var repository = new LiteratureSequenceRepository(db);

                            repository.Create(sequence, Request.Files[i].InputStream, Language.Russian, true, Translator.NoneOrManual, dropPunctuation);
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
