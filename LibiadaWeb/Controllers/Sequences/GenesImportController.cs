namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.CalculatorsData;
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
                var data = viewDataHelper.FillViewData(1, 1, m => matterIds.Contains(m.Id), "Import");
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
                    using (var subsequenceImporter = new SubsequenceImporter(parentSequence))
                    {
                        subsequenceImporter.CreateSubsequences();
                    }

                    Dictionary<byte, string> features = ArrayExtensions.ToArray<Feature>()
                        .ToDictionary(f => (byte)f, f => f.GetDisplayValue());
                    string matterName = db.Matter.Single(m => m.Id == matterId).Name;
                    SubsequenceData[] sequenceSubsequences = db.Subsequence
                        .Where(s => s.SequenceId == parentSequence.Id)
                        .Include(s => s.Position)
                        .ToArray()
                        .Select(s => new SubsequenceData(s))
                        .ToArray();

                    result = new Dictionary<string, object>
                                 {
                                     { "matterName", matterName },
                                     { "genes", sequenceSubsequences },
                                     { "features", features }
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
