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
        public OrderTransformerController() : base("OrderTransformer", "Order transformation")
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
            var linksFilter = new List<int> { (int)Link.Start, (int)Link.End, (int)Link.CycleStart, (int)Link.CycleEnd };
            data.Add("links", new SelectList(db.Link.Where(l => linksFilter.Contains(l.Id)).OrderBy(n => n.Id), "id", "name"));
            ViewBag.data = JsonConvert.SerializeObject(data);
            ViewBag.angularController = "OrderTransformerController";
            return View();
        }

       
        [HttpPost]
        public ActionResult Index(long matterId, int[] linkIds, string[] transformations)
        {
            return Action(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                var sequence = commonSequenceRepository.ToLibiadaChain(sequenceId);
                for (int i = 0; i < transformations.Length; i++)
                {
                    if (transformations[i] == "Dissimilar")
                    {
                        sequence = DissimilarChainFactory.Create(sequence);
                    }
                    else
                    {
                        sequence = HighOrderFactory.Create(sequence, (Link)linkIds[i]);
                    }
                }
                var result = new Dictionary<string, object>
                {
                    { "Chain", sequence }
                };
                return result;
            });
        }
    }
}
