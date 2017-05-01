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
    /// Controller for filtered subsequences calculation.
    /// </summary>
    public class FilteredSubsequenceCalculationController : AbstractResultController
    {
       /// <summary>
        /// Initializes a new instance of the <see cref="FilteredSubsequenceCalculationController"/> class.
        /// </summary>
        public FilteredSubsequenceCalculationController() : base(TaskType.FilteredSubsequenceCalculation)
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
        /// <param name="filters">
        /// Filters for the subsequences.
        /// Filters are applied in "OR" logic (if subsequence corresponds to any filter it is added to calculation).
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, short[] characteristicLinkIds, Feature[] features, string[] filters)
        {
            return Action(() =>
            {
                var attributeValues = new List<AttributeValue>();
                var characteristics = new Dictionary<string, SubsequenceData[]>(matterIds.Length);
                var matterNames = new string[matterIds.Length];
                var characteristicNames = new string[characteristicLinkIds.Length];

                long[] parentSequenceIds;

                using (var db = new LibiadaWebEntities())
                {
                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                            .Select(s => new { s.Id, MatterName = s.Matter.Name })
                                            .ToDictionary(s => s.Id);
                    parentSequenceIds = parentSequences.Keys.ToArray();

                    for (int n = 0; n < parentSequenceIds.Length; n++)
                    {
                        matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    }

                    var characteristicTypeLinkRepository = new FullCharacteristicRepository(db);
                    for (int k = 0; k < characteristicLinkIds.Length; k++)
                    {
                        var characteristicLinkId = characteristicLinkIds[k];
                        characteristicNames[k] = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkId);
                    }
                }

                // cycle through matters; first level of characteristics array
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    // all subsequence calculations
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                        characteristicLinkIds,
                        features,
                        parentSequenceIds[i],
                        attributeValues,
                        filters);
                    subsequencesData = subsequencesData.OrderByDescending(s => s.CharacteristicsValues[0]).ToArray();
                    characteristics[matterNames[i]] = subsequencesData;
                }

                return new Dictionary<string, object>
                            {
                                { "characteristics", characteristics },
                                { "matterNames", matterNames },
                                { "characteristicNames", characteristicNames },
                                { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                                { "attributes", ArrayExtensions.ToArray<Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                                { "attributeValues", attributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) }
                            };
            });
        }
    }
}
