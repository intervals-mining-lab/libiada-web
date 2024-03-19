namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;
using Libiada.Core.Extensions;
using Libiada.Core.Music;

using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Catalogs;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

/// <summary>
/// The relation calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class RelationCalculationController : AbstractResultController
{
    /// <summary>
    /// Database context.
    /// </summary>
    private readonly LibiadaDatabaseEntities db; 

    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IBinaryCharacteristicRepository characteristicTypeLinkRepository;
    private readonly Cache cache;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationCalculationController"/> class.
    /// </summary>
    public RelationCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                         IViewDataHelper viewDataHelper, 
                                         ITaskManager taskManager, 
                                         ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                         IBinaryCharacteristicRepository characteristicTypeLinkRepository,
                                         Cache cache)
        : base(TaskType.RelationCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.db = dbFactory.CreateDbContext();
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.cache = cache;
        this.viewDataHelper = viewDataHelper;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(CharacteristicCategory.Binary, 1, 1, "Calculate"));
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterId">
    /// The matter id.
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
    /// <param name="filterSize">
    /// The filter size.
    /// </param>
    /// <param name="filter">
    /// The filter.
    /// </param>
    /// <param name="frequencyFilter">
    /// The frequency filter.
    /// </param>
    /// <param name="frequencyCount">
    /// The frequency count.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(
        long matterId,
        short characteristicLinkId,
        Notation notation,
        Language? language,
        Translator? translator,
        PauseTreatment? pauseTreatment,
        bool? sequentialTransfer,
        ImageOrderExtractor? trajectory,
        int filterSize,
        bool filter,
        bool frequencyFilter,
        int frequencyCount)
    {
        return CreateTask(() =>
        {
            string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);
            Matter matter = cache.Matters.Single(m => m.Id == matterId);
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            long sequenceId = commonSequenceRepository.GetSequenceIds([matterId], 
                                                                      notation, 
                                                                      language, 
                                                                      translator, 
                                                                      pauseTreatment, 
                                                                      sequentialTransfer,
                                                                      trajectory).Single();

            Chain currentChain = commonSequenceRepository.GetLibiadaChain(sequenceId);
            CommonSequence sequence = db.CommonSequences.Include(cs => cs.Matter).Single(m => m.Id == sequenceId);

            var result = new Dictionary<string, object>
            {
                { "isFilter", filter },
                { "matterName", sequence.Matter.Name },
                { "notationName", sequence.Notation.GetDisplayValue() },
                { "characteristicName", characteristicName }
            };

            BinaryCharacteristic binaryCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            IBinaryCalculator calculator = BinaryCalculatorsFactory.CreateCalculator(binaryCharacteristic);
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

            if (frequencyFilter)
            {
                CalculateFrequencyCharacteristics(characteristicLinkId, frequencyCount, currentChain, sequenceId, calculator, link);
            }
            else
            {
                CalculateAllCharacteristics(characteristicLinkId, sequenceId, currentChain, calculator, link);
            }

            if (filter)
            {
                var filteredResult = db.BinaryCharacteristicValues
                                       .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
                                       .OrderByDescending(b => b.Value)
                                       .Take(filterSize)
                                       .Select(rc => new { rc.FirstElementId, rc.SecondElementId, rc.Value })
                                       .ToArray();

                List<string> firstElements = [];
                List<string> secondElements = [];
                for (int i = 0; i < filterSize; i++)
                {
                    long firstElementId = filteredResult[i].FirstElementId;
                    Element firstElement = db.Elements.Single(e => e.Id == firstElementId);
                    firstElements.Add(firstElement.Name ?? firstElement.Value);

                    long secondElementId = filteredResult[i].SecondElementId;
                    Element secondElement = db.Elements.Single(e => e.Id == secondElementId);
                    secondElements.Add(secondElement.Name ?? secondElement.Value);
                }

                result.Add("filteredResult", filteredResult);
                result.Add("filterSize", filterSize);
                result.Add("firstElements", firstElements);
                result.Add("secondElements", secondElements);
            }
            else
            {
                var characteristics = db.BinaryCharacteristicValues
                                        .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
                                        .GroupBy(b => b.FirstElementId)
                                        .ToDictionary(b => b.Key, b => b.ToDictionary(bb => bb.SecondElementId, bb => bb.Value));
                long[] elementsIds = db.CommonSequences.Single(cs => cs.Id == sequenceId).Alphabet;
                var elements = db.Elements
                                 .Where(e => elementsIds.Contains(e.Id))
                                 .OrderBy(e => e.Id)
                                 .Select(e => new { Name = e.Name ?? e.Value, e.Id })
                                 .ToArray();
                
                result.Add("characteristics", characteristics);
                result.Add("elements", elements);
            }

            string json = JsonConvert.SerializeObject(result);

            return new Dictionary<string, string> { { "data", json } };
        });
    }

    /// <summary>
    /// The not frequency characteristic.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type and link id.
    /// </param>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <param name="chain">
    /// The chain.
    /// </param>
    /// <param name="calculator">
    /// The calculator.
    /// </param>
    /// <param name="link">
    /// The link.
    /// </param>
    [NonAction]
    private void CalculateAllCharacteristics(short characteristicLinkId, long sequenceId, Chain chain, IBinaryCalculator calculator, Link link)
    {
        List<BinaryCharacteristicValue> newCharacteristics = [];
        BinaryCharacteristicValue[] databaseCharacteristics = db.BinaryCharacteristicValues
            .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
            .ToArray();
        int calculatedCount = databaseCharacteristics.Length;
        int alphabetCardinality = chain.Alphabet.Cardinality;

        if (calculatedCount < alphabetCardinality * alphabetCardinality)
        {
            long[] sequenceElements = db.CommonSequences.Single(cs => cs.Id == sequenceId).Alphabet;
            for (int i = 0; i < alphabetCardinality; i++)
            {
                for (int j = 0; j < alphabetCardinality; j++)
                {
                    long firstElementId = sequenceElements[i];
                    long secondElementId = sequenceElements[j];
                    if (i != j && !databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                    {
                        double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                        newCharacteristics.Add(characteristicTypeLinkRepository.CreateCharacteristic(sequenceId, characteristicLinkId, firstElementId, secondElementId, result));
                    }
                }
            }
        }

        db.BinaryCharacteristicValues.AddRange(newCharacteristics);
        db.SaveChanges();
    }

    /// <summary>
    /// The frequency characteristic.
    /// </summary>
    /// <param name="characteristicLinkId">
    /// The characteristic type and link id.
    /// </param>
    /// <param name="frequencyCount">
    /// The frequency count.
    /// </param>
    /// <param name="chain">
    /// The chain.
    /// </param>
    /// <param name="sequenceId">
    /// The sequence id.
    /// </param>
    /// <param name="calculator">
    /// The calculator.
    /// </param>
    /// <param name="link">
    /// The link.
    /// </param>
    [NonAction]
    private void CalculateFrequencyCharacteristics(short characteristicLinkId, int frequencyCount, Chain chain, long sequenceId, IBinaryCalculator calculator, Link link)
    {
        long[] sequenceElements = db.CommonSequences.Single(cs => cs.Id == sequenceId).Alphabet;
        List<BinaryCharacteristicValue> newCharacteristics = [];
        BinaryCharacteristicValue[] databaseCharacteristics = db.BinaryCharacteristicValues
            .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
            .ToArray();

        // calculating frequencies of elements in alphabet
        Alphabet alphabet = chain.Alphabet;
        var frequencies = new (IBaseObject element, double frequency)[alphabet.Cardinality];
        for (int f = 0; f < alphabet.Cardinality; f++)
        {
            var probabilityCalculator = new Probability();
            double result = probabilityCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied);
            frequencies[f] = (alphabet[f], result);
        }

        // ordering alphabet by frequencies
        Array.Sort(frequencies, (x,y) => x.frequency.CompareTo(y.frequency));

        // calculating relation characteristic only for elements with maximum frequency
        for (int i = 0; i < frequencyCount; i++)
        {
            for (int j = 0; j < frequencyCount; j++)
            {
                int firstElementNumber = alphabet.IndexOf(frequencies[i].element) + 1;
                int secondElementNumber = alphabet.IndexOf(frequencies[j].element) + 1;
                long firstElementId = sequenceElements[firstElementNumber];
                long secondElementId = sequenceElements[secondElementNumber];

                // searching characteristic in database
                if (!databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                {
                    double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                    newCharacteristics.Add(characteristicTypeLinkRepository.CreateCharacteristic(sequenceId, characteristicLinkId, firstElementId, secondElementId, result));
                }
            }
        }

        db.BinaryCharacteristicValues.AddRange(newCharacteristics);
        db.SaveChanges();
    }
}
