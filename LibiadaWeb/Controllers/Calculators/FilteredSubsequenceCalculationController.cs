namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// Controller for filtered subsequences calculation.
    /// </summary>
    public class FilteredSubsequenceCalculationController : AbstractResultController
    {
       /// <summary>
        /// Initializes a new instance of the <see cref="FilteredSubsequenceCalculationController"/> class.
        /// </summary>
        public FilteredSubsequenceCalculationController() : base("Subsequences characteristics calculation")
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
        /// <param name="characteristicTypeLinkIds">
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
        public ActionResult Index(long[] matterIds, int[] characteristicTypeLinkIds, Feature[] features, string[] filters)
        {
            return Action(() =>
            {
                var attributeValues = new List<AttributeValue>();
                var characteristics = new Dictionary<string, SubsequenceData[]>(matterIds.Length);
                var matterNames = new string[matterIds.Length];
                var characteristicNames = new string[characteristicTypeLinkIds.Length];

                long[] parentSequenceIds;
                var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                var links = new LibiadaCore.Core.Link[characteristicTypeLinkIds.Length];

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

                    var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                    for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                    {
                        var characteristicTypeLinkId = characteristicTypeLinkIds[k];
                        characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId);
                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        calculators[k] = CalculatorsFactory.CreateFullCalculator(className);
                        links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                    }
                }

                // cycle through matters; first level of characteristics array
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    // all subsequence calculations
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                        characteristicTypeLinkIds,
                        features,
                        parentSequenceIds[i],
                        calculators,
                        links,
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
