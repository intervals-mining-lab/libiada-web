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

    using Newtonsoft.Json;

    /// <summary>
    /// The local calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
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
        public LocalCalculationController() : base("Local calculation")
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
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(c => c.FullSequenceApplicable, 1, int.MaxValue, "Calculate"));
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
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="translatorId">
        /// The translators id.
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
        /// The is auto correlation.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
            int? languageId,
            int? translatorId,
            int notationId,
            int length,
            int step,
            bool delta,
            bool fourier,
            bool growingWindow,
            bool autocorrelation)
        {
            return Action(() =>
            {
                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                var partNames = new List<string>[matterIds.Length];
                var starts = new List<int>[matterIds.Length];
                var lengthes = new List<int>[matterIds.Length];
                var chains = new Chain[matterIds.Length];
                var mattersCharacteristics = new object[matterIds.Length];

                var calculators = new List<IFullCalculator>();
                var links = new List<Link>();
                matterIds = matterIds.OrderBy(m => m).ToArray();
                var matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);

                for (int k = 0; k < matterIds.Length; k++)
                {
                    long matterId = matterIds[k];
                    Nature nature = db.Matter.Single(m => m.Id == matterId).Nature;

                    long sequenceId;
                    switch (nature)
                    {
                        case Nature.Literature:
                            sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                        && l.NotationId == notationId
                                                                        && l.LanguageId == languageId
                                                                        && l.TranslatorId == translatorId).Id;
                            break;
                        default:
                            var id = notationId;
                            sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId
                                                                    && c.NotationId == id).Id;
                            break;
                    }

                    chains[k] = commonSequenceRepository.ToLibiadaChain(sequenceId);
                }

                foreach (int characteristicTypeLinkId in characteristicTypeLinkIds)
                {
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                    calculators.Add(CalculatorsFactory.CreateFullCalculator(className));
                    links.Add(characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId));
                }

                for (int i = 0; i < chains.Length; i++)
                {
                    CutRule cutRule = growingWindow
                            ? (CutRule)new CutRuleWithFixedStart(chains[i].GetLength(), step)
                            : new SimpleCutRule(chains[i].GetLength(), step, length);

                    CutRuleIterator iter = cutRule.GetIterator();

                    List<Chain> fragments = new List<Chain>();
                    partNames[i] = new List<string>();
                    starts[i] = new List<int>();
                    lengthes[i] = new List<int>();
                    

                    while (iter.Next())
                    {
                        var fragment = new Chain(iter.GetEndPosition() - iter.GetStartPosition());

                        for (int k = 0; iter.GetStartPosition() + k < iter.GetEndPosition(); k++)
                        {
                            fragment.Set(chains[i][iter.GetStartPosition() + k], k);
                        }

                        fragments.Add(fragment);
                        partNames[i].Add(fragment.ToString());
                        starts[i].Add(iter.GetStartPosition());
                        lengthes[i].Add(fragment.GetLength());
                    }

                    var fragmentsData = new FragmentData[fragments.Count];
                    for (int k = 0; k < fragments.Count; k++)
                    {
                        var characteristics = new double[calculators.Count];
                        for (int j = 0; j < calculators.Count; j++)
                        {
                            characteristics[j] = calculators[j].Calculate(fragments[k], links[j]);
                        }

                        fragmentsData[k] = new FragmentData(characteristics, fragments[k].ToString(), starts[i][k], fragments[k].GetLength());
                    }

                    double[][] differenceData = null;
                    double[][] fourierData = null;
                    double[][] autocorrelationData = null;

                    if (delta)
                    {
                        differenceData = CalculateDifference(fragmentsData.Select(f => f.Characteristics).ToArray());
                    }

                    if (fourier)
                    {
                        fourierData = FastFourierTransform.CalculateFastFourierTransform(fragmentsData.Select(f => f.Characteristics).ToArray());
                    }

                    if (autocorrelation)
                    {
                        autocorrelationData = AutoCorrelation.CalculateAutocorrelation(fragmentsData.Select(f => f.Characteristics).ToArray());
                    }

                    mattersCharacteristics[i] = new { matterName = matters[matterIds[i]].Name, fragmentsData, differenceData, fourierData, autocorrelationData };
                }

                for (int l = 0; l < characteristicTypeLinkIds.Length; l++)
                {
                    characteristicNames[l] = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[l]).Name;
                }

                string notationName = db.Notation.Single(n => n.Id == notationId).Name;

                return new Dictionary<string, object>
                {
                    { "characteristics", mattersCharacteristics },
                    { "notationName", notationName },
                    { "starts", starts },
                    { "partNames", partNames },
                    { "lengthes", lengthes },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds }
                };
            });
        }

        /// <summary>
        /// Calculates difference between characteristics of nearest fragments.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        /// <returns>
        /// Difference between next and current fragment characteristics as <see cref="T:double[][]"/>.
        /// </returns>
        private double[][] CalculateDifference(double[][] characteristics)
        {
            var result = new double[characteristics.Length - 1][];

            // cycle through fragments
            for (int i = 0; i < characteristics.Length - 1; i++)
            {
                result[i] = new double[characteristics[i].Length];

                // cycle through characteristics
                for (int j = 0; j < characteristics[i].Length; j++)
                {
                    result[i][j] = characteristics[i + 1][j] - characteristics[i][j];
                }
            }

            return result;
        }
    }
}
