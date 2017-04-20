namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
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
        public SubsequencesDistributionController() : base("Subsequences distribution", TaskType.SubsequencesDistribution)
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
            return Action(() =>
                {
                    matterIds = matterIds.OrderBy(m => m).ToArray();

                    var matterNames = new string[matterIds.Length];
                    var remoteIds = new string[matterIds.Length];
                    var subsequencesCharacteristicsNames = new string[characteristicLinkIds.Length];
                    var subsequencesCharacteristicsList = new SelectListItem[characteristicLinkIds.Length];
                    var attributeValues = new List<AttributeValue>();
                    Chain[] chains;
                    string sequenceCharacteristicName;

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

                        var commonSequenceRepository = new CommonSequenceRepository(db);
                        chains = commonSequenceRepository.GetNucleotideChains(matterIds);

                        var characteristicTypeLinkRepository = new FullCharacteristicRepository(db);

                        sequenceCharacteristicName = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkId);

                        for (int k = 0; k < characteristicLinkIds.Length; k++)
                        {
                            subsequencesCharacteristicsNames[k] = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkIds[k]);
                            subsequencesCharacteristicsList[k] = new SelectListItem
                            {
                                Value = k.ToString(),
                                Text = subsequencesCharacteristicsNames[k],
                                Selected = false
                            };
                        }
                    }

                    double[] characteristics = SequencesCharacteristicsCalculator.Calculate(chains, characteristicLinkId);

                    var sequencesData = new SequenceData[matterIds.Length];

                    for (int i = 0; i < matterIds.Length; i++)
                    {
                        // all subsequence calculations
                        SubsequenceData[] subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            characteristicLinkIds,
                            features,
                            chains[i].Id,
                            attributeValues);

                        sequencesData[i] = new SequenceData(matterIds[i], matterNames[i], remoteIds[i], characteristics[i], subsequencesData);
                    }

                    // sorting organisms by their characteristic
                    sequencesData = sequencesData.OrderBy(r => r.Characteristic).ToArray();

                    var resultData = new Dictionary<string, object>
                                 {
                                     { "result", sequencesData },
                                     { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                                     { "subsequencesCharacteristicsList", subsequencesCharacteristicsList },
                                     { "sequenceCharacteristicName", sequenceCharacteristicName },
                                     { "features", features.ToSelectList(features).ToDictionary(f => f.Value) },
                                     { "attributes", ArrayExtensions.ToArray<Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
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
