namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Bio.IO.GenBank;

    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The annotation update controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AnnotationsUpdateController : AbstractResultController
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
        /// Initializes a new instance of the <see cref="AnnotationsUpdateController"/> class.
        /// </summary>
        public AnnotationsUpdateController() : base("AnnotationsUpdate", "Annotations update")
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return Action(() =>
            {
                var tempData = TempData["result"] as Dictionary<string, object>;
                if (tempData == null)
                {
                    throw new Exception("No data.");
                }

                long sequenceId = (long)tempData["sequenceId"];

                var features = tempData["features"] as List<FeatureItem>;

                if (features == null)
                {
                    throw new Exception("No annotation data.");
                }

                subsequenceRepository.UpdateAnnotations(features, sequenceId);

                return new Dictionary<string, object>();
            });
        }
    }
}
