namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Models;
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
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCalculationController"/> class.
        /// </summary>
        public LocalCalculationController() : base("LocalCalculation", "Local calculation")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            notationRepository = new NotationRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable).Select(c => c.Id);
            var characteristicTypes = db.CharacteristicTypeLink.Where(c => characteristicsList.Contains(c.CharacteristicTypeId)).ToList();

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", 1 },
                    { "maximumSelectedMatters", int.MaxValue },
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "links", links }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
                    { "translators", translators }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic typw and link ids.
        /// </param>
        /// <param name="languageIds">
        /// The language id.
        /// </param>
        /// <param name="translatorIds">
        /// The translators ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <param name="delta">
        /// The is delta.
        /// </param>
        /// <param name="fourier">
        /// The Fourier transform flag.
        /// </param>
        /// <param name="growingWindow">
        /// The is growing window.
        /// </param>
        /// <param name="autocorrelation">
        /// The is auto corelation.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicTypeLinkIds,  
            int?[] languageIds, 
            int?[] translatorIds,
            int[] notationIds, 
            int length, 
            int step,
            bool delta,
            bool fourier, 
            bool growingWindow,
            bool autocorrelation)
        {
            return Action(() =>
            {
                List<List<List<double>>> characteristics = CalculateCharacteristics(
                    matterIds,
                    growingWindow,
                    notationIds,
                    languageIds,
                    translatorIds,
                    length,
                    characteristicTypeLinkIds,
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
                    var notationId = notationIds[k];
                    if (db.Matter.Single(m => m.Id == matterId).NatureId == Aliases.Nature.Literature)
                    {
                        var languageId = languageIds[k];
                        var translatorId = translatorIds[k];

                        sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId &&
                                                                  l.NotationId == notationId &&
                                                                  l.LanguageId == languageId &&
                                                                  l.TranslatorId == translatorId).Id;
                    }
                    else
                    {
                        sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                    }

                    Chain chain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                    CutRule cutRule = growingWindow
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

                    if (delta)
                    {
                        CalculateDelta(characteristics);
                    }

                    if (fourier)
                    {
                        FastFourierTransform.FourierTransform(characteristics);
                    }

                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notationIds[k]));
                }

                if (autocorrelation)
                {
                    var autoCorrelation = new AutoCorrelation();
                    autoCorrelation.CalculateAutocorrelation(characteristics);
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
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
                    { "characteristicIds", new List<int>(characteristicTypeLinkIds) },
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
        /// <param name="gowingWindow">
        /// The is growing window.
        /// </param>
        /// <param name="notationIds">
        /// The notations ids.
        /// </param>
        /// <param name="languageIds">
        /// The languages ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translators ids.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <returns>
        /// The <see cref="List{List{List{Double}}}"/>.
        /// </returns>
        private List<List<List<double>>> CalculateCharacteristics(
            long[] matterIds,
            bool gowingWindow,
            int[] notationIds,
            int?[] languageIds,
            int?[] translatorIds,
            int length,
            int[] characteristicTypeLinkIds,
            int step)
        {
            var calculators = new List<IFullCalculator>();

            for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
            {
                string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[i]).ClassName;
                calculators.Add(CalculatorsFactory.CreateFullCalculator(className));
            }

            var characteristics = new List<List<List<double>>>();
            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                characteristics.Add(new List<List<double>>());

                long sequenceId;
                var notationId = notationIds[k];
                if (db.Matter.Single(m => m.Id == matterId).NatureId == Aliases.Nature.Literature)
                {
                    var languageId = languageIds[k];
                    var translatorId = translatorIds[k];

                    sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId &&
                                                              l.NotationId == notationId &&
                                                              l.LanguageId == languageId &&
                                                              l.TranslatorId == translatorId).Id;
                }
                else
                {
                    sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                }

                Chain chain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                
                CutRule cutRule = gowingWindow
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

                    for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                    {
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[i]);
                        characteristics.Last().Last().Add(calculators[i].Calculate(tempChain, link));
                    }
                }
            }

            return characteristics;
        }
    }
}
