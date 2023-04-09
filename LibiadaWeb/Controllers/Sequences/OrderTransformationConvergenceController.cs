﻿namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using LibiadaCore.DataTransformers;

    using LibiadaWeb.Helpers;

    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using LibiadaCore.Extensions;
    using LibiadaWeb.Tasks;

    /// <summary>
    /// The order transformation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationConvergenceController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaDatabaseEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;
        private readonly IViewDataHelper viewDataHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationConvergenceController"/> class.
        /// </summary>
        public OrderTransformationConvergenceController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper, ITaskManager taskManager) : base(TaskType.OrderTransformationConvergence, taskManager)
        {
            this.db = db;
            commonSequenceRepository = new CommonSequenceRepository(db);
            this.viewDataHelper = viewDataHelper;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var data = viewDataHelper.FillViewData(1, 1, "Transform");

            var transformations = Extensions.EnumExtensions.GetSelectList<OrderTransformation>();
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
                // TODO: add nature params
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId).Id;
                var sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
                int loopIteration = -1;
                int lastIteration = -1;
                var transformationsResult = new List<int[]>(iterationsCount + 1) { sequence.Building };

                for (int j = 0; j < iterationsCount; j++)
                {
                    for (int i = 0; i < transformationsSequence.Length; i++)
                    {

                        sequence = transformationsSequence[i] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                             : HighOrderFactory.Create(sequence, transformationsSequence[i].GetLink());

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

                var transformations = transformationsSequence.Select(ts =>ts.GetDisplayValue());

                var result = new Dictionary<string, object>
                {
                    { "chain", sequence.ToString(" ") },
                    { "transformationsList", transformations },
                    { "iterationsCount", iterationsCount },
                    { "transformationsResult", transformationsResult },
                    { "loopIteration", loopIteration },
                    { "lastIteration", lastIteration }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
