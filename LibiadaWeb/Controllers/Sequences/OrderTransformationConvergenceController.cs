namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;
    using LibiadaCore.Misc;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

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

            var transformationLinks = new[] { Link.Start, Link.End, Link.CycleStart, Link.CycleEnd };
            transformationLinks = transformationLinks.OrderBy(n => (int)n).ToArray();
            data.Add("transformationLinks", transformationLinks.ToSelectList());

            var operations = new[]
            {
                new SelectListItem { Text = "Dissimilar", Value = 1.ToString() },
                new SelectListItem { Text = "Higher order", Value = 2.ToString() }
            };
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
        public ActionResult Index(long matterId, Link[] transformationLinkIds, int[] transformationIds, int iterationsCount)
        {
            return CreateTask(() =>
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                var sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
                int loopIneration = -1;
                var transformationsResult = new List<int[]>();
                transformationsResult.Add(sequence.Building);
                for (int j = 0; j < iterationsCount; j++)
                {
                    for (int i = 0; i < transformationIds.Length; i++)
                    {

                        sequence = transformationIds[i] == 1 ? DissimilarChainFactory.Create(sequence)
                                                             : HighOrderFactory.Create(sequence, transformationLinkIds[i]);

                        if (transformationsResult.Any(tr => tr.SequenceEqual(sequence.Building)))
                        {
                            loopIneration = transformationsResult.FindIndex(tr => tr.SequenceEqual(sequence.Building));
                            break;
                        }
                        transformationsResult.Add(sequence.Building);
                    }
                }

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationIds.Length; i++)
                {
                    transformations.Add(i, transformationIds[i] == 1 ? "dissimilar" : $"higher order {transformationLinkIds[i].GetDisplayValue()}");
                }

                var result = new Dictionary<string, object>
                {
                    { "chain", sequence },
                    { "transformationsList", transformations },
                    { "iterationsCount", iterationsCount },
                    { "transformationsResult", transformationsResult },
                    { "loopIteration", loopIneration }
                };
                return result;
            });
        }
    }
}
