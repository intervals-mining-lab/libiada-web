namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.DataTransformers;
using Libiada.Core.Extensions;
using Libiada.Core.Music;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

using Newtonsoft.Json;

using Microsoft.EntityFrameworkCore;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// The order transformation calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrderTransformationCalculationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformationCalculationController"/> class.
    /// </summary>
    public OrderTransformationCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                                    IViewDataHelper viewDataHelper, 
                                                    ITaskManager taskManager,
                                                    IFullCharacteristicRepository characteristicTypeLinkRepository,
                                                    Cache cache)
        : base(TaskType.OrderTransformationCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
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
        Dictionary<string, object> data = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate");

        var transformations = Extensions.EnumExtensions.GetSelectList<OrderTransformation>();
        data.Add("transformations", transformations);

        ViewBag.data = JsonConvert.SerializeObject(data);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterIds">
    /// The matter ids.
    /// </param>
    /// <param name="transformationsSequence">
    /// The transformation ids.
    /// </param>
    /// <param name="iterationsCount">
    /// Number of transformations iterations.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// The characteristic type link ids.
    /// </param>
    /// <param name="notations">
    /// The notation ids.
    /// </param>
    /// <param name="languages">
    /// The language ids.
    /// </param>
    /// <param name="translators">
    /// The translator ids.
    /// </param>
    /// <param name="pauseTreatments">
    /// Pause treatment parameters of music sequences.
    /// </param>
    /// <param name="sequentialTransfers">
    /// Sequential transfer flag used in music sequences.
    /// </param>
    /// <param name="trajectories">
    /// Reading trajectories for images.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(
        long[] matterIds,
        OrderTransformation[] transformationsSequence,
        int iterationsCount,
        short[] characteristicLinkIds,
        Notation[] notations,
        Language[] languages,
        Translator[] translators,
        PauseTreatment[] pauseTreatments,
        bool[] sequentialTransfers,
        ImageOrderExtractor[] trajectories)
    {
        return CreateTask(() =>
        {
            Dictionary<long, string> mattersNames = cache.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
            Chain[][] sequences = new Chain[matterIds.Length][];

            var commonSequenceRepository = new CommonSequenceRepository(dbFactory, cache);
            long[][] sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds,
                                                                           notations,
                                                                           languages,
                                                                           translators,
                                                                           pauseTreatments,
                                                                           sequentialTransfers,
                                                                           trajectories);
            for (int i = 0; i < matterIds.Length; i++)
            {
                sequences[i] = new Chain[characteristicLinkIds.Length];
                for (int j = 0; j < characteristicLinkIds.Length; j++)
                {
                    sequences[i][j] = commonSequenceRepository.GetLibiadaChain(sequenceIds[i][j]);
                }
            }

            var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
            Array.Sort(matterIds);

            for (int i = 0; i < matterIds.Length; i++)
            {
                long matterId = matterIds[i];
                double[] characteristics = new double[characteristicLinkIds.Length];
                for (int j = 0; j < characteristicLinkIds.Length; j++)
                {
                    Notation notation = notations[j];


                    Chain sequence = sequences[i][j];
                    for (int l = 0; l < iterationsCount; l++)
                    {
                        for (int k = 0; k < transformationsSequence.Length; k++)
                        {
                            sequence = transformationsSequence[k] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                                 : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[k]));
                        }
                    }

                    int characteristicLinkId = characteristicLinkIds[j];
                    Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);

                    IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                    characteristics[j] = calculator.Calculate(sequence, link);
                }

                sequencesCharacteristics[i] = new SequenceCharacteristics
                {
                    MatterName = mattersNames[matterId],
                    Characteristics = characteristics
                };
            }

            string[] characteristicNames = new string[characteristicLinkIds.Length];
            var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
            for (int k = 0; k < characteristicLinkIds.Length; k++)
            {
                characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                characteristicsList[k] = new SelectListItem
                {
                    Value = k.ToString(),
                    Text = characteristicNames[k],
                    Selected = false
                };
            }

            var transformations = transformationsSequence.Select(ts => ts.GetDisplayValue());

            var result = new Dictionary<string, object>
            {
                { "characteristics", sequencesCharacteristics },
                { "characteristicNames", characteristicNames },
                { "characteristicsList", characteristicsList },
                { "transformationsList", transformations },
                { "iterationsCount", iterationsCount }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
