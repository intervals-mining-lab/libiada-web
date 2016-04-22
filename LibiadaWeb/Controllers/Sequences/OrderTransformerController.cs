namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The order transformation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformerController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformerController"/> class.
        /// </summary>
        public OrderTransformerController() : base("Order transformation")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillViewData(1, 1, "Transform");

            var transformationLinks = new[] { Link.Start, Link.End, Link.CycleStart, Link.CycleEnd };
            transformationLinks = transformationLinks.OrderBy(n => (int)n).ToArray();
            data.Add("transformationLinks", transformationLinks.ToSelectList());
            
            var operations = new List<SelectListItem> { new SelectListItem { Text = "Dissimilar", Value = 1.ToString() }, new SelectListItem { Text = "Higher order", Value = 2.ToString() } };
            data.Add("operations", operations);
            
            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="transformationLinkIds">
        /// The transformation link ids.
        /// </param>
        /// <param name="transformationIds">
        /// The transformation ids.
        /// </param>
        /// <param name="iterationsCount">
        /// Number of transformations iterations.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId, int[] transformationLinkIds, int[] transformationIds, int iterationsCount)
        {
            return Action(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                var sequence = commonSequenceRepository.ToLibiadaChain(sequenceId);
                for (int j = 0; j < iterationsCount; j++)
                {
                    for (int i = 0; i < transformationIds.Length; i++)
                    {
                        if (transformationIds[i] == 1)
                        {
                            sequence = DissimilarChainFactory.Create(sequence);
                        }
                        else
                        {
                            sequence = HighOrderFactory.Create(sequence, (LibiadaCore.Core.Link)transformationLinkIds[i]);
                        }
                    }
                }

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationIds.Length; i++)
                {
                    var link = ((LibiadaCore.Core.Link)transformationLinkIds[i]).GetDisplayValue();
                    transformations.Add(i, transformationIds[i] == 1 ? "dissimilar" : "higher order " + link);
                }

                var result = new Dictionary<string, object>
                {
                    {"chain", sequence},
                    {"transformationsList", transformations},
                    {"iterationsCount", iterationsCount}
                };
                return result;
            });
        }
    }
}
