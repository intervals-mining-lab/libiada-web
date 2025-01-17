namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Music;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Newtonsoft.Json;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Calculators;
using Libiada.Database.Tasks;


/// <summary>
/// The calculation controller.
/// </summary>
[Authorize]
public class CalculationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly Cache cache;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationController"/> class.
    /// </summary>
    public CalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                 IViewDataHelper viewDataHelper,
                                 ITaskManager taskManager,
                                 Cache cache, 
                                 IFullCharacteristicRepository characteristicTypeLinkRepository,
                                 ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                 ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory)
        : base(TaskType.Calculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.cache = cache;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.sequencesCharacteristicsCalculator = sequencesCharacteristicsCalculator;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
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
    /// The matters ids.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// The characteristic type and link ids.
    /// </param>
    /// <param name="notations">
    /// The notations ids.
    /// </param>
    /// <param name="languages">
    /// The languages ids.
    /// </param>
    /// <param name="translators">
    /// The translators ids.
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
    /// <param name="rotate">
    /// Rotation flag.
    /// </param>
    /// <param name="complementary">
    /// Complement flag.
    /// </param>
    /// <param name="rotationLength">
    /// The rotation length.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(
        string tableType,
        long[] matterIds,
        int[] sequenceGroupIds,
        short[] characteristicLinkIds,
        Notation[] notations,
        Language[] languages,
        Translator[] translators,
        PauseTreatment[] pauseTreatments,
        bool[] sequentialTransfers,
        ImageOrderExtractor[] trajectories,
        bool rotate,
        bool complementary,
        uint? rotationLength)
    {
        return CreateTask(() =>
        {
            IEnumerable<SelectListItem>? sequenceGroupsSelectList = null;
            Dictionary<long, int>? mattersIdsSequenceGroupIds = null;
            if (tableType.Equals("sequenceGroups"))
            {
                using var db = dbFactory.CreateDbContext();
                SequenceGroup[] sequenceGroups = db.SequenceGroups.Where(sg => sequenceGroupIds.Contains(sg.Id)).Include(sg => sg.Matters).ToArray();
                matterIds = sequenceGroups.Select(sg => sg.Matters.Select(m => m.Id)).SelectMany(m => m).ToArray();
                int distinctMattersCount = matterIds.Distinct().ToArray().Length;
                if (matterIds.Length != distinctMattersCount) throw new ArgumentException("Sequence groups contain intesecting sets of sequences", nameof(sequenceGroupIds));

                mattersIdsSequenceGroupIds = sequenceGroups.SelectMany(sg => sg.Matters.Select(m => new { id = sg.Id, matterId = m.Id }))
                                                           .ToDictionary(sg => sg.matterId, sg => sg.id);

                sequenceGroupsSelectList = SelectListHelper.GetSequenceGroupSelectList(sg => sequenceGroupIds.Contains(sg.Id), db);
            }

            using var sequenceRepository = sequenceRepositoryFactory.Create();
            long[][] sequenceIds;
            sequenceIds = sequenceRepository.GetSequenceIds(matterIds,
                                                            notations,
                                                            languages,
                                                            translators,
                                                            pauseTreatments,
                                                            sequentialTransfers,
                                                            trajectories);
            Dictionary<long, string> mattersNames = cache.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

            double[][] characteristics;
            if (!rotate && !complementary)
            {
                characteristics = sequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);
            }
            else
            {
                characteristics = sequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds, rotate, complementary, rotationLength);
            }

            var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
            for (int i = 0; i < matterIds.Length; i++)
            {
                sequencesCharacteristics[i] = new SequenceCharacteristics
                {
                    MatterName = mattersNames[matterIds[i]],
                    SequenceGroupId = mattersIdsSequenceGroupIds?[matterIds[i]],
                    Characteristics = characteristics[i]
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

            var result = new Dictionary<string, object>
            {
                    { "characteristics", sequencesCharacteristics },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList },

            };

            if (sequenceGroupsSelectList is not null) result.Add("sequenceGroups", sequenceGroupsSelectList);
            

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
