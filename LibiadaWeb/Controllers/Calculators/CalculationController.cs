namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

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
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate");
                ViewBag.data = JsonConvert.SerializeObject(viewData);
                return View();
            }
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
            return CreateTask(() =>
            {
                var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
                var characteristicNames = new string[characteristicLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                Dictionary<long, string> mattersNames;
                double[][] characteristics;
                long[][] sequenceIds;

                using (var db = new LibiadaWebEntities())
                {
                    mattersNames = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    sequenceIds = commonSequenceRepository.GetSeuqneceIds(matterIds, notations, languages, translators);

                    var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
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
                    characteristics = SequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);
                }
                else
                {
                    characteristics = SequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds, rotate, complementary, rotationLength);
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
