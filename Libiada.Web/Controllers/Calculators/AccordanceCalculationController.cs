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
    /// The common sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;
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
                                           ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                           Cache cache)
        : base(TaskType.AccordanceCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
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
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            long[] sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds,
                                                                      notation,
                                                                      language,
                                                                      translator,
                                                                      pauseTreatment,
                                                                      sequentialTransfer,
                                                                      trajectory);

            Chain firstChain = commonSequenceRepository.GetLibiadaChain(sequenceIds[0]);
            Chain secondChain = commonSequenceRepository.GetLibiadaChain(sequenceIds[1]);

            AccordanceCharacteristic accordanceCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            IAccordanceCalculator calculator = AccordanceCalculatorsFactory.CreateCalculator(accordanceCharacteristic);
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
            Alphabet firstChainAlphabet = firstChain.Alphabet;
            Alphabet secondChainAlphabet = secondChain.Alphabet;

            switch (calculationType)
            {
                case "Equality":
                    if (!firstChainAlphabet.SetEquals(secondChainAlphabet))
                    {
                        throw new Exception("Alphabets of sequences are not equal.");
                    }

                    characteristics.Add(0, []);
                    characteristics.Add(1, []);
                    List<string> alphabet = [];

                    for (int i = 0; i < firstChainAlphabet.Cardinality; i++)
                    {
                        IBaseObject element = firstChainAlphabet[i];
                        alphabet.Add(element.ToString());

                        CongenericChain firstCongenericChain = firstChain.CongenericChain(element);
                        CongenericChain secondCongenericChain = secondChain.CongenericChain(element);

                        double characteristicValue = calculator.Calculate(firstCongenericChain, secondCongenericChain, link);
                        characteristics[0].Add(i, characteristicValue);

                        characteristicValue = calculator.Calculate(secondCongenericChain, firstCongenericChain, link);
                        characteristics[1].Add(i, characteristicValue);
                    }

                    result.Add("alphabet", alphabet);
                    break;

                case "All":
                    List<string> firstAlphabet = [];
                    for (int i = 0; i < firstChain.Alphabet.Cardinality; i++)
                    {
                        characteristics.Add(i, []);
                        IBaseObject firstElement = firstChainAlphabet[i];
                        firstAlphabet.Add(firstElement.ToString());
                        for (int j = 0; j < secondChainAlphabet.Cardinality; j++)
                        {
                            IBaseObject secondElement = secondChainAlphabet[j];

                            CongenericChain firstCongenericChain = firstChain.CongenericChain(firstElement);
                            CongenericChain secondCongenericChain = secondChain.CongenericChain(secondElement);

                            double characteristicValue = calculator.Calculate(firstCongenericChain, secondCongenericChain, link);
                            characteristics[i].Add(j, characteristicValue);
                        }
                    }

                    List<string> secondAlphabet = [];
                    for (int j = 0; j < secondChainAlphabet.Cardinality; j++)
                    {
                        secondAlphabet.Add(secondChainAlphabet[j].ToString());
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
