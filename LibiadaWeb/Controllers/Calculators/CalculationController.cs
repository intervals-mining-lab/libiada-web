namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Music;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.CalculatorsData;
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
        /// <param name="pauseTreatments">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfers">
        /// Sequential transfer flag used in music sequences.
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
            PauseTreatment[] pauseTreatments,
            bool[] sequentialTransfers,
            bool rotate,
            bool complementary,
            uint? rotationLength)
        {
            return CreateTask(() =>
            {
                Dictionary<long, string> mattersNames;
                long[][] sequenceIds;
                using (var db = new LibiadaWebEntities())
                {
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds, notations, languages, translators, pauseTreatments, sequentialTransfers);
                    mattersNames = Cache.GetInstance().Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
                }

                double[][] characteristics;
                if (!rotate && !complementary)
                {
                    characteristics = SequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);
                }
                else
                {
                    characteristics = SequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds, rotate, complementary, rotationLength);
                }

                var sequencesCharacteristics = new SequenceCharacteristics[matterIds.Length];
                for (int i = 0; i < matterIds.Length; i++)
                {
                    sequencesCharacteristics[i] = new SequenceCharacteristics
                    {
                        MatterName = mattersNames[matterIds[i]],
                        Characteristics = characteristics[i]
                    };
                }

                var characteristicNames = new string[characteristicLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                    characteristicsList[k] = new SelectListItem
                                                 {
                                                     Value = k.ToString(),
                                                     Text = characteristicNames[k],
                                                     Selected = false
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
