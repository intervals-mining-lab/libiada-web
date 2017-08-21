namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using Attribute = LibiadaWeb.Attribute;

    /// <summary>
    /// The attribute check controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AttributesCheckController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributesCheckController"/> class.
        /// </summary>
        public AttributesCheckController() : base(TaskType.AttributesCheck)
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
                var sequencesWithSubsequencesIds = db.Subsequence.Select(s => s.SequenceId).Distinct().ToArray();
                // TODO: Store this list separately
                var matterIds = db.DnaSequence.Include(c => c.Matter).Where(c => !string.IsNullOrEmpty(c.RemoteId) &&
                                                          !sequencesWithSubsequencesIds.Contains(c.Id) &&
                                                          (c.Matter.SequenceType == SequenceType.CompleteGenome
                                                        || c.Matter.SequenceType == SequenceType.MitochondrionGenome
                                                        || c.Matter.SequenceType == SequenceType.Plasmid))
                                                          .Select(c => c.MatterId).ToArray();

                var viewDataHelper = new ViewDataHelper(db);
                var data = viewDataHelper.FillViewData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Check");
                data.Add("nature", (byte)Nature.Genetic);
                ViewBag.data = JsonConvert.SerializeObject(data);
                return View();
            }
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
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
        public ActionResult Index(long[] matterIds)
        {
            return CreateTask(() =>
                {
                    string[] matterNames;
                    var attributes = new List<string>();
                    string[] databaseAttributes;
                    string[] parentRemoteIds;
                    using (var db = new LibiadaWebEntities())
                    {
                        databaseAttributes = ArrayExtensions.ToArray<Attribute>().Select(a => a.GetDisplayValue()).ToArray();
                        matterNames = db.Matter.Where(m => matterIds.Contains(m.Id)).OrderBy(m => m.Id).Select(m => m.Name).ToArray();
                        parentRemoteIds = db.DnaSequence.Where(c => matterIds.Contains(c.MatterId)).OrderBy(c => c.MatterId).Select(c => c.RemoteId).ToArray();
                    }

                    var features = NcbiHelper.GetFeatures(parentRemoteIds);

                    for (int i = 0; i < matterIds.Length; i++)
                    {
                        for (int j = 1; j < features[i].Count; j++)
                        {
                            var featureAttributes = features[i][j].Qualifiers;
                            attributes.AddRange(featureAttributes.Select(attribute => attribute.Key));
                        }
                    }

                    attributes = attributes.Distinct().Where(a => !databaseAttributes.Contains(a)).ToList();

                    return new Dictionary<string, object>
                                     {
                                         { "attributes", attributes },
                                         { "matterNames", matterNames }
                                     };
                });
        }
    }
}
