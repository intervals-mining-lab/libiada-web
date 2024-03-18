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
    private readonly IViewDataHelper viewDataHelper;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicRepository;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;
    private readonly ICongenericSequencesCharacteristicsCalculator congenericSequencesCharacteristicsCalculator;
    private readonly Cache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CongenericCalculationController"/> class.
    /// </summary>
    public CongenericCalculationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                           IViewDataHelper viewDataHelper,
                                           ITaskManager taskManager,
                                           ICongenericCharacteristicRepository congenericCharacteristicRepository,
                                           ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                           ICongenericSequencesCharacteristicsCalculator congenericSequencesCharacteristicsCalculator,
                                           Cache cache)
        : base(TaskType.CongenericCalculation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.congenericCharacteristicRepository = congenericCharacteristicRepository;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
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
        var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Congeneric, 1, int.MaxValue, "Calculate");
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
    [ValidateAntiForgeryToken]
    public ActionResult Index(
        long[] matterIds,
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
            var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
            Dictionary<long, string> mattersNames;
            long[][] sequenceIds;

            mattersNames = cache.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds, notations, languages, translators, pauseTreatments, sequentialTransfers, trajectories);

            //// characteristics names
            //for (int k = 0; k < characteristicLinkIds.Length; k++)
            //{
            //    string characteristicType = congenericCharacteristicRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
            //    if (isLiteratureSequence)
            //    {
            //        Language language = languages[k];
            //        characteristicNames.Add(characteristicType + " " + language.GetDisplayValue());
            //    }
            //    else
            //    {
            //        characteristicNames.Add(characteristicType);
            //    }
            //}

            var characteristics = congenericSequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);
            
            using var db = dbFactory.CreateDbContext();
            var flatSequenceIds = sequenceIds.SelectMany(si => si);
            var elementIds = db.CommonSequences
                                .Where(cs => flatSequenceIds.Contains(cs.Id))
                                .Select(cs => cs.Alphabet)
                                .ToArray()
                                .SelectMany(a => a)
                                .Distinct();

            var unitedAlphabet = db.Elements.Where(e => elementIds.Contains(e.Id)).Select(e => new { e.Id, Name = e.Name ?? e.Value }).ToArray();
            int characteristicsCount = unitedAlphabet.Length * characteristicLinkIds.Length;
            for (int i = 0; i < matterIds.Length; i++)
            {
                var characteristicsValues = new double[characteristicsCount];

                for(int j = 0; j < unitedAlphabet.Length; j++)
                {
                    for(int k = 0; k < characteristicLinkIds.Length; k++)
                    {
                        bool hasValue = characteristics[sequenceIds[i][k]].TryGetValue((characteristicLinkIds[k], unitedAlphabet[j].Id), out double value);
                        characteristicsValues[j * characteristicLinkIds.Length + k] = hasValue ? value : double.NaN;
                    }
                }

                sequencesCharacteristics[i] = new SequenceCharacteristics
                {
                    MatterName = mattersNames[matterIds[i]],
                    Characteristics = characteristicsValues
                };
            }

            var characteristicNames = new string[characteristicsCount];
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

            var theoreticalRanks = new List<List<List<double>>>();

            // cycle through matters; first level of characteristics array
            for (int w = 0; w < matterIds.Length; w++)
            {
                long matterId = matterIds[w];
                theoreticalRanks.Add(new List<List<double>>());

                // cycle through characteristics and notations; second level of characteristics array
                for (int i = 0; i < characteristicLinkIds.Length; i++)
                {
                    Notation notation = notations[i];

                    long sequenceId;
                    if (cache.Matters.Single(m => m.Id == matterId).Nature == Nature.Literature)
                    {
                        Language language = languages[i];
                        Translator? translator = translators[i];

                        sequenceId = db.LiteratureSequences.Single(l => l.MatterId == matterId
                                                                  && l.Notation == notation
                                                                  && l.Language == language
                                                                  && translator == l.Translator).Id;
                    }
                    else
                    {
                        sequenceId = db.CommonSequences.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                    }

                    Chain chain = commonSequenceRepository.GetLibiadaChain(sequenceId);

                    // theoretical frequencies of orlov criterion
                    if (theoretical)
                    {
                        theoreticalRanks[w].Add(new List<double>());
                        ICongenericCalculator countCalculator = CongenericCalculatorsFactory.CreateCalculator(CongenericCharacteristic.ElementsCount);
                        var counts = new List<int>();
                        for (int f = 0; f < chain.Alphabet.Cardinality; f++)
                        {
                            counts.Add((int)countCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied));
                        }

                        ICongenericCalculator frequencyCalculator = CongenericCalculatorsFactory.CreateCalculator(CongenericCharacteristic.Probability);
                        var frequency = new List<double>();
                        for (int f = 0; f < chain.Alphabet.Cardinality; f++)
                        {
                            frequency.Add(frequencyCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied));
                        }

                        double maxFrequency = frequency.Max();
                        double k = 1 / System.Math.Log(counts.Max());
                        double b = (k / maxFrequency) - 1;
                        int n = 1;
                        double plow = chain.Length;
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
        arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
    }
}
