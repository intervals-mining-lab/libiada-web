﻿namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.DataTransformers;
using Libiada.Core.Extensions;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// The order transformation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrderTransformerController : AbstractResultController
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformerController"/> class.
    /// </summary>
    public OrderTransformerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                      IViewDataHelper viewDataHelper,
                                      ITaskManager taskManager,
                                      ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory)
        : base(TaskType.OrderTransformer, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
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
    public ActionResult Index(long matterId, OrderTransformation[] transformationsSequence, int iterationsCount)
    {
        return CreateTask(() =>
        {
            // TODO: add nature params
            using var db = dbFactory.CreateDbContext();
            var sequenceId = db.CommonSequences.Single(c => c.MatterId == matterId).Id;
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            var sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
            for (int j = 0; j < iterationsCount; j++)
            {
                for (int i = 0; i < transformationsSequence.Length; i++)
                {
                    sequence = transformationsSequence[i] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                         : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[i]));
                }
            }

            var transformations = transformationsSequence.Select(ts => ts.GetDisplayValue());

            var result = new Dictionary<string, object>
            {
                { "chain", sequence.ToString(" ") },
                { "transformationsList", transformations },
                { "iterationsCount", iterationsCount }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
