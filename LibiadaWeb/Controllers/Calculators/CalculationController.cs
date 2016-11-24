namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Account;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;
    using Models;
    using Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The calculation controller.
    /// </summary>
    [Authorize]
    public class CalculationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationController"/> class.
        /// </summary>
        public CalculationController()
            : base("Characteristics calculation")
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
            var db = new LibiadaWebEntities();
            var viewDataHelper = new ViewDataHelper(db);

            Func<CharacteristicType, bool> filter;
            if (UserHelper.IsAdmin())
            {
                filter = c => c.FullSequenceApplicable;
            }
            else
            {
                filter = c => c.FullSequenceApplicable && Aliases.UserAvailableCharacteristics.Contains((Aliases.CharacteristicType)c.Id);
            }

            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(filter, 1, int.MaxValue, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notations ids.
        /// </param>
        /// <param name="languageIds">
        /// The languages ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translators ids.
        /// </param>
        /// <param name="rotate">
        /// Rotation flag.
        /// </param>
        /// <param name="complementary">
        /// Complement flag.
        /// </param>
        /// <param name="rotationLength">
        /// The rotation length.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
            int[] notationIds,
            int[] languageIds,
            int?[] translatorIds,
            bool rotate,
            bool complementary,
            uint? rotationLength)
        {
            return Action(() =>
                {
                    var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
                    var characteristicNames = new string[characteristicTypeLinkIds.Length];
                    var characteristicsList = new SelectListItem[characteristicTypeLinkIds.Length];
                    var links = new Link[characteristicTypeLinkIds.Length];
                    var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                    Dictionary<long, string> mattersNames;
                    double[][] characteristics;
                    Chain[][] chains;

                    using (var db = new LibiadaWebEntities())
                    {
                        mattersNames = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

                        var commonSequenceRepository = new CommonSequenceRepository(db);
                        chains = commonSequenceRepository.GetChains(matterIds, notationIds, languageIds, translatorIds);

                        var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                        for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                        {
                            links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                            calculators[k] = CalculatorsFactory.CreateFullCalculator(className);
                            characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notationIds[k]);
                            characteristicsList[k] = new SelectListItem
                            {
                                Value = k.ToString(),
                                Text = characteristicNames[k],
                                Selected = false
                            };
                        }
                    }

                    if (!rotate && !complementary)
                    {
                        characteristics = SequencesCharacteristicsCalculator.Calculate(chains, calculators, links, characteristicTypeLinkIds);
                    }
                    else
                    {
                        characteristics = SequencesCharacteristicsCalculator.Calculate(chains, calculators, links, rotate, complementary, rotationLength);
                    }

                    for (int i = 0; i < matterIds.Length; i++)
                    {
                        sequencesCharacteristics[i] = new SequenceCharacteristics
                        {
                            MatterName = mattersNames[matterIds[i]],
                            Characteristics = characteristics[i]
                        };
                    }

                    var result = new Dictionary<string, object>
                                            {
                                                    { "characteristics", sequencesCharacteristics },
                                                    { "characteristicNames", characteristicNames },
                                                    { "characteristicsList", characteristicsList }
                                            };

                    return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
                });
        }
    }
}
