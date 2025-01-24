namespace Libiada.Web.Controllers.Sequences;

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
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformerController"/> class.
    /// </summary>
    public OrderTransformerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                      IViewDataHelper viewDataHelper,
                                      ITaskManager taskManager,
                                      ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory)
        : base(TaskType.OrderTransformer, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
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
    /// <param name="researchObjectId">
    /// The research object id.
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
    public ActionResult Index(long researchObjectId, OrderTransformation[] transformationsSequence, int iterationsCount)
    {
        return CreateTask(() =>
        {
            // TODO: add nature params
            using var db = dbFactory.CreateDbContext();
            var sequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId).Id;
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            var sequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);
            for (int j = 0; j < iterationsCount; j++)
            {
                for (int i = 0; i < transformationsSequence.Length; i++)
                {
                    sequence = transformationsSequence[i] == OrderTransformation.Dissimilar ? DissimilarSequenceFactory.Create(sequence)
                                                         : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[i]));
                }
            }

            var transformations = transformationsSequence.Select(ts => ts.GetDisplayValue());

            var result = new Dictionary<string, object>
            {
                { "sequence", sequence.ToString(" ") },
                { "transformationsList", transformations },
                { "iterationsCount", iterationsCount }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
