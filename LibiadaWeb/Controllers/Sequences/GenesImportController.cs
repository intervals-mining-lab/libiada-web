namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;

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
        public GenesImportController() : base("Genes Import")
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
            var matterIds = db.DnaSequence.Where(c => c.WebApiId != null &&
                                                          !genesSequenceIds.Contains(c.Id) &&
                                                          (c.FeatureId == Aliases.Feature.FullGenome ||
                                                           c.FeatureId == Aliases.Feature.MitochondrionGenome ||
                                                           c.FeatureId == Aliases.Feature.Plasmid)).Select(c => c.MatterId).ToList();

            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillMattersData(1, 1, false, m => matterIds.Contains(m.Id), "Import");
            data.Add("natureId", (byte)Nature.Genetic);
            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="localFile">
        /// The local file.
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
        public ActionResult Index(long matterId, bool localFile)
        {
            return Action(() =>
                {
                    string matterName;
                    Subsequence[] sequenceSubsequences;
                    using (var db = new LibiadaWebEntities())
                    {
                        var subsequenceRepository = new SubsequenceRepository(db);
                        long parentSequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                        DnaSequence parentSequence = db.DnaSequence.Single(c => c.Id == parentSequenceId);

                        Stream stream;
                        if (localFile)
                        {
                            HttpPostedFileBase file = Request.Files[0];

                            if (file == null || file.ContentLength == 0)
                            {
                                throw new ArgumentNullException("file", "Sequence file is empty");
                            }

                            stream = file.InputStream;
                        }
                        else
                        {
                            stream = NcbiHelper.GetGenesFileStream(parentSequence.WebApiId.ToString());
                        }

                        var features = NcbiHelper.GetFeatures(stream);

                        subsequenceRepository.CreateSubsequences(features, parentSequenceId);

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
