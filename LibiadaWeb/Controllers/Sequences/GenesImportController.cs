namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using LibiadaCore.Extensions;

    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models;
    using Libiada.Database.Tasks;

    using LibiadaWeb.Helpers;

    using Newtonsoft.Json;
    using LibiadaWeb.Tasks;



    /// <summary>
    /// The genes import controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class GenesImportController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesImportController"/> class.
        /// </summary>
        public GenesImportController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper, ITaskManager taskManager) : base(TaskType.GenesImport, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var genesSequenceIds = db.Subsequence.Select(s => s.SequenceId).Distinct();

            var matterIds = db.DnaSequence
                              .Include(c => c.Matter)
                              .Where(c => !string.IsNullOrEmpty(c.RemoteId)
                                       && !genesSequenceIds.Contains(c.Id)
                                       && Aliases.SequenceTypesWithSubsequences.Contains(c.Matter.SequenceType))
                              .Select(c => c.MatterId).ToList();

            var data = viewDataHelper.FillViewData(1, 1, m => matterIds.Contains(m.Id), "Import");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId)
        {
            return CreateTask(() =>
            {
                Dictionary<string, object> result;

                DnaSequence parentSequence = db.DnaSequence.Single(d => d.MatterId == matterId);
                using (var subsequenceImporter = new SubsequenceImporter(parentSequence))
                {
                    subsequenceImporter.CreateSubsequences();
                }

                var features = EnumExtensions.ToArray<Feature>().ToDictionary(f => (byte)f, f => f.GetDisplayValue());
                string matterName = Cache.GetInstance().Matters.Single(m => m.Id == matterId).Name;
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


                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
