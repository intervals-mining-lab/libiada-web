using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Core;
using LibiadaCore.Misc;
using LibiadaCore.Misc.DataTransformers;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Sequences;

namespace LibiadaWeb.Controllers.Sequences
{
    public class OrderTransformationController : AbstractResultController
    {
       /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The dna sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceTransformerController"/> class.
        /// </summary>
        public OrderTransformationController() : base("OrderTransformation", "Order transformation")
        {
            db = new LibiadaWebEntities();
            dnaSequenceRepository = new DnaSequenceRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
            elementRepository = new ElementRepository(db);
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

            return View();
        }


        [HttpPost]
        public ActionResult Index(long matterId, int linkId)
        {
            return Action(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                Chain sourceChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                BaseChain transformedChain = HighOrderFactory.Create(sourceChain, (LibiadaCore.Core.Link) linkId);
                var result = new Dictionary<string, object>
                {
                    {"Chain", transformedChain}
                };
                return result;
            });
        }

    }
}
