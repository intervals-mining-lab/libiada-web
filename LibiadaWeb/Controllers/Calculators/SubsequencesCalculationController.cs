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
        public SubsequencesCalculationController() : base("Subsequences characteristics calculation")
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
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int[] characteristicTypeLinkIds, Feature[] features)
        {
            return Action(() =>
            {
                var attributeValues = new List<AttributeValue>();
                var characteristics = new Dictionary<string, SubsequenceData[]>(matterIds.Length);

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];
                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                var links = new Link[characteristicTypeLinkIds.Length];
                var featureRepository = new FeatureRepository();

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
                    var subsequencesData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            characteristicTypeLinkIds,
                            features,
                            parentSequenceIds[i],
                            calculators,
                            links,
                            attributeValues);
                    characteristics.Add(matterNames[i], subsequencesData);
                }

                return new Dictionary<string, object>
                            {
                                { "characteristics", characteristics },
                                { "matterNames", matterNames },
                                { "characteristicNames", characteristicNames },
                                { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) }
                            };
            });
        }
    }
}
