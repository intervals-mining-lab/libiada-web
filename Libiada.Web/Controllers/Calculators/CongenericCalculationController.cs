namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;
using Libiada.Core.Music;

using Newtonsoft.Json;

using Libiada.Database.Tasks;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.CalculatorsData;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using System.Linq;

/// <summary>
/// The congeneric calculation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class CongenericCalculationController : AbstractResultController
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicRepository;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;
    private readonly ICongenericSequencesCharacteristicsCalculator congenericSequencesCharacteristicsCalculator;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CongenericCalculationController"/> class.
    /// </summary>
    public CongenericCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                           IViewDataBuilder viewDataBuilder,
                                           ITaskManager taskManager,
                                           ICongenericCharacteristicRepository congenericCharacteristicRepository,
                                           ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                           ICongenericSequencesCharacteristicsCalculator congenericSequencesCharacteristicsCalculator,
                                           IResearchObjectsCache cache)
        : base(TaskType.CongenericCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataBuilder = viewDataBuilder;
        this.congenericCharacteristicRepository = congenericCharacteristicRepository;
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        this.congenericSequencesCharacteristicsCalculator = congenericSequencesCharacteristicsCalculator;
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
        var viewData = viewDataBuilder.AddMinMaxResearchObjects()
                                      .AddSequenceGroups()
                                      .AddNatures()
                                      .AddNotations()
                                      .AddLanguages()
                                      .AddTranslators()
                                      .AddPauseTreatments()
                                      .AddTrajectories()
                                      .AddSequenceTypes()
                                      .AddGroups()
                                      .AddCharacteristicsData(CharacteristicCategory.Congeneric)
                                      .Build();
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The research object ids.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// The characteristic type and link ids.
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
    /// <param name="imageOrderExtractor">
    /// Image order extractor of image sequences.
    /// </param>
    /// <param name="sequentialTransfers">
    /// Sequential transfer flag used in music sequences.
    /// </param>
    /// <param name="sort">
    /// The is sort.
    /// </param>
    /// <param name="theoretical">
    /// The theoretical.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(
        long[] researchObjectIds,
        short[] characteristicLinkIds,
        Notation[] notations,
        Language[] languages,
        Translator[] translators,
        PauseTreatment[] pauseTreatments,
        bool[] sequentialTransfers,
        ImageOrderExtractor[] trajectories,
        bool theoretical)
    {
        return CreateTask(() =>
        {
            var sequencesCharacteristics = new SequenceCharacteristics[researchObjectIds.Length];
            Dictionary<long, string> researchObjectsNames;
            long[][] sequenceIds;

            researchObjectsNames = cache.ResearchObjects.Where(m => researchObjectIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            sequenceIds = sequenceRepository.GetSequenceIds(researchObjectIds, notations, languages, translators, pauseTreatments, sequentialTransfers, trajectories);

            //// characteristics names
            //for (int k = 0; k < characteristicLinkIds.Length; k++)
            //{
            //    string characteristicType = congenericCharacteristicRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
            //    if (isLiteratureSequence)
            //    {
            //        Language language = languages[k];
            //        characteristicNames.Add($"{characteristicType} {language.GetDisplayValue()}");
            //    }
            //    else
            //    {
            //        characteristicNames.Add(characteristicType);
            //    }
            //}

            var characteristics = congenericSequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);

            using var db = dbFactory.CreateDbContext();
            var flatSequenceIds = sequenceIds.SelectMany(si => si);
            var elementIds = db.CombinedSequenceEntities
                                .Where(cs => flatSequenceIds.Contains(cs.Id))
                                .Select(cs => cs.Alphabet)
                                .ToArray()
                                .SelectMany(a => a)
                                .Distinct()
                                .ToArray();

            Element[] elements = new ElementRepository(db).GetElements(elementIds);

            var unitedAlphabet = elements.Select(e => new { e.Id, Name = e.Name ?? e.Value }).ToArray();
            int characteristicsCount = unitedAlphabet.Length * characteristicLinkIds.Length;
            for (int i = 0; i < researchObjectIds.Length; i++)
            {
                double[] characteristicsValues = new double[characteristicsCount];

                for (int j = 0; j < unitedAlphabet.Length; j++)
                {
                    for (int k = 0; k < characteristicLinkIds.Length; k++)
                    {
                        bool hasValue = characteristics[sequenceIds[i][k]].TryGetValue((characteristicLinkIds[k], unitedAlphabet[j].Id), out double value);
                        characteristicsValues[j * characteristicLinkIds.Length + k] = hasValue ? value : double.NaN;
                    }
                }

                sequencesCharacteristics[i] = new SequenceCharacteristics
                {
                    ResearchObjectName = researchObjectsNames[researchObjectIds[i]],
                    Characteristics = characteristicsValues
                };
            }

            string[] characteristicNames = new string[characteristicsCount];
            var characteristicsList = new SelectListItem[characteristicsCount];
            for (int j = 0; j < unitedAlphabet.Length; j++)
            {
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    int index = j * characteristicLinkIds.Length + k;
                    string characteristicName = congenericCharacteristicRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                    string elementName = unitedAlphabet[j].Name;
                    characteristicNames[index] = $"{characteristicName} {elementName}";
                    characteristicsList[index] = new SelectListItem
                    {
                        Value = index.ToString(),
                        Text = characteristicNames[index],
                        Selected = false
                    };
                }
            }

            List<List<List<double>>> theoreticalRanks = [];

            // cycle through research objects; first level of characteristics array
            for (int w = 0; w < researchObjectIds.Length; w++)
            {
                long researchObjectId = researchObjectIds[w];
                theoreticalRanks.Add([]);

                // cycle through characteristics and notations; second level of characteristics array
                for (int i = 0; i < characteristicLinkIds.Length; i++)
                {
                    Notation notation = notations[i];

                    long sequenceId;
                    if (cache.ResearchObjects.Single(m => m.Id == researchObjectId).Nature == Nature.Literature)
                    {
                        Language language = languages[i];
                        Translator translator = translators[i];

                        sequenceId = db.CombinedSequenceEntities.Single(l => l.ResearchObjectId == researchObjectId
                                                                  && l.Notation == notation
                                                                  && l.Language == language
                                                                  && translator == l.Translator).Id;
                    }
                    else
                    {
                        sequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId && c.Notation == notation).Id;
                    }

                    ComposedSequence sequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);

                    // theoretical frequencies of orlov criterion
                    if (theoretical)
                    {
                        theoreticalRanks[w].Add([]);
                        ICongenericCalculator countCalculator = CongenericCalculatorsFactory.CreateCalculator(CongenericCharacteristic.ElementsCount);
                        List<int> counts = [];
                        for (int f = 0; f < sequence.Alphabet.Cardinality; f++)
                        {
                            counts.Add((int)countCalculator.Calculate(sequence.CongenericSequence(f), Link.NotApplied));
                        }

                        ICongenericCalculator frequencyCalculator = CongenericCalculatorsFactory.CreateCalculator(CongenericCharacteristic.Probability);
                        List<double> frequency = [];
                        for (int f = 0; f < sequence.Alphabet.Cardinality; f++)
                        {
                            frequency.Add(frequencyCalculator.Calculate(sequence.CongenericSequence(f), Link.NotApplied));
                        }

                        double maxFrequency = frequency.Max();
                        // TODO: check if this should be Log2
                        double k = 1 / System.Math.Log(counts.Max());
                        double b = (k / maxFrequency) - 1;
                        int n = 1;
                        double plow = sequence.Length;
                        double p = k / (b + n);
                        while (p >= (1 / plow))
                        {
                            theoreticalRanks.Last().Last().Add(p);
                            n++;
                            p = k / (b + n);
                        }
                    }
                }
            }

            var result = new Dictionary<string, object>
            {
                { "characteristics", sequencesCharacteristics },
                { "characteristicNames", characteristicNames },
                { "theoreticalRanks", theoreticalRanks },
                { "characteristicsList", characteristicsList }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }


    /// <summary>
    /// The sort key value pair list.
    /// </summary>
    /// <param name="arrayForSort">
    /// The array for sort.
    /// </param>
    [NonAction]
    private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
    {
        //TODO: refactor this to use tuples
        arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
    }
}
