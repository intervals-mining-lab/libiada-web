namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
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
        public LocalCalculationController()
            : base("LocalCalculation", "Local calculation")
        {
            db = new LibiadaWebEntities();
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
            var calculatorsHelper = new ViewDataHelper(db);
            ViewBag.data = calculatorsHelper.FillCalculationData(c => c.FullSequenceApplicable, 1, int.MaxValue, true);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
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
                var matterNames = new List<string>();
                var notationNames = new List<string>();
                var characteristicNames = new List<string>();
                var partNames = new List<List<List<string>>>();
                var starts = new List<List<List<int>>>();
                var lengthes = new List<List<List<int>>>();
                var chains = new List<List<Chain>>();

                for (int k = 0; k < matterIds.Length; k++)
                {
                    long matterId = matterIds[k];
                    int natureId = db.Matter.Single(m => m.Id == matterId).NatureId;
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    chains.Add(new List<Chain>());

                    for (int n = 0; n < notationIds.Length; n++)
                    {
                        var notationId = notationIds[n];
                        long sequenceId;

                        switch (natureId)
                        {
                            case Aliases.Nature.Literature:
                                var languageId = languageIds[k];
                                var translatorId = translatorIds[k];
                                sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                                && l.NotationId == notationId
                                                                                && l.LanguageId == languageId
                                                                                && l.TranslatorId == translatorId).Id;
                                break;
                            default:
                                sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId
                                                                            && c.NotationId == notationId).Id;
                                break;
                        }

                        chains[k].Add(commonSequenceRepository.ToLibiadaChain(sequenceId));
                    }
                }

                List<List<List<double>>> characteristics = CalculateCharacteristics(chains, characteristicTypeLinkIds, length, step, growingWindow);

                for (int i = 0; i < chains.Count; i++)
                {
                    partNames.Add(new List<List<string>>());
                    starts.Add(new List<List<int>>());
                    lengthes.Add(new List<List<int>>());

                    for (int j = 0; j < chains[i].Count; j++)
                    {
                        partNames[i].Add(new List<string>());
                        starts[i].Add(new List<int>());
                        lengthes[i].Add(new List<int>());

                        var chain = chains[i][j];

                        CutRule cutRule = growingWindow
                        ? (CutRule)new CutRuleWithFixedStart(chain.GetLength(), step)
                        : new SimpleCutRule(chain.GetLength(), step, length);

                        CutRuleIterator iter = cutRule.GetIterator();

                        while (iter.Next())
                        {
                            var tempChain = new Chain(iter.GetEndPosition() - iter.GetStartPosition());

                            for (int m = 0; iter.GetStartPosition() + m < iter.GetEndPosition(); m++)
                            {
                                tempChain.Set(chain[iter.GetStartPosition() + m], m);
                            }

                            partNames[i][j].Add(tempChain.ToString());
                            starts[i][j].Add(iter.GetStartPosition());
                            lengthes[i][j].Add(tempChain.GetLength());
                        }

                        if (delta)
                        {
                            CalculateDelta(characteristics[i]);
                        }

                        if (fourier)
                        {
                            FastFourierTransform.FourierTransform(characteristics[i]);
                        }

                        if (autocorrelation)
                        {
                            AutoCorrelation.CalculateAutocorrelation(characteristics[i]);
                        }
                    }
                }

                for (int l = 0; l < characteristicTypeLinkIds.Length; l++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[l]).Name);
                }

                for (int w = 0; w < notationIds.Length; w++)
                {
                    var notationId = notationIds[w];
                    notationNames.Add(db.Notation.Single(n => n.Id == notationId).Name);
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "notationNames", notationNames },
                    { "starts", starts },
                    { "partNames", partNames },
                    { "lengthes", lengthes },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds }
                };
            });
        }

        /// <summary>
        /// The calculate delta.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        private static void CalculateDelta(List<List<double>> characteristics)
        {
            // Перебираем характеристики 
            for (int i = 0; i < characteristics.Count; i++)
            {
                // перебираем фрагменты цепочек
                for (int j = characteristics[i].Count - 1; j > 0; j--)
                {
                    characteristics[i][j] -= characteristics[i - 1][j];
                }

                characteristics[i].RemoveAt(0);
            }
        }

        /// <summary>
        /// The calculate characteristics.
        /// </summary>
        /// <param name="chains">
        /// The chains.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <param name="gowingWindow">
        /// The gowing window.
        /// </param>
        /// <returns>
        /// The <see cref="List{List{List{Double}}}"/>.
        /// </returns>
        private List<List<List<double>>> CalculateCharacteristics(
            List<List<Chain>> chains,
            int[] characteristicTypeLinkIds,
            int length,
            int step,
            bool gowingWindow)
        {
            var calculators = new List<IFullCalculator>();
            var links = new List<Link>();

            for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
            {
                string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[i]).ClassName;
                calculators.Add(CalculatorsFactory.CreateFullCalculator(className));
                links.Add(characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[i]));
            }

            var characteristics = new List<List<List<double>>>();
            for (int i = 0; i < chains.Count; i++)
            {
                characteristics.Add(new List<List<double>>());
                for (int j = 0; j < chains[i].Count; j++)
                {
                    characteristics[i].Add(new List<double>());
                    Chain chain = chains[i][j];

                    CutRule cutRule = gowingWindow
                        ? (CutRule)new CutRuleWithFixedStart(chain.GetLength(), step)
                        : new SimpleCutRule(chain.GetLength(), step, length);

                    CutRuleIterator iter = cutRule.GetIterator();

                    while (iter.Next())
                    {
                        var tempChain = new Chain();
                        tempChain.ClearAndSetNewLength(iter.GetEndPosition() - iter.GetStartPosition());

                        for (int k = 0; iter.GetStartPosition() + k < iter.GetEndPosition(); k++)
                        {
                            tempChain.Set(chain[iter.GetStartPosition() + k], k);
                        }

                        characteristics[i][j].Add(calculators[j].Calculate(tempChain, links[j]));
                    }
                }
            }

            return characteristics;
        }
    }
}
