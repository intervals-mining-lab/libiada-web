namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;

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
        public BatchGenesImportController() : base("Batch genes import")
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
            var db = new LibiadaWebEntities();
            var genesSequenceIds = db.Subsequence.Select(s => s.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => !string.IsNullOrEmpty(c.RemoteId) &&
                                                          !genesSequenceIds.Contains(c.Id) &&
                                                          (c.FeatureId == Aliases.Feature.FullGenome ||
                                                           c.FeatureId == Aliases.Feature.MitochondrionGenome ||
                                                           c.FeatureId == Aliases.Feature.Plasmid)).Select(c => c.MatterId).ToList();

            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillMattersData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Import");
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
            return Action(() =>
            {
                var matterNames = new string[matterIds.Length];
                var results = new string[matterIds.Length];

                for (int i = 0; i < matterIds.Length; i++)
                {
                    using (var db = new LibiadaWebEntities())
                    {
                        var matterId = matterIds[i];
                        matterNames[i] = db.Matter.Single(m => m.Id == matterId).Name;
                        try
                        {
                            var subsequenceRepository = new SubsequenceRepository(db);
                            long parentSequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                            string parentRemoteId =
                                db.DnaSequence.Single(c => c.Id == parentSequenceId).RemoteId;

                            Stream stream = NcbiHelper.GetGenesFileStream(parentRemoteId);
                            var features = NcbiHelper.GetFeatures(stream);

                            subsequenceRepository.CreateSubsequences(features, parentSequenceId);

                            results[i] = "Successfully imported "
                                            + db.Subsequence.Where(s => s.SequenceId == parentSequenceId)
                                                .Include(s => s.Position)
                                                .Include(s => s.Feature)
                                                .Include(s => s.SequenceAttribute)
                                                .Count() + " subsequences";
                        }
                        catch (Exception exception)
                        {
                            results[i] = exception.Message;
                            if (exception.InnerException != null)
                            {
                                results[i] += " " + exception.InnerException.Message;
                            }
                        }
                    }
                }

                return new Dictionary<string, object> { { "matterNames", matterNames }, { "results", results } };
            });
        }
    }
}