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
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformationCalculationController"/> class.
    /// </summary>
    public OrderTransformationCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                                    IViewDataBuilder viewDataBuilder,
                                                    ITaskManager taskManager,
                                                    IFullCharacteristicRepository characteristicTypeLinkRepository,
                                                    IResearchObjectsCache cache)
        : base(TaskType.OrderTransformationCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
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
        Dictionary<string, object> data = viewDataBuilder.AddMinMaxResearchObjects()
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
        long[] researchObjectIds,
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
            Dictionary<long, string> researchObjectsNames = cache.ResearchObjects.Where(m => researchObjectIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
            ComposedSequence[][] sequences = new ComposedSequence[researchObjectIds.Length][];

            var sequenceRepository = new CombinedSequenceEntityRepository(dbFactory, cache);
            long[][] sequenceIds = sequenceRepository.GetSequenceIds(researchObjectIds,
                                                                           notations,
                                                                           languages,
                                                                           translators,
                                                                           pauseTreatments,
                                                                           sequentialTransfers,
                                                                           trajectories);
            for (int i = 0; i < researchObjectIds.Length; i++)
            {
                sequences[i] = new ComposedSequence[characteristicLinkIds.Length];
                for (int j = 0; j < characteristicLinkIds.Length; j++)
                {
                    sequences[i][j] = sequenceRepository.GetLibiadaComposedSequence(sequenceIds[i][j]);
                }
            }

            var sequencesCharacteristics = new SequenceCharacteristics[researchObjectIds.Length];
            Array.Sort(researchObjectIds);

            for (int i = 0; i < researchObjectIds.Length; i++)
            {
                long researchObjectId = researchObjectIds[i];
                double[] characteristics = new double[characteristicLinkIds.Length];
                for (int j = 0; j < characteristicLinkIds.Length; j++)
                {
                    Notation notation = notations[j];


                    ComposedSequence sequence = sequences[i][j];
                    for (int l = 0; l < iterationsCount; l++)
                    {
                        for (int k = 0; k < transformationsSequence.Length; k++)
                        {
                            sequence = transformationsSequence[k] == OrderTransformation.Dissimilar ? DissimilarSequenceFactory.Create(sequence)
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
                    ResearchObjectName = researchObjectsNames[researchObjectId],
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
