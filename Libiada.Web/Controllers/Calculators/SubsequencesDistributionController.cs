namespace Libiada.Web.Controllers.Calculators;

using System.Net;
using System.Text;

using Newtonsoft.Json;

using Libiada.Core.Extensions;

using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Bio.IO.FastA;
using Bio;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;
using System.Net.Http;
using System.Xml;

/// <summary>
/// The subsequences distribution controller.
/// </summary>
[Authorize]
public class SubsequencesDistributionController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
    private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;
    private readonly Cache cache;
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// The base url for all alignment requests.
    /// </summary>
    private readonly Uri BaseAddress = new(@"https://www.ebi.ac.uk/Tools/services/rest/");

    /// <summary>
    /// Initializes a new instance of the <see cref="SubsequencesDistributionController"/> class.
    /// </summary>
    public SubsequencesDistributionController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                              IViewDataHelper viewDataHelper,
                                              ITaskManager taskManager,
                                              IFullCharacteristicRepository characteristicTypeLinkRepository,
                                              ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                              ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                              ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory,
                                              Cache cache,
                                              IHttpClientFactory httpClientFactory)
        : base(TaskType.SubsequencesDistribution, taskManager)
    {
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
        this.sequencesCharacteristicsCalculator = sequencesCharacteristicsCalculator;
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
        this.cache = cache;
        this.httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(1, int.MaxValue, "Calculate"));
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterIds">
    /// The matter ids.
    /// </param>
    /// <param name="characteristicLinkId">
    /// Full sequence characteristic type and link id.
    /// </param>
    /// <param name="characteristicLinkIds">
    /// Subsequences characteristics types and links ids.
    /// </param>
    /// <param name="features">
    /// The feature ids.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(long[] matterIds, short characteristicLinkId, short[] characteristicLinkIds, Feature[] features)
    {
        return CreateTask(() =>
        {
            Array.Sort(matterIds);
            using var db = dbFactory.CreateDbContext();
            var matterNames = new string[matterIds.Length];
            var remoteIds = new string?[matterIds.Length];
            var subsequencesCharacteristicsNames = new string[characteristicLinkIds.Length];
            var subsequencesCharacteristicsList = new SelectListItem[characteristicLinkIds.Length];
            var attributeValuesCache = new AttributeValueCacheManager(db);
            long[] sequenceIds;

            DnaSequence[] parentSequences = db.DnaSequences.Include(s => s.Matter)
                                    .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                    .OrderBy(s => s.MatterId)
                                    .ToArray();

            for (int n = 0; n < parentSequences.Length; n++)
            {
                matterNames[n] = parentSequences[n].Matter.Name;
                remoteIds[n] = parentSequences[n].RemoteId;
            }

            var geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
            sequenceIds = geneticSequenceRepository.GetNucleotideSequenceIds(matterIds);

            string sequenceCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId);

            for (int k = 0; k < characteristicLinkIds.Length; k++)
            {
                subsequencesCharacteristicsNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k]);
                subsequencesCharacteristicsList[k] = new SelectListItem
                {
                    Value = k.ToString(),
                    Text = subsequencesCharacteristicsNames[k],
                    Selected = false
                };
            }

            double[] characteristics = sequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkId);

            var sequencesData = new SequenceData[matterIds.Length];
            for (int i = 0; i < matterIds.Length; i++)
            {
                // all subsequence calculations
                SubsequenceData[] subsequencesData = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                    characteristicLinkIds,
                    features,
                    sequenceIds[i]);

                attributeValuesCache.FillAttributeValues(subsequencesData);

                sequencesData[i] = new SequenceData(matterIds[i], matterNames[i], remoteIds[i], characteristics[i], subsequencesData);
            }

            // sorting organisms by their characteristic
            Array.Sort(sequencesData, (x, y) => x.Characteristic.CompareTo(y.Characteristic));
            List<AttributeValue> allAttributeValues = attributeValuesCache.AllAttributeValues;
            var result = new Dictionary<string, object>(7)
                            {
                                { "result", sequencesData },
                                { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                                { "subsequencesCharacteristicsList", subsequencesCharacteristicsList },
                                { "sequenceCharacteristicName", sequenceCharacteristicName },
                                { "features", features.ToSelectList(features).ToDictionary(f => f.Value) },
                                { "attributes", EnumExtensions.ToArray<AnnotationAttribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                                { "attributeValues", allAttributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) }
                            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }


    /// <summary>
    /// Creates clustalw alignment job
    /// and returns it's id.
    /// </summary>
    /// <param name="subsequencesIds">
    /// Ids of subsequences selected for alignment
    /// </param>
    /// <returns>
    /// JSON containing result status (Success / Error)
    /// and remote job id or errror message.
    /// </returns>
    public string CreateAlignmentTask(long[] subsequencesIds)
    {
        try
        {
            ISequence[] bioSequences;
            using var db = dbFactory.CreateDbContext();
            using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
            var subsequenceExtractor = new SubsequenceExtractor(db, commonSequenceRepository);
            
            bioSequences = subsequenceExtractor.GetBioSequencesForFastaConverter(subsequencesIds);
            
            FastAFormatter formatter = new FastAFormatter();
            using MemoryStream stream = new MemoryStream();
            formatter.Format(stream, bioSequences);
            string fasta = Encoding.ASCII.GetString(stream.ToArray());
            
            // TODO: make email global parameter
            string data = $"email=info@foarlab.org&sequence={fasta}";
            const string urlClustalo = $"clustalo/run";
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = BaseAddress;
            StringContent postData = new(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            using HttpResponseMessage response = httpClient.PostAsync(urlClustalo, postData)
                .ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode()).Result;

            string requestResult = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.SerializeObject(new { Status = "Success", Result = requestResult });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { Status = "Error", ex.Message });
        }
    }
}
