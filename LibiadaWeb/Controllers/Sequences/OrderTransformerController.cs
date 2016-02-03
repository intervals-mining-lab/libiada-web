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
            var matterIds = db.Matter.Select(d => d.Id).ToArray(); 

            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillMattersData(1, int.MaxValue, false, m => matterIds.Contains(m.Id), "Transform");

            var transformationLinks = new[] { Link.Start, Link.End, Link.CycleStart, Link.CycleEnd };
            transformationLinks = transformationLinks.OrderBy(n => (int)n).ToArray();
            data.Add("transformationLinks", transformationLinks.Select(l => new SelectListItem { Text = EnumHelper.GetDisplayValue(l), Value = ((int)l).ToString() }).ToArray());
            
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
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="transformationIds">
        /// The transformations.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId, int[] transformationLinkIds, int[] transformationIds)
        {
            return Action(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                var sequence = commonSequenceRepository.ToLibiadaChain(sequenceId);
                for (int i = 0; i < transformationIds.Length; i++)
                {
                    if (transformationIds[i] == 1)
                    {
                        sequence = DissimilarChainFactory.Create(sequence);
                    }
                    else
                    {
                        sequence = HighOrderFactory.Create(sequence, (Link)transformationLinkIds[i]);
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
