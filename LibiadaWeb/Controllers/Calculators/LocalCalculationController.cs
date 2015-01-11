namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Models.Repositories.Sequences;

    using Math;

    using Models.Repositories.Catalogs;

    /// <summary>
    /// The local calculation controller.
    /// </summary>
    public class LocalCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The link repository.
        /// </summary>
        private readonly LinkRepository linkRepository;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCalculationController"/> class.
        /// </summary>
        public LocalCalculationController() : base("LocalCalculation", "Local calculation")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkRepository = new LinkRepository(db);
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
            ViewBag.dbName = DbHelper.GetDbName(db);
            var matters = db.Matter.Include(m => m.Nature);
            ViewBag.matterCheckBoxes = matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable);
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristicsList, null);

            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linksList = linkRepository.GetSelectListItems(null);
            ViewBag.languagesList = new SelectList(db.Language, "id", "name");

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <param name="isDelta">
        /// The is delta.
        /// </param>
        /// <param name="isFourier">
        /// The Fourier transform flag.
        /// </param>
        /// <param name="isGrowingWindow">
        /// The is growing window.
        /// </param>
        /// <param name="isAutocorrelation">
        /// The is auto corelation.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicIds, 
            int[] linkIds, 
            int languageId, 
            int notationId, 
            int length, 
            int step,
            bool isDelta,
            bool isFourier, 
            bool isGrowingWindow,
            bool isAutocorrelation)
        {
            return Action(() =>
            {
                List<List<List<double>>> characteristics = CalculateCharacteristics(
                    matterIds,
                    isGrowingWindow,
                    notationId,
                    languageId,
                    length,
                    characteristicIds,
                    linkIds,
                    step);

                var matterNames = new List<string>();
                var characteristicNames = new List<string>();
                var partNames = new List<List<string>>();
                var starts = new List<List<int>>();
                var lengthes = new List<List<int>>();

                for (int k = 0; k < matterIds.Length; k++)
                {
                    long matterId = matterIds[k];
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    partNames.Add(new List<string>());
                    starts.Add(new List<int>());
                    lengthes.Add(new List<int>());

                    long sequenceId;
                    if (db.Matter.Single(m => m.Id == matterId).NatureId == 3)
                    {
                        sequenceId =
                            db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                            && l.NotationId == notationId
                                                            && l.LanguageId == languageId).Id;
                    }
                    else
                    {
                        sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                    }

                    Chain chain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                    CutRule cutRule = isGrowingWindow
                        ? (CutRule)new CutRuleWithFixedStart(chain.GetLength(), step)
                        : new SimpleCutRule(chain.GetLength(), step, length);

                    CutRuleIterator iter = cutRule.GetIterator();

                    while (iter.Next())
                    {
                        var tempChain = new Chain(iter.GetEndPosition() - iter.GetStartPosition());

                        for (int i = 0; iter.GetStartPosition() + i < iter.GetEndPosition(); i++)
                        {
                            tempChain.Set(chain[iter.GetStartPosition() + i], i);
                        }

                        partNames.Last().Add(tempChain.ToString());
                        starts.Last().Add(iter.GetStartPosition());
                        lengthes.Last().Add(tempChain.GetLength());
                    }

                    if (isDelta)
                    {
                        CalculateDelta(characteristics);
                    }

                    if (isFourier)
                    {
                        FastFourierTransform.FourierTransform(characteristics);
                    }
                }

                if (isAutocorrelation)
                {
                    var autoCorrelation = new AutoCorrelation();
                    autoCorrelation.CalculateAutocorrelation(characteristics);
                }

                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    int characteristicId = characteristicIds[i];
                    int linkId = linkIds[i];
                    characteristicNames.Add(db.CharacteristicType.Single(c => c.Id == characteristicId).Name + " " +
                                            db.Link.Single(l => l.Id == linkId).Name);
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    });
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "starts", starts },
                    { "lengthes", lengthes },
                    { "characteristicIds", new List<int>(characteristicIds) },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds },
                    { "characteristicsList", characteristicsList }
                };
            });
        }

        /// <summary>
        /// The calculate delta.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        private static void CalculateDelta(List<List<List<double>>> characteristics)
        {
            // Перебираем характеристики 
            for (int i = 0; i < characteristics.Last().Last().Count; i++)
            {
                // перебираем фрагменты цепочек
                for (int j = characteristics.Last().Count - 1; j > 0; j--)
                {
                    characteristics.Last()[j][i] -= characteristics.Last()[j - 1][i];
                }
            }

            characteristics.Last().RemoveAt(0);
        }

        /// <summary>
        /// The calculate characteristics.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="isGrowingWindow">
        /// The is growing window.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<List<List<double>>> CalculateCharacteristics(
            long[] matterIds,
            bool isGrowingWindow,
            int notationId,
            int languageId,
            int length,
            int[] characteristicIds,
            int[] linkIds,
            int step)
        {
            var calculators = new List<IFullCalculator>();

            for (int i = 0; i < characteristicIds.Length; i++)
            {
                int characteristicId = characteristicIds[i];
                string className = db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;
                calculators.Add(CalculatorsFactory.CreateFullCalculator(className));
            }

            var characteristics = new List<List<List<double>>>();
            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                characteristics.Add(new List<List<double>>());

                long sequenceId;
                if (db.Matter.Single(m => m.Id == matterId).NatureId == 3)
                {
                    sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId && 
                                                              l.NotationId == notationId && 
                                                              l.LanguageId == languageId).Id;
                }
                else
                {
                    sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                }

                Chain chain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                
                CutRule cutRule = isGrowingWindow
                    ? (CutRule)new CutRuleWithFixedStart(chain.GetLength(), step)
                    : new SimpleCutRule(chain.GetLength(), step, length);

                CutRuleIterator iter = cutRule.GetIterator();

                while (iter.Next())
                {
                    characteristics.Last().Add(new List<double>());
                    var tempChain = new Chain();
                    tempChain.ClearAndSetNewLength(iter.GetEndPosition() - iter.GetStartPosition());

                    for (int i = 0; iter.GetStartPosition() + i < iter.GetEndPosition(); i++)
                    {
                        tempChain.Set(chain[iter.GetStartPosition() + i], i);
                    }

                    for (int i = 0; i < characteristicIds.Length; i++)
                    {
                        characteristics.Last().Last().Add(calculators[i].Calculate(tempChain, (Link)linkIds[i]));
                    }
                }
            }

            return characteristics;
        }
    }
}
