namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Sequences;

using Newtonsoft.Json;

using Segmenter.PoemsSegmenter;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

public class PoemSegmentationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;
    private readonly Cache cache;

    public PoemSegmentationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                      IViewDataHelper viewDataHelper,
                                      ITaskManager taskManager,
                                      ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                      Cache cache)
       : base(TaskType.PoemSegmentation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
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
        long matterId,
        int wordLength,
        string startThreshold,
        string balance)
    {
        return CreateTask(() =>
        {
            using var db = dbFactory.CreateDbContext();
            var sequenceId = db.LiteratureSequences.Single(l => l.MatterId == matterId && l.Notation == Notation.Consonance).Id;
            var sequenceName = cache.Matters.Single(l => l.Id == matterId).Name;
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            var chain = commonSequenceRepository.GetLibiadaBaseChain(sequenceId);
            var thresholdString = startThreshold.Replace('.', ',');
            var threshold = Convert.ToDouble(thresholdString);
            var balanceString = balance.Replace('.', ',');
            var balanceDouble = Convert.ToDouble(balanceString);

            PoemSegmenter poemSegmenter = new PoemSegmenter(chain.ToString(), wordLength, threshold, balanceDouble);

            (Dictionary<string, int>, string, string) resultSegmentation = poemSegmenter.StartSegmentation();
            var consonanceDictionary = resultSegmentation.Item1.OrderByDescending(d => d.Value).ToDictionary(d => d.Key, d => d.Value);
            string poemChain = resultSegmentation.Item2;
            var result = new Dictionary<string, object>
            {
                {"segmentedString", consonanceDictionary},
                {"poemChain", poemChain },
                {"poemName", sequenceName}
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
