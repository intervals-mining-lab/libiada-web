namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The annotations check controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AnnotationsCheckController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The subsequence repository.
        /// </summary>
        private readonly SubsequenceRepository subsequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationsCheckController"/> class.
        /// </summary>
        public AnnotationsCheckController() : base("Subsequences annotations check")
        {
            db = new LibiadaWebEntities();
            subsequenceRepository = new SubsequenceRepository(db);
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
            var matterIds = db.DnaSequence.Where(c => genesSequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToArray();

            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillMattersData(1, 1, false, m => matterIds.Contains(m.Id), "Check");
            data.Add("nature", (byte)Nature.Genetic);
            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matters id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if there is no file with sequence.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if unknown part is found.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId)
        {
            return Action(() =>
                {
                    long sequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                    DnaSequence parentSequence = db.DnaSequence.Single(c => c.Id == sequenceId);

                    Stream stream = NcbiHelper.GetGenesFileStream(parentSequence.WebApiId.ToString());
                    var features = NcbiHelper.GetFeatures(stream);

                    var result = subsequenceRepository.CheckAnnotations(features, sequenceId);

                    var matterName = db.Matter.Single(m => m.Id == matterId).Name;

                    result.Add("matterName", matterName);
                    result.Add("features", features);
                    result.Add("sequenceId", sequenceId);

                    return result;
                });
        }
    }
}
