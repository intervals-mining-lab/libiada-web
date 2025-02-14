namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.DataTransformers;
using Libiada.Core.Extensions;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

/// <summary>
/// The order transformation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrderTransformationConvergenceController : AbstractResultController
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformationConvergenceController"/> class.
    /// </summary>
    public OrderTransformationConvergenceController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                                    IViewDataHelper viewDataHelper,
                                                    ITaskManager taskManager,
                                                    ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory)
        : base(TaskType.OrderTransformationConvergence, taskManager)
    {
        this.dbFactory = dbFactory;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
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
        var data = viewDataHelper.AddMinMaxResearchObjects(1, 1)
                                 .AddNatures()
                                 .AddNotations()
                                 .AddLanguages()
                                 .AddTranslators()
                                 .AddPauseTreatments()
                                 .AddTrajectories()
                                 .AddSequenceTypes()
                                 .AddGroups()
                                 .AddOrderTransformations()
                                 .AddSubmitName("Transform")
                                 .Build();
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
            int loopIteration = -1;
            int lastIteration = -1;
            List<int[]> transformationsResult = new(iterationsCount + 1) { sequence.Order };

            for (int j = 0; j < iterationsCount; j++)
            {
                for (int i = 0; i < transformationsSequence.Length; i++)
                {

                    sequence = transformationsSequence[i] == OrderTransformation.Dissimilar ? DissimilarSequenceFactory.Create(sequence)
                                                         : HighOrderFactory.Create(sequence, transformationsSequence[i].GetLink());

                    if (transformationsResult.Any(tr => tr.SequenceEqual(sequence.Order)))
                    {
                        loopIteration = transformationsResult.FindIndex(tr => tr.SequenceEqual(sequence.Order)) + 1;
                        lastIteration = j + 1;
                        goto exitLoops;
                    }
                    transformationsResult.Add(sequence.Order);
                }
            }

        exitLoops:

            var transformations = transformationsSequence.Select(ts => ts.GetDisplayValue());

            var result = new Dictionary<string, object>
            {
                { "sequence", sequence.ToString(" ") },
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
