namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Tasks;

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
        public GenesImportController() : base(TaskType.GenesImport)
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
                var genesSequenceIds = db.Subsequence.Select(s => s.SequenceId).Distinct();

                // TODO: extract list of applicable SequenceTypes into separate entity
                var matterIds = db.DnaSequence
                                  .Include(c => c.Matter)
                                  .Where(c => !string.IsNullOrEmpty(c.RemoteId) &&
                                                                                 !genesSequenceIds.Contains(c.Id) &&
                                                                                 (c.Matter.SequenceType == SequenceType.CompleteGenome
                                                                               || c.Matter.SequenceType == SequenceType.MitochondrionGenome
                                                                               || c.Matter.SequenceType == SequenceType.Plasmid))
                                  .Select(c => c.MatterId).ToList();

                var viewDataHelper = new ViewDataHelper(db);
                var data = viewDataHelper.GetMattersData(1, 1, m => matterIds.Contains(m.Id), "Import");
                data.Add("nature", (byte)Nature.Genetic);
                ViewBag.data = JsonConvert.SerializeObject(data);
                return View();
            }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId)
        {
            return Action(() =>
            {
                Dictionary<string, object> result;

                using (var db = new LibiadaWebEntities())
                {
                    DnaSequence parentSequence = db.DnaSequence.Single(d => d.MatterId == matterId);
                    var features = NcbiHelper.GetFeatures(parentSequence.RemoteId);
                    using (var subsequenceImporter = new SubsequenceImporter(features, parentSequence.Id))
                    {
                        subsequenceImporter.CreateSubsequences();
                    }

                    string matterName = db.Matter.Single(m => m.Id == matterId).Name;
                    Subsequence[] sequenceSubsequences = db.Subsequence
                        .Where(s => s.SequenceId == parentSequence.Id)
                        .Include(s => s.Position)
                        .Include(s => s.SequenceAttribute)
                        .ToArray();

                    result = new Dictionary<string, object>
                                 {
                                     { "matterName", matterName },
                                     { "genes", sequenceSubsequences }
                                 };
                }

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
