namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
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
                ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(1, int.MaxValue, "Calculate"));
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
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int[] characteristicTypeLinkIds, int[] featureIds)
        {
            return Action(() =>
            {
                Dictionary<int, string> features;
                var characteristics = new Dictionary<string, SubsequenceData[]>(matterIds.Length);

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];
                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                var links = new Link[characteristicTypeLinkIds.Length];

                using (var db = new LibiadaWebEntities())
                {
                    var featureRepository = new FeatureRepository(db);
                    features = featureRepository.Features.ToDictionary(f => f.Id, f => f.Name);

                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
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
                for (int m = 0; m < parentSequenceIds.Length; m++)
                {
                    var subsequenceData = SubsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(
                            characteristicTypeLinkIds,
                            featureIds,
                            parentSequenceIds[m],
                            calculators,
                            links);
                    characteristics.Add(matterNames[m], subsequenceData);
                }

                return new Dictionary<string, object>
                            {
                                { "characteristics", characteristics },
                                { "matterNames", matterNames },
                                { "characteristicNames", characteristicNames },
                                { "features", features }
                            };
            });
        }
    }
}
