namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The batch genes import controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class BatchGenesImportController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchGenesImportController"/> class.
        /// </summary>
        public BatchGenesImportController() : base(TaskType.BatchGenesImport)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                var sequencesWithSubsequencesIds = db.Subsequence.Select(s => s.SequenceId).Distinct();

                // TODO: Move list of sequenceTypes into separate entity
                var matterIds = db.DnaSequence.Include(c => c.Matter)
                    .Where(c => !string.IsNullOrEmpty(c.RemoteId) && !sequencesWithSubsequencesIds.Contains(c.Id)
                                && (c.Matter.SequenceType == SequenceType.CompleteGenome
                                    || c.Matter.SequenceType == SequenceType.MitochondrionGenome
                                    || c.Matter.SequenceType == SequenceType.Plasmid)).Select(c => c.MatterId).ToArray();

                var viewDataHelper = new ViewDataHelper(db);
                var data = viewDataHelper.FillViewData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Import");
                data.Add("nature", (byte)Nature.Genetic);
                ViewBag.data = JsonConvert.SerializeObject(data);
            }

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

                    using (var db = new LibiadaWebEntities())
                    {
                        matterNames = Cache.GetInstance().Matters
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
                                using (var subsequenceImporter = new SubsequenceImporter(parentSequence))
                                {
                                    subsequenceImporter.CreateSubsequences();
                                }

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
                    }

                    var data = new Dictionary<string, object> { { "result", importResults } };

                    return new Dictionary<string, object>
                    {
                        {"data", JsonConvert.SerializeObject(data)}
                    };
                });
        }
    }
}
