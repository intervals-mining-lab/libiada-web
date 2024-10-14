namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.Extensions;
using Libiada.Core.Iterators;
using Libiada.Core.Music;
using Libiada.Core.TimeSeries.Aggregators;
using Libiada.Core.TimeSeries.Aligners;
using Libiada.Core.TimeSeries.OneDimensional.DistanceCalculators;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.CalculatorsData;

using Newtonsoft.Json;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;
using Libiada.Web.Math;

/// <summary>
/// The local calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class LocalCalculationController : AbstractResultController
{
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalCalculationController"/> class.
    /// </summary>
    public LocalCalculationController(IViewDataHelper viewDataHelper, 
                                      ITaskManager taskManager,
                                      ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                      IFullCharacteristicRepository characteristicTypeLinkRepository,
                                      Cache cache)
        : base(TaskType.LocalCalculation, taskManager)
    {
        this.viewDataHelper = viewDataHelper;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.cache = cache;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate");
        ViewBag.data = JsonConvert.SerializeObject(viewData);
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
    /// <param name="pauseTreatment">
    /// Pause treatment parameters of music sequences.
    /// </param>
    /// <param name="sequentialTransfer">
    /// Sequential transfer flag used in music sequences.
    /// </param>
    /// <param name="trajectory">
    /// Reading trajectory for images.
    /// </param> 
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(
        long[] matterIds,
        short[] characteristicLinkIds,
        int length,
        int step,
        bool delta,
        bool fourier,
        bool growingWindow,
        bool autocorrelation,
        Notation notation,
        Language? language,
        Translator? translator,
        PauseTreatment? pauseTreatment,
        bool? sequentialTransfer,
        ImageOrderExtractor? trajectory)
    {
        return CreateTask(() =>
        {
            string[] characteristicNames = new string[characteristicLinkIds.Length];
            var partNames = new List<string>[matterIds.Length];
            var starts = new List<int>[matterIds.Length];
            var lengthes = new List<int>[matterIds.Length];
            var chains = new Chain[matterIds.Length];
            var mattersCharacteristics = new object[matterIds.Length];

            var calculators = new IFullCalculator[characteristicLinkIds.Length];
            var links = new Link[characteristicLinkIds.Length];
            Array.Sort(matterIds);
            Dictionary<long, Matter> matters = cache.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                Nature nature = cache.Matters.Single(m => m.Id == matterId).Nature;

                long sequenceId = commonSequenceRepository.GetSequenceIds([matterId],
                                                                       notation,
                                                                       language,
                                                                       translator,
                                                                       pauseTreatment,
                                                                       sequentialTransfer,
                                                                       trajectory).Single();

                chains[k] = commonSequenceRepository.GetLibiadaChain(sequenceId);
            }

            for (int i = 0; i < characteristicLinkIds.Length; i++)
            {
                int characteristicLinkId = characteristicLinkIds[i];
                FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                calculators[i] = FullCalculatorsFactory.CreateCalculator(characteristic);
                links[i] = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            }

            for (int i = 0; i < chains.Length; i++)
            {
                CutRule cutRule = growingWindow
                        ? (CutRule)new CutRuleWithFixedStart(chains[i].Length, step)
                        : new SimpleCutRule(chains[i].Length, step, length);

                CutRuleIterator iterator = cutRule.GetIterator();

                List<Chain> fragments = [];
                partNames[i] = [];
                starts[i] = [];
                lengthes[i] = [];

                while (iterator.Next())
                {
                    int start = iterator.GetStartPosition();
                    int end = iterator.GetEndPosition();

                    List<IBaseObject> fragment = [];
                    for (int k = 0; start + k < end; k++)
                    {
                        fragment.Add(chains[i][start + k]);
                    }

                    fragments.Add(new Chain(fragment));

                    partNames[i].Add(fragment.ToString());
                    starts[i].Add(iterator.GetStartPosition());
                    lengthes[i].Add(fragment.Count);
                }

                var fragmentsData = new FragmentData[fragments.Count];
                for (int k = 0; k < fragments.Count; k++)
                {
                    double[] characteristics = new double[calculators.Length];
                    for (int j = 0; j < calculators.Length; j++)
                    {
                        characteristics[j] = calculators[j].Calculate(fragments[k], links[j]);
                    }

                    fragmentsData[k] = new FragmentData(characteristics, fragments[k].ToString(), starts[i][k], fragments[k].Length);
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

                mattersCharacteristics[i] = new LocalCharacteristicsData(matters[matterIds[i]].Name,
                                                                         fragmentsData,
                                                                         differenceData,
                                                                         fourierData,
                                                                         autocorrelationData);
            }

            for (int l = 0; l < characteristicLinkIds.Length; l++)
            {
                characteristicNames[l] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[l]);
            }

            var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
            for (int k = 0; k < characteristicLinkIds.Length; k++)
            {
                characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k], notation);
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
                { "characteristicsList", characteristicsList },
                { "aligners", Extensions.EnumExtensions.GetSelectList<Aligner>() },
                { "distanceCalculators", Extensions.EnumExtensions.GetSelectList<DistanceCalculator>() },
                { "aggregators", Extensions.EnumExtensions.GetSelectList<Aggregator>() }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
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
    [NonAction]
    private double[][] CalculateDifference(double[][] characteristics)
    {
        double[][] result = new double[characteristics.Length - 1][];

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
