namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web.Mvc;

    using Newtonsoft.Json;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Bio.IO.FastA;
    using Bio;

    using static LibiadaWeb.Models.Calculators.SubsequencesCharacteristicsCalculator;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;
    using Attribute = LibiadaWeb.Attribute;


    /// <summary>
    /// The subsequences distribution controller.
    /// </summary>
    [Authorize]
    public class SubsequencesDistributionController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesDistributionController"/> class.
        /// </summary>
        public SubsequencesDistributionController() : base(TaskType.SubsequencesDistribution)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(1, int.MaxValue, "Calculate"));
            }

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

                    var matterNames = new string[matterIds.Length];
                    var remoteIds = new string[matterIds.Length];
                    var subsequencesCharacteristicsNames = new string[characteristicLinkIds.Length];
                    var subsequencesCharacteristicsList = new SelectListItem[characteristicLinkIds.Length];
                    var attributeValuesCache = new AttributeValueCacheManager();
                    long[] sequenceIds;

                    using (var db = new LibiadaWebEntities())
                    {
                        DnaSequence[] parentSequences = db.DnaSequence.Include(s => s.Matter)
                                                .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                                .OrderBy(s => s.MatterId)
                                                .ToArray();

                        for (int n = 0; n < parentSequences.Length; n++)
                        {
                            matterNames[n] = parentSequences[n].Matter.Name;
                            remoteIds[n] = parentSequences[n].RemoteId;
                        }

                        var geneticSequenceRepository = new GeneticSequenceRepository(db);
                        sequenceIds = geneticSequenceRepository.GetNucleotideSequenceIds(matterIds);
                    }

                    var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
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

                    double[] characteristics = SequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkId);

                    var sequencesData = new SequenceData[matterIds.Length];

                    for (int i = 0; i < matterIds.Length; i++)
                    {
                        // all subsequence calculations
                        SubsequenceData[] subsequencesData = CalculateSubsequencesCharacteristics(
                            characteristicLinkIds,
                            features,
                            sequenceIds[i]);

                        attributeValuesCache.FillAttributeValues(subsequencesData);

                        sequencesData[i] = new SequenceData(matterIds[i], matterNames[i], remoteIds[i], characteristics[i], subsequencesData);
                    }

                    // sorting organisms by their characteristic
                    Array.Sort(sequencesData, (x,y) => x.Characteristic.CompareTo(y.Characteristic));
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

                using (var db = new LibiadaWebEntities())
                {
                    var subsequenceExtractor = new SubsequenceExtractor(db);
                    bioSequences = subsequenceExtractor.GetBioSequencesForFastaConverter(subsequencesIds);
                }

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
