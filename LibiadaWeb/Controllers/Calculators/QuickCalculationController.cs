namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;

    using Models.Repositories.Catalogs;

    /// <summary>
    /// The quick calculation controller.
    /// </summary>
    public class QuickCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuickCalculationController"/> class.
        /// </summary>
        public QuickCalculationController() : base("QuickCalculation", "Quick calculation")
        {
            db = new LibiadaWebEntities();
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var characteristicTypes = characteristicTypeLinkRepository.GetCharacteristics(c => c.FullSequenceApplicable);

            ViewBag.data = new Dictionary<string, object>
                {
                    { "characteristicTypes", characteristicTypes }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(int[] characteristicIds, int[] linkIds, string sequence)
        {
            return Action(() =>
            {
                var characteristics = new List<double>();
                var characteristicNames = new List<string>();

                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    var characteristicId = characteristicIds[i];
                    var linkId = linkIds[i];

                    var chain = new Chain(sequence);

                    characteristicNames.Add(db.CharacteristicType.Single(charact => charact.Id == characteristicId).Name);
                    var className = db.CharacteristicType.Single(charact => charact.Id == characteristicId).ClassName;
                    var calculator = CalculatorsFactory.CreateFullCalculator(className);

                    characteristics.Add(calculator.Calculate(chain, (Link)linkId));
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem { Value = i.ToString(), Text = characteristicNames[i], Selected = false });
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "characteristicIds", new List<int>(characteristicIds) },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList }
                };
            });
        }
    }
}
