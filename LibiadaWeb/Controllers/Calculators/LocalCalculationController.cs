namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

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
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCalculationController"/> class.
        /// </summary>
        public LocalCalculationController() : base(TaskType.LocalCalculation)
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
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
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="language">
        /// The language id.
        /// </param>
        /// <param name="translator">
        /// The translators id.
        /// </param>
        /// <param name="notation">
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
            int[] characteristicLinkIds,
            Language? language,
            Translator? translator,
            Notation notation,
            int length,
            int step,
            bool delta,
            bool fourier,
            bool growingWindow,
            bool autocorrelation)
        {
            return CreateTask(() =>
            {
                var characteristicNames = new string[characteristicLinkIds.Length];
                var partNames = new List<string>[matterIds.Length];
                var starts = new List<int>[matterIds.Length];
                var lengthes = new List<int>[matterIds.Length];
                var chains = new Chain[matterIds.Length];
                var mattersCharacteristics = new object[matterIds.Length];

                var calculators = new List<IFullCalculator>();
                var links = new List<Link>();
                matterIds = matterIds.OrderBy(m => m).ToArray();
                Dictionary<long, Matter> matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);

                for (int k = 0; k < matterIds.Length; k++)
                {
                    long matterId = matterIds[k];
                    Nature nature = db.Matter.Single(m => m.Id == matterId).Nature;

                    long sequenceId;
                    switch (nature)
                    {
                        case Nature.Literature:
                            sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                        && l.Notation == notation
                                                                        && l.Language == language
                                                                        && l.Translator == translator).Id;
                            break;
                        default:
                            sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId
                                                                    && c.Notation == notation).Id;
                            break;
                    }

                    chains[k] = commonSequenceRepository.GetLibiadaChain(sequenceId);
                }

                foreach (int characteristicLinkId in characteristicLinkIds)
                {
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetFullCharacteristic(characteristicLinkId);
                    calculators.Add(FullCalculatorsFactory.CreateCalculator(characteristic));
                    links.Add(characteristicTypeLinkRepository.GetLinkForFullCharacteristic(characteristicLinkId));
                }

                for (int i = 0; i < chains.Length; i++)
                {
                    CutRule cutRule = growingWindow
                            ? (CutRule)new CutRuleWithFixedStart(chains[i].GetLength(), step)
                            : new SimpleCutRule(chains[i].GetLength(), step, length);

                    CutRuleIterator iter = cutRule.GetIterator();

                    var fragments = new List<Chain>();
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

                for (int l = 0; l < characteristicLinkIds.Length; l++)
                {
                    characteristicNames[l] = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkIds[l]);
                }

                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkIds[k], notation);
                    characteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = characteristicNames[k],
                        Selected = false
                    };
                }

                var result = new Dictionary<string, object>
                {
                    { "characteristics", mattersCharacteristics },
                    { "notationName", notation.GetDisplayValue() },
                    { "starts", starts },
                    { "partNames", partNames },
                    { "lengthes", lengthes },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds },
                    { "characteristicsList", characteristicsList }
                };

                return new Dictionary<string, object> { { "data", JsonConvert.SerializeObject(result) } };
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
