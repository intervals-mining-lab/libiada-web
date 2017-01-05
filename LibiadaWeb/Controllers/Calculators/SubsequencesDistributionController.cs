namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences distribution controller.
    /// </summary>
    [Authorize]
    public class SubsequencesDistributionController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesDistributionController"/> class.
        /// </summary>
        public SubsequencesDistributionController() : base("Subsequences distribution")
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
        /// <param name="characteristicTypeLinkId">
        /// Full sequence characteristic type and link id.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// Subsequences characteristics types and links ids.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int characteristicTypeLinkId, int[] characteristicTypeLinkIds, int[] featureIds)
        {
            return Action(() =>
                {
                    var matterNames = new string[matterIds.Length];
                    var remoteIds = new string[matterIds.Length];
                    var subsequencesCharacteristicsNames = new string[characteristicTypeLinkIds.Length];
                    var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                    var links = new Link[characteristicTypeLinkIds.Length];
                    var subsequencesCharacteristicsList = new SelectListItem[characteristicTypeLinkIds.Length];
                    var attributeValues = new List<AttributeValue>();
                    IEnumerable<SelectListItem> featuresSelectList;
                    Chain[] chains;
                    long[] parentSequenceIds;
                    string sequenceCharacteristicName;
                    IFullCalculator calculator;
                    Link link;

                    using (var db = new LibiadaWebEntities())
                    {
                        var featureRepository = new FeatureRepository(db);
                        featuresSelectList = featureRepository.GetFeaturesById(featureIds).Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name, Selected = true });

                        var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                                .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                                .Select(s => new { s.Id, MatterName = s.Matter.Name, s.RemoteId })
                                                .ToDictionary(s => s.Id);
                        parentSequenceIds = parentSequences.Keys.ToArray();

                        for (int n = 0; n < parentSequenceIds.Length; n++)
                        {
                            matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                            remoteIds[n] = parentSequences[parentSequenceIds[n]].RemoteId;
                        }

                        var commonSequenceRepository = new CommonSequenceRepository(db);
                        chains = commonSequenceRepository.GetNucleotideChains(matterIds);

                        var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);

                        sequenceCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId);
                        calculator = CalculatorsFactory.CreateFullCalculator(characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName);
                        link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                        for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                        {
                            links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                            calculators[k] = CalculatorsFactory.CreateFullCalculator(className);
                            subsequencesCharacteristicsNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k]);
                            subsequencesCharacteristicsList[k] = new SelectListItem
                            {
                                Value = k.ToString(),
                                Text = subsequencesCharacteristicsNames[k],
                                Selected = false
                            };
                        }
                    }

                    var characteristics = SequencesCharacteristicsCalculator.Calculate(chains, calculator, link, characteristicTypeLinkId);

                    var sequenceData = new SequenceData[parentSequenceIds.Length];

                    for (int i = 0; i < parentSequenceIds.Length; i++)
                    {
                        // all subsequence calculations
                        var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            characteristicTypeLinkIds,
                            featureIds,
                            parentSequenceIds[i],
                            calculators,
                            links,
                            attributeValues);
                        sequenceData[i] = new SequenceData(matterIds[i], matterNames[i], remoteIds[i], characteristics[i], subsequencesData);
                    }

                    // sorting organisms by their characteristic
                    sequenceData = sequenceData.OrderBy(r => r.Characteristic).ToArray();

                    var resultData = new Dictionary<string, object>
                                 {
                                     { "result", sequenceData },
                                     { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                                     { "subsequencesCharacteristicsList", subsequencesCharacteristicsList },
                                     { "sequenceCharacteristicName", sequenceCharacteristicName },
                                     { "features", featuresSelectList.ToDictionary(f => f.Value) },
                                     { "attributes", EnumExtensions.ToArray<Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                                     { "attributeValues", attributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) }
                                 };

                    return new Dictionary<string, object>
                    {
                        { "data", JsonConvert.SerializeObject(resultData) }
                    };
                });
        }
    }
}
