namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;

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
        public BatchGenesImportController()
            : base("Batch genes import")
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
                var matterIds = db.DnaSequence.Where(c => !string.IsNullOrEmpty(c.RemoteId) && !sequencesWithSubsequencesIds.Contains(c.Id)
                                && (c.FeatureId == Aliases.Feature.FullGenome
                                    || c.FeatureId == Aliases.Feature.MitochondrionGenome
                                    || c.FeatureId == Aliases.Feature.Plasmid)).Select(c => c.MatterId).ToArray();

                var viewDataHelper = new ViewDataHelper(db);
                var data = viewDataHelper.GetMattersData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Import");
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
            return Action(() =>
                {
                    string[] matterNames;
                    string[] results = new string[matterIds.Length];
                    string[] statuses = new string[matterIds.Length];
                    using (var db = new LibiadaWebEntities())
                    {
                        matterNames = db.Matter.Where(m => matterIds.Contains(m.Id)).OrderBy(m => m.Id).Select(m => m.Name).ToArray();
                        var parentSequences = db.DnaSequence.Where(c => matterIds.Contains(c.MatterId)).OrderBy(c => c.MatterId);
                        long[] parentSequenceIds = parentSequences.Select(c => c.Id).ToArray();
                        string[] parentRemoteIds = parentSequences.Select(c => c.RemoteId).ToArray();
                        var features = NcbiHelper.GetFeatures(parentRemoteIds);

                        for (int i = 0; i < matterIds.Length; i++)
                        {
                            try
                            {
                                var parentSequenceId = parentSequenceIds[i];
                                using (var subsequenceImporter = new SubsequenceImporter(features[i], parentSequenceId))
                                {
                                    subsequenceImporter.CreateSubsequences();
                                }

                                var nonCodingCount = db.Subsequence.Count(s => s.SequenceId == parentSequenceId && s.FeatureId == Aliases.Feature.NonCodingSequence);
                                var featuresCount = db.Subsequence.Count(s => s.SequenceId == parentSequenceId && s.FeatureId != Aliases.Feature.NonCodingSequence);

                                statuses[i] = "Success";
                                results[i] = "Successfully imported " + featuresCount + " features and " + nonCodingCount + " non coding subsequences";
                            }
                            catch (Exception exception)
                            {
                                statuses[i] = "Error";
                                results[i] = exception.Message;
                                if (exception.InnerException != null)
                                {
                                    results[i] += " " + exception.InnerException.Message;
                                }
                            }
                        }
                    }

                    return new Dictionary<string, object>
                           {
                               { "matterNames", matterNames },
                               { "results", results },
                               { "status", statuses }
                           };
                });
        }
    }
}
