namespace Libiada.Web.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Libiada.Web.Helpers;
    using Libiada.Database.Tasks;
    using Libiada.Database.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    using Segmenter.PoemsSegmenter;
    using Libiada.Web.Tasks;

    public class PoemSegmentationController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;
        private readonly ICommonSequenceRepository commonSequenceRepository;

        public PoemSegmentationController(LibiadaDatabaseEntities db,
                                          IViewDataHelper viewDataHelper,
                                          ITaskManager taskManager,
                                          ICommonSequenceRepository commonSequenceRepository)
           : base(TaskType.PoemSegmentation, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
            this.commonSequenceRepository = commonSequenceRepository;
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
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long matterId,
            int wordLength,
            string startThreshold,
            string balance)
        {
            return CreateTask(() =>
            {
                var sequenceId = db.LiteratureSequences.Single(l => l.MatterId == matterId && l.Notation == Notation.Consonance).Id;
                var sequenceName = db.Matters.Single(l => l.Id == matterId).Name;
                var chain = commonSequenceRepository.GetLibiadaBaseChain(sequenceId);
                var thresholdString = startThreshold.Replace('.', ',');
                var threshold = Convert.ToDouble(thresholdString);
                var balanceString = balance.Replace('.', ',');
                var balanceDouble = Convert.ToDouble(balanceString);

                PoemSegmenter poemSegmenter = new PoemSegmenter(chain.ToString(), wordLength, threshold, balanceDouble);

                var resultSegmentation = poemSegmenter.StartSegmentation();
                var consonanceDictionary = resultSegmentation.Item1.OrderByDescending(d => d.Value).ToDictionary(d => d.Key, d => d.Value);
                var poemChain = resultSegmentation.Item2;
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
}
