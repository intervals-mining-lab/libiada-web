namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesCalculationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
        /// </summary>
        public SubsequencesCalculationController() : base(TaskType.SubsequencesCalculation)
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
        /// <param name="characteristicLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, short[] characteristicLinkIds, Feature[] features)
        {
            return Action(() =>
            {
                var attributeValues = new List<AttributeValue>();
                var sequencesData = new SequenceData[matterIds.Length];

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];
                var remoteIds = new string[matterIds.Length];
                var subsequencesCharacteristicsNames = new string[characteristicLinkIds.Length];
                var subsequencesCharacteristicsList = new SelectListItem[characteristicLinkIds.Length];

                using (var db = new LibiadaWebEntities())
                {
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

                    var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
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

                // cycle through matters; first level of characteristics array
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            characteristicLinkIds,
                            features,
                            parentSequenceIds[i],
                            attributeValues);
                    sequencesData[i] = new SequenceData(matterIds[i], matterNames[i], remoteIds[i], default(double), subsequencesData);
                }

                var resultData = new Dictionary<string, object>
                            {
                                { "sequencesData", sequencesData },
                                { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                                { "attributes", ArrayExtensions.ToArray<Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                                { "attributeValues", attributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                                { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                                { "subsequencesCharacteristicsList", subsequencesCharacteristicsList }
                            };
                return new Dictionary<string, object>
                    {
                        { "data", JsonConvert.SerializeObject(resultData) }
                    };
            });
        }
    }
}
