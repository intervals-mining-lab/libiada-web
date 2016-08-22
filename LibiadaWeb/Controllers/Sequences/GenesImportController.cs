namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;

    using Newtonsoft.Json;

    /// <summary>
    /// The genes import controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class GenesImportController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenesImportController"/> class.
        /// </summary>
        public GenesImportController()
            : base("Genes import")
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
            var data = viewDataHelper.FillMattersData(1, 1, m => matterIds.Contains(m.Id), "Import");
            data.Add("nature", (byte)Nature.Genetic);
            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if unknown feature or attribute is found.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId)
        {
            return Action(() =>
            {
                string matterName;
                Subsequence[] sequenceSubsequences;
                using (var db = new LibiadaWebEntities())
                {
                    long parentSequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                    DnaSequence parentSequence = db.DnaSequence.Single(c => c.Id == parentSequenceId);

                    var features = NcbiHelper.GetFeatures(parentSequence.RemoteId);
                    using (var subsequenceImporter = new SubsequenceImporter(features, parentSequenceId))
                    {
                        subsequenceImporter.CreateSubsequences();
                    }

                    matterName = db.Matter.Single(m => m.Id == matterId).Name;
                    sequenceSubsequences = db.Subsequence.Where(s => s.SequenceId == parentSequenceId)
                                                         .Include(s => s.Position)
                                                         .Include(s => s.Feature)
                                                         .Include(s => s.SequenceAttribute)
                                                         .ToArray();
                }

                return new Dictionary<string, object>
                                     {
                                         { "matterName", matterName }, 
                                         { "genes", sequenceSubsequences }
                                     };
            });
        }
    }
}
