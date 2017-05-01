namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

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
        public CalculationController() : base(TaskType.Calculation)
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

            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="notations">
        /// The notations ids.
        /// </param>
        /// <param name="languages">
        /// The languages ids.
        /// </param>
        /// <param name="translators">
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
            short[] characteristicLinkIds,
            Notation[] notations,
            Language[] languages,
            Translator?[] translators,
            bool rotate,
            bool complementary,
            uint? rotationLength)
        {
            return Action(() =>
                {
                    var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
                    var characteristicNames = new string[characteristicLinkIds.Length];
                    var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                    Dictionary<long, string> mattersNames;
                    double[][] characteristics;
                    Chain[][] chains;

                    using (var db = new LibiadaWebEntities())
                    {
                        mattersNames = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

                        var commonSequenceRepository = new CommonSequenceRepository(db);
                        chains = commonSequenceRepository.GetChains(matterIds, notations, languages, translators);

                        var characteristicTypeLinkRepository = new FullCharacteristicRepository(db);
                        for (int k = 0; k < characteristicLinkIds.Length; k++)
                        {
                            characteristicNames[k] = characteristicTypeLinkRepository.GetFullCharacteristicName(characteristicLinkIds[k], notations[k]);
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
                        characteristics = SequencesCharacteristicsCalculator.Calculate(chains, characteristicLinkIds);
                    }
                    else
                    {
                        characteristics = SequencesCharacteristicsCalculator.Calculate(chains, characteristicLinkIds, rotate, complementary, rotationLength);
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
