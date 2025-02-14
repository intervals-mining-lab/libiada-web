namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.DataTransformers;
using Libiada.Core.Extensions;
using Libiada.Core.Music;

using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

/// <summary>
/// The order transformation calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrderTransformationCharacteristicsDynamicVisualizationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformationCharacteristicsDynamicVisualizationController"/> class.
    /// </summary>
    public OrderTransformationCharacteristicsDynamicVisualizationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                                                            IViewDataHelper viewDataHelper,
                                                                            ITaskManager taskManager,
                                                                            IFullCharacteristicRepository characteristicTypeLinkRepository,
                                                                            IResearchObjectsCache cache)
        : base(TaskType.OrderTransformationCharacteristicsDynamicVisualization, taskManager)
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
        Dictionary<string, object> data = viewDataHelper.AddMinMaxResearchObjects()
                                                        .AddSequenceGroups()
                                                        .AddNatures()
                                                        .AddNotations()
                                                        .AddLanguages()
                                                        .AddTranslators()
                                                        .AddPauseTreatments()
                                                        .AddTrajectories()
                                                        .AddOrderTransformations()
                                                        .AddSequenceTypes()
                                                        .AddGroups()
                                                        .AddSubmitName()
                                                        .AddCharacteristicsData(CharacteristicCategory.Full)
                                                        .Build();
        ViewBag.data = JsonConvert.SerializeObject(data);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The research objects ids.
    /// </param>
    /// <param name="transformationsSequence">
    /// The transformation ids.
    /// </param>
    /// <param name="iterationsCount">
    /// Number of transformations iterations.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic type link ids.
    /// </param>
    /// <param name="notation">
    /// The notation ids.
    /// </param>
    /// <param name="language">
    /// The language ids.
    /// </param>
    /// <param name="translator">
    /// The translator ids.
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
    public ActionResult Index(
        long[] researchObjectIds,
        OrderTransformation[] transformationsSequence,
        int iterationsCount,
        short characteristicLinkId,
        Notation notation,
        Language? language,
        Translator? translator,
        PauseTreatment? pauseTreatment,
        bool? sequentialTransfer,
        ImageOrderExtractor? trajectory)
    {
        return CreateTask(() =>
        {
            var sequenceRepository = new CombinedSequenceEntityRepository(dbFactory, cache);
            var researchObjectsCharacteristics = new object[researchObjectIds.Length];
            Array.Sort(researchObjectIds);
            Dictionary<long, ResearchObject> researchObjects = cache.ResearchObjects.Where(m => researchObjectIds.Contains(m.Id)).ToDictionary(m => m.Id);

            for (int i = 0; i < researchObjectIds.Length; i++)
            {
                long researchObjectId = researchObjectIds[i];
                long sequenceId = sequenceRepository.GetSequenceIds([researchObjectId],
                                                                          notation,
                                                                          language,
                                                                          translator,
                                                                          pauseTreatment,
                                                                          sequentialTransfer,
                                                                          trajectory).Single();

                Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                ComposedSequence sequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);

                double[] characteristics = new double[transformationsSequence.Length * iterationsCount];
                for (int j = 0; j < iterationsCount; j++)
                {
                    for (int k = 0; k < transformationsSequence.Length; k++)
                    {
                        sequence = transformationsSequence[k] == OrderTransformation.Dissimilar ? DissimilarSequenceFactory.Create(sequence)
                                                             : HighOrderFactory.Create(sequence, transformationsSequence[k].GetLink());
                        characteristics[transformationsSequence.Length * j + k] = calculator.Calculate(sequence, link);
                    }
                }

                researchObjectsCharacteristics[i] = new { researchObjectName = researchObjects[researchObjectId].Name, characteristics };
            }

            string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);


            var result = new Dictionary<string, object>
                             {
                                 { "characteristics", researchObjectsCharacteristics },
                                 { "characteristicName", characteristicName },
                                 { "transformationsList", transformationsSequence.Select(ts => ts.GetDisplayValue()) },
                                 { "iterationsCount", iterationsCount }
                             };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
