namespace Libiada.Web.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json;

    using LibiadaCore.Extensions;

    using Libiada.Web.Extensions;
    using Libiada.Web.Helpers;
    using Libiada.Web.Tasks;

    using Libiada.Database.Models;
    using Libiada.Database.Models.Calculators;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Bio.IO.FastA;
    using Bio;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;
    using Attribute = Database.Attribute;
    
    /// <summary>
    /// The subsequences distribution controller.
    /// </summary>
    [Authorize]
    public class SubsequencesDistributionController : AbstractResultController
    {
        private readonly ILibiadaDatabaseEntitiesFactory dbFactory;
        private readonly IViewDataHelper viewDataHelper;
        private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
        private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
        private readonly ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator;
        private readonly ICommonSequenceRepository commonSequenceRepository;
        private readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesDistributionController"/> class.
        /// </summary>
        public SubsequencesDistributionController(ILibiadaDatabaseEntitiesFactory dbFactory,
                                                  IViewDataHelper viewDataHelper,
                                                  ITaskManager taskManager,
                                                  IFullCharacteristicRepository characteristicTypeLinkRepository,
                                                  ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                                  ISequencesCharacteristicsCalculator sequencesCharacteristicsCalculator,
                                                  ICommonSequenceRepository commonSequenceRepository, 
                                                  Cache cache)
            : base(TaskType.SubsequencesDistribution, taskManager)
        {
            this.dbFactory = dbFactory;
            this.viewDataHelper = viewDataHelper;
            this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
            this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
            this.sequencesCharacteristicsCalculator = sequencesCharacteristicsCalculator;
            this.commonSequenceRepository = commonSequenceRepository;
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
                var db = dbFactory.CreateDbContext();
                var matterNames = new string[matterIds.Length];
                var remoteIds = new string[matterIds.Length];
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
                var result = new Dictionary<string, object>
                                {
                                    { "result", sequencesData },
                                    { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                                    { "subsequencesCharacteristicsList", subsequencesCharacteristicsList },
                                    { "sequenceCharacteristicName", sequenceCharacteristicName },
                                    { "features", features.ToSelectList(features).ToDictionary(f => f.Value) },
                                    { "attributes", EnumExtensions.ToArray<Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
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

                var subsequenceExtractor = new SubsequenceExtractor(dbFactory.CreateDbContext(), commonSequenceRepository);
                bioSequences = subsequenceExtractor.GetBioSequencesForFastaConverter(subsequencesIds);


                string fasta;
                FastAFormatter formatter = new FastAFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Format(stream, bioSequences);
                    fasta = Encoding.ASCII.GetString(stream.ToArray());
                }

                string result;
                using (var webClient = new WebClient())
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    Uri url = new Uri("https://www.ebi.ac.uk/Tools/services/rest/clustalo/run");

                    // TODO: make email global parameter
                    result = webClient.UploadString(url, $"email=info@foarlab.org&sequence={fasta}");
                }

                return JsonConvert.SerializeObject(new { Status = "Success", Result = result });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Status = "Error", ex.Message });
            }
        }
    }
}
