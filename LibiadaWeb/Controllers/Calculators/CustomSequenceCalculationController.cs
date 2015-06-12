namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The quick calculation controller.
    /// </summary>
    public class CustomSequenceCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceCalculationController"/> class.
        /// </summary>
        public CustomSequenceCalculationController() : base("CustomSequenceCalculation", "Custom sequence calculation")
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
            var viewDataHelper = new ViewDataHelper(db);

            ViewBag.data = new Dictionary<string, object>
                {
                    { "characteristicTypes", viewDataHelper.GetCharacteristicTypes(c => c.FullSequenceApplicable) }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(int[] characteristicTypeLinkIds, string sequence)
        {
            return Action(() =>
            {
                var characteristics = new List<double>();
                var characteristicNames = new List<string>();

                foreach (int characteristicTypeLinkId in characteristicTypeLinkIds)
                {
                    var chain = new Chain(sequence);

                    var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;

                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId));
                    
                    var calculator = CalculatorsFactory.CreateFullCalculator(className);

                    characteristics.Add(calculator.Calculate(chain, link));
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem { Value = i.ToString(), Text = characteristicNames[i], Selected = false });
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList }
                };
            });
        }
    }
}
