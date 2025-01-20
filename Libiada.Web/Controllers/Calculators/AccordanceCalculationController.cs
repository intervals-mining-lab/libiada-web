namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;
using Libiada.Core.Music;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Tasks;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

using Newtonsoft.Json;

/// <summary>
/// The accordance calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class AccordanceCalculationController : AbstractResultController
{
    /// <summary>
    /// The database context factory.
    /// </summary>
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// The sequence repository factory.
    /// </summary>
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly Cache cache;

    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    private readonly IAccordanceCharacteristicRepository characteristicTypeLinkRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccordanceCalculationController"/> class.
    /// </summary>
    public AccordanceCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                           IViewDataHelper viewDataHelper, 
                                           ITaskManager taskManager,
                                           IAccordanceCharacteristicRepository characteristicTypeLinkRepository,
                                           ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                           Cache cache)
        : base(TaskType.AccordanceCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.cache = cache;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Accordance, 2, 2, "Calculate");
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterIds">
    /// The matter ids.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic type and link id.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <param name="language">
    /// The language id.
    /// </param>
    /// <param name="translator">
    /// The translator id.
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
    /// <param name="calculationType">
    /// The calculation type.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if count of matter ids is not 2.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown alphabets of sequences are not equal.
    /// </exception>
    [HttpPost]
    public ActionResult Index(
        long[] matterIds,
        int characteristicLinkId,
        Notation notation,
        Language? language,
        Translator? translator,
        PauseTreatment? pauseTreatment,
        bool? sequentialTransfer,
        ImageOrderExtractor? trajectory,
        string calculationType)
    {
        return CreateTask(() =>
        {
            if (matterIds.Length != 2)
            {
                throw new ArgumentException("Number of selected matters must be 2.", nameof(matterIds));
            }

            var characteristics = new Dictionary<int, Dictionary<int, double>>();
            string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);
            var result = new Dictionary<string, object>
                             {
                                 { "characteristics", characteristics },
                                 { "matterNames", cache.Matters.Where(m => matterIds.Contains(m.Id)).Select(m => m.Name).ToList() },
                                 { "characteristicName", characteristicName },
                                 { "calculationType", calculationType }
                             };
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            long[] sequenceIds = sequenceRepository.GetSequenceIds(matterIds,
                                                                      notation,
                                                                      language,
                                                                      translator,
                                                                      pauseTreatment,
                                                                      sequentialTransfer,
                                                                      trajectory);

            ComposedSequence firstSequence = sequenceRepository.GetLibiadaComposedSequence(sequenceIds[0]);
            ComposedSequence secondSequence = sequenceRepository.GetLibiadaComposedSequence(sequenceIds[1]);

            AccordanceCharacteristic accordanceCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            IAccordanceCalculator calculator = AccordanceCalculatorsFactory.CreateCalculator(accordanceCharacteristic);
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            Alphabet firstSequenceAlphabet = firstSequence.Alphabet;
            Alphabet secondSequenceAlphabet = secondSequence.Alphabet;

            switch (calculationType)
            {
                case "Equality":
                    if (!firstSequenceAlphabet.SetEquals(secondSequenceAlphabet))
                    {
                        throw new Exception("Alphabets of sequences are not equal.");
                    }

                    characteristics.Add(0, []);
                    characteristics.Add(1, []);
                    List<string> alphabet = [];

                    for (int i = 0; i < firstSequenceAlphabet.Cardinality; i++)
                    {
                        IBaseObject element = firstSequenceAlphabet[i];
                        alphabet.Add(element.ToString());

                        CongenericSequence firstCongenericSequence = firstSequence.CongenericSequence(element);
                        CongenericSequence secondCongenericSequence = secondSequence.CongenericSequence(element);

                        double characteristicValue = calculator.Calculate(firstCongenericSequence, secondCongenericSequence, link);
                        characteristics[0].Add(i, characteristicValue);

                        characteristicValue = calculator.Calculate(secondCongenericSequence, firstCongenericSequence, link);
                        characteristics[1].Add(i, characteristicValue);
                    }

                    result.Add("alphabet", alphabet);
                    break;

                case "All":
                    List<string> firstAlphabet = [];
                    for (int i = 0; i < firstSequence.Alphabet.Cardinality; i++)
                    {
                        characteristics.Add(i, []);
                        IBaseObject firstElement = firstSequenceAlphabet[i];
                        firstAlphabet.Add(firstElement.ToString());
                        for (int j = 0; j < secondSequenceAlphabet.Cardinality; j++)
                        {
                            IBaseObject secondElement = secondSequenceAlphabet[j];

                            CongenericSequence firstCongenericSequence = firstSequence.CongenericSequence(firstElement);
                            CongenericSequence secondCongenericSequence = secondSequence.CongenericSequence(secondElement);

                            double characteristicValue = calculator.Calculate(firstCongenericSequence, secondCongenericSequence, link);
                            characteristics[i].Add(j, characteristicValue);
                        }
                    }

                    List<string> secondAlphabet = [];
                    for (int j = 0; j < secondSequenceAlphabet.Cardinality; j++)
                    {
                        secondAlphabet.Add(secondSequenceAlphabet[j].ToString());
                    }

                    result.Add("firstAlphabet", firstAlphabet);
                    result.Add("secondAlphabet", secondAlphabet);
                    break;

                case "Specified":
                    throw new NotImplementedException();

                default:
                    throw new ArgumentException("Calculation type is not implemented", nameof(calculationType));
            }

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
