﻿namespace Libiada.Web.Controllers.Calculators;

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
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IBinaryCharacteristicRepository characteristicTypeLinkRepository;
    private readonly IResearchObjectsCache cache;
    private readonly IViewDataBuilder viewDataBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationCalculationController"/> class.
    /// </summary>
    public RelationCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                         IViewDataBuilder viewDataBuilder,
                                         ITaskManager taskManager,
                                         ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                         IBinaryCharacteristicRepository characteristicTypeLinkRepository,
                                         IResearchObjectsCache cache)
        : base(TaskType.RelationCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.db = dbFactory.CreateDbContext();
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.cache = cache;
        this.viewDataBuilder = viewDataBuilder;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var viewData = viewDataBuilder.AddMinMaxResearchObjects(1, 1)
                                      .AddNatures()
                                      .AddNotations()
                                      .AddLanguages()
                                      .AddTranslators()
                                      .AddPauseTreatments()
                                      .AddTrajectories()
                                      .AddSequenceTypes()
                                      .AddGroups()
                                      .AddCharacteristicsData(CharacteristicCategory.Binary)
                                      .Build();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectId">
    /// The research object id.
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
    public ActionResult Index(
        long researchObjectId,
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
            ResearchObject researchObject = cache.ResearchObjects.Single(m => m.Id == researchObjectId);
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            long sequenceId = sequenceRepository.GetSequenceIds([researchObjectId],
                                                                      notation,
                                                                      language,
                                                                      translator,
                                                                      pauseTreatment,
                                                                      sequentialTransfer,
                                                                      trajectory).Single();

            ComposedSequence currentSequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);
            CombinedSequenceEntity sequence = db.CombinedSequenceEntities.Include(cs => cs.ResearchObject).Single(m => m.Id == sequenceId);

            var result = new Dictionary<string, object>
            {
                { "isFilter", filter },
                { "researchObjectName", sequence.ResearchObject.Name },
                { "notationName", sequence.Notation.GetDisplayValue() },
                { "characteristicName", characteristicName }
            };

            BinaryCharacteristic binaryCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
            IBinaryCalculator calculator = BinaryCalculatorsFactory.CreateCalculator(binaryCharacteristic);
            Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

            if (frequencyFilter)
            {
                CalculateFrequencyCharacteristics(characteristicLinkId, frequencyCount, currentSequence, sequenceId, calculator, link);
            }
            else
            {
                CalculateAllCharacteristics(characteristicLinkId, sequenceId, currentSequence, calculator, link);
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

                ElementRepository elementRepository = new(db);
                long[] allElementIds = filteredResult.Select(fr => fr.FirstElementId)
                                                     .Union(filteredResult.Select(fr => fr.SecondElementId)).ToArray();
                Element[] allElements = elementRepository.GetElements(allElementIds);
                for (int i = 0; i < filterSize; i++)
                {
                    long firstElementId = filteredResult[i].FirstElementId;
                    Element firstElement = allElements.Single(e => e.Id == firstElementId);
                    firstElements.Add(firstElement.Name ?? firstElement.Value);

                    long secondElementId = filteredResult[i].SecondElementId;
                    Element secondElement = allElements.Single(e => e.Id == secondElementId);
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
                long[] elementIds = db.CombinedSequenceEntities.Single(cs => cs.Id == sequenceId).Alphabet;

                Element[] elements = new ElementRepository(db).GetElements(elementIds);

                result.Add("characteristics", characteristics);
                result.Add("elements", elements.Select(e => new { Name = e.Name ?? e.Value, e.Id }));
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
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <param name="calculator">
    /// The calculator.
    /// </param>
    /// <param name="link">
    /// The link.
    /// </param>
    [NonAction]
    private void CalculateAllCharacteristics(short characteristicLinkId, long sequenceId, ComposedSequence sequence, IBinaryCalculator calculator, Link link)
    {
        List<BinaryCharacteristicValue> newCharacteristics = [];
        BinaryCharacteristicValue[] databaseCharacteristics = db.BinaryCharacteristicValues
            .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
            .ToArray();
        int calculatedCount = databaseCharacteristics.Length;
        int alphabetCardinality = sequence.Alphabet.Cardinality;

        if (calculatedCount < alphabetCardinality * alphabetCardinality)
        {
            long[] sequenceElements = db.CombinedSequenceEntities.Single(cs => cs.Id == sequenceId).Alphabet;
            for (int i = 0; i < alphabetCardinality; i++)
            {
                for (int j = 0; j < alphabetCardinality; j++)
                {
                    long firstElementId = sequenceElements[i];
                    long secondElementId = sequenceElements[j];
                    if (i != j && !databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                    {
                        double result = calculator.Calculate(sequence.GetRelationIntervalsManager(i + 1, j + 1), link);

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
    /// <param name="sequence">
    /// The sequence.
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
    private void CalculateFrequencyCharacteristics(short characteristicLinkId, int frequencyCount, ComposedSequence sequence, long sequenceId, IBinaryCalculator calculator, Link link)
    {
        long[] sequenceElements = db.CombinedSequenceEntities.Single(cs => cs.Id == sequenceId).Alphabet;
        List<BinaryCharacteristicValue> newCharacteristics = [];
        BinaryCharacteristicValue[] databaseCharacteristics = db.BinaryCharacteristicValues
            .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
            .ToArray();

        // calculating frequencies of elements in alphabet
        Alphabet alphabet = sequence.Alphabet;
        var frequencies = new (IBaseObject element, double frequency)[alphabet.Cardinality];
        for (int f = 0; f < alphabet.Cardinality; f++)
        {
            var probabilityCalculator = new Probability();
            double result = probabilityCalculator.Calculate(sequence.CongenericSequence(f), Link.NotApplied);
            frequencies[f] = (alphabet[f], result);
        }

        // ordering alphabet by frequencies
        Array.Sort(frequencies, (x, y) => x.frequency.CompareTo(y.frequency));

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
                    double result = calculator.Calculate(sequence.GetRelationIntervalsManager(i + 1, j + 1), link);

                    newCharacteristics.Add(characteristicTypeLinkRepository.CreateCharacteristic(sequenceId, characteristicLinkId, firstElementId, secondElementId, result));
                }
            }
        }

        db.BinaryCharacteristicValues.AddRange(newCharacteristics);
        db.SaveChanges();
    }
}
