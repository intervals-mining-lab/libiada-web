namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaCore.DataTransformers;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// The order transformation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationConvergenceController : AbstractResultController
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
        /// Initializes a new instance of the <see cref="OrderTransformationConvergenceController"/> class.
        /// </summary>
        public OrderTransformationConvergenceController() : base(TaskType.OrderTransformationConvergence)
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

            var transformations = EnumHelper.GetSelectList(typeof(OrderTransformation));
            data.Add("transformations", transformations);

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
        /// <param name="transformationsSequnce">
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
        public ActionResult Index(long matterId, OrderTransformation[] transformationsSequence, int iterationsCount)
        {
            return CreateTask(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                var sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
                int loopIteration = -1;
                int lastIteration = -1;
                var transformationsResult = new List<int[]>();
                transformationsResult.Add(sequence.Building);

                for (int j = 0; j < iterationsCount; j++)
                {
                    for (int i = 0; i < transformationsSequence.Length; i++)
                    {

                        sequence = transformationsSequence[i] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                             : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[i]));

                        if (transformationsResult.Any(tr => tr.SequenceEqual(sequence.Building)))
                        {
                            loopIteration = transformationsResult.FindIndex(tr => tr.SequenceEqual(sequence.Building)) + 1;
                            lastIteration = j + 1;
                            goto exitLoops;
                        }
                        transformationsResult.Add(sequence.Building);
                    }
                }

                exitLoops:

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationsSequence.Length; i++)
                {
                    transformations.Add(i, transformationsSequence[i].GetDisplayValue());
                }

                var result = new Dictionary<string, object>
                {
                    { "chain", sequence },
                    { "transformationsList", transformations },
                    { "iterationsCount", iterationsCount },
                    { "transformationsResult", transformationsResult },
                    { "loopIteration", loopIteration },
                    { "lastIteration", lastIteration }
                };
                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
