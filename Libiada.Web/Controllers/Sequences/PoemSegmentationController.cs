namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Sequences;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Newtonsoft.Json;

using Segmenter.PoemsSegmenter;

public class PoemSegmentationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly IResearchObjectsCache cache;

    public PoemSegmentationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                      IViewDataHelper viewDataHelper,
                                      ITaskManager taskManager,
                                      ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                      IResearchObjectsCache cache)
       : base(TaskType.PoemSegmentation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.cache = cache;
    }

    // GET: PoemSequenceSegmentation
    public ActionResult Index()
    {
        var data = viewDataHelper.FillViewData(1, 1, m => m.SequenceType == SequenceType.CompletePoem, "Segment");
        data.Add("nature", (byte)Nature.Literature);
        ViewBag.data = JsonConvert.SerializeObject(data);
        return View();
    }

    [HttpPost]
    public ActionResult Index(
        long researchObjectId,
        int wordLength,
        string startThreshold,
        string balance)
    {
        return CreateTask(() =>
        {
            using var db = dbFactory.CreateDbContext();
            var sequenceId = db.CombinedSequenceEntities.Single(l => l.ResearchObjectId == researchObjectId && l.Notation == Notation.Consonance).Id;
            var sequenceName = cache.ResearchObjects.Single(l => l.Id == researchObjectId).Name;
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            var sequence = sequenceRepository.GetLibiadaSequence(sequenceId);
            var thresholdString = startThreshold.Replace('.', ',');
            var threshold = Convert.ToDouble(thresholdString);
            var balanceString = balance.Replace('.', ',');
            var balanceDouble = Convert.ToDouble(balanceString);

            PoemSegmenter poemSegmenter = new PoemSegmenter(sequence.ToString(), wordLength, threshold, balanceDouble);

            (Dictionary<string, int>, string, string) resultSegmentation = poemSegmenter.StartSegmentation();
            var consonanceDictionary = resultSegmentation.Item1.OrderByDescending(d => d.Value).ToDictionary(d => d.Key, d => d.Value);
            string poemSequence = resultSegmentation.Item2;
            var result = new Dictionary<string, object>
            {
                {"segmentedString", consonanceDictionary},
                {"poemSequence", poemSequence },
                {"poemName", sequenceName}
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
