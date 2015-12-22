namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The order transformation controller.
    /// </summary>
    public class OrderTransformationController : AbstractResultController
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
        /// Initializes a new instance of the <see cref="OrderTransformationController"/> class.
        /// </summary>
        public OrderTransformationController() : base("OrderTransformation", "Order transformation")
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
            var matterIds = db.Matter.Select(d => d.Id).ToList(); 

            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillMattersData(1, int.MaxValue, false, m => matterIds.Contains(m.Id), "Transform");
            data.Add("links", new SelectList(db.Link.OrderBy(n => n.Id), "id", "name"));
            ViewBag.data = data;
            ViewBag.angularController = "SubsequencesCalculationController";
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(long matterId, int linkId)
        {
            return Action(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                Chain sourceChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                BaseChain transformedChain = HighOrderFactory.Create(sourceChain, (Link)linkId);
                var result = new Dictionary<string, object>
                {
                    { "Chain", transformedChain }
                };
                return result;
            });
        }
    }
}
