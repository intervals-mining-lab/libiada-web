namespace LibiadaWeb.Controllers.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;

    /// <summary>
    /// The attribute check controller.
    /// </summary>
    public class AttributesCheckController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributesCheckController"/> class.
        /// </summary>
        public AttributesCheckController() : base("AttributesCheck", "Attributes check")
        {
            db = new LibiadaWebEntities();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var subsequencesSequenceIds = db.Subsequence.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => c.WebApiId != null && !subsequencesSequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToList();

            var calculatorsHelper = new ViewDataHelper(db);

            var data = calculatorsHelper.FillMattersData(1, 1, false, m => matterIds.Contains(m.Id));

            data.Add("natureId", Aliases.Nature.Genetic);

            ViewBag.data = data;

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
        public ActionResult Index(long matterId, bool localFile)
        {
            return Action(() =>
                {
                    long sequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                    DnaSequence parentSequence = db.DnaSequence.Single(c => c.Id == sequenceId);

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

                    var attributes = new List<string>();

                    for (int i = 1; i < features.Count; i++)
                    {
                        var featureAttributes = features[i].Qualifiers;
                        attributes.AddRange(featureAttributes.Select(attribute => attribute.Key));
                    }

                    var databaseAttributes = db.Attribute.Select(a => a.Name).ToList();

                    attributes = attributes.Distinct().Where(a => !databaseAttributes.Contains(a)).ToList();

                    var matterName = db.Matter.Single(m => m.Id == matterId).Name;

                    return new Dictionary<string, object>
                                     {
                                         { "attributes", attributes }, 
                                         { "matterName", matterName }
                                     };
                });
        }
    }
}