namespace Libiada.Web.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Libiada.Web.Helpers;
    using Libiada.Database.Models;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Authorization;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The batch genes import controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class BatchGenesImportController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;
        private readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchGenesImportController"/> class.
        /// </summary>
        public BatchGenesImportController(LibiadaDatabaseEntities db, 
                                          IViewDataHelper viewDataHelper, 
                                          ITaskManager taskManager,
                                          Cache cache)
            : base(TaskType.BatchGenesImport, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
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
            var sequencesWithSubsequencesIds = db.Subsequence.Select(s => s.SequenceId).Distinct();

            var matterIds = db.DnaSequence.Include(c => c.Matter)
                .Where(c => !string.IsNullOrEmpty(c.RemoteId)
                         && !sequencesWithSubsequencesIds.Contains(c.Id)
                         && Aliases.SequenceTypesWithSubsequences.Contains(c.Matter.SequenceType))
                .Select(c => c.MatterId).ToArray();

            var data = viewDataHelper.FillViewData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Import");
            data.Add("nature", (byte)Nature.Genetic);
            ViewBag.data = JsonConvert.SerializeObject(data);

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds)
        {
            return CreateTask(() =>
                {
                    string[] matterNames;
                    var importResults = new List<MatterImportResult>(matterIds.Length);

                    matterNames = cache.Matters
                                       .Where(m => matterIds.Contains(m.Id))
                                       .OrderBy(m => m.Id)
                                       .Select(m => m.Name)
                                       .ToArray();
                    var parentSequences = db.DnaSequence
                                            .Where(c => matterIds.Contains(c.MatterId))
                                            .OrderBy(c => c.MatterId)
                                            .ToArray();

                    for (int i = 0; i < parentSequences.Length; i++)
                    {
                        var importResult = new MatterImportResult()
                        {
                            MatterName = matterNames[i]
                        };

                        try
                        {
                            DnaSequence parentSequence = parentSequences[i];
                            var subsequenceImporter = new SubsequenceImporter(db, parentSequence);

                            subsequenceImporter.CreateSubsequences();


                            int featuresCount = db.Subsequence.Count(s => s.SequenceId == parentSequence.Id
                                                                         && s.Feature != Feature.NonCodingSequence);
                            int nonCodingCount = db.Subsequence.Count(s => s.SequenceId == parentSequence.Id
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
}
