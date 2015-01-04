namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaWeb.Models.Repositories.Catalogs;

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
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The link repository.
        /// </summary>
        private readonly LinkRepository linkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuickCalculationController"/> class.
        /// </summary>
        public QuickCalculationController()
        {
            db = new LibiadaWebEntities();
            this.characteristicRepository = new CharacteristicTypeRepository(db);
            this.linkRepository = new LinkRepository(db);

            ControllerName = "QuickCalculation";
            DisplayName = "Quick calculation";
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);
            ViewBag.characteristicsList = this.characteristicRepository.GetSelectListItems(
                characteristicsList, 
                null);

            ViewBag.linksList = this.linkRepository.GetSelectListItems(null);
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
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(int[] characteristicIds, int[] linkIds, string chain)
        {
            return Action(() =>
            {
                var characteristics = new List<double>();
                var characteristicNames = new List<string>();

                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    var characteristicId = characteristicIds[i];
                    var linkId = linkIds[i];

                    var tempChain = new Chain(chain);

                    characteristicNames.Add(
                        db.characteristic_type.Single(charact => charact.id == characteristicId).name);
                    var className = db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                    var calculator = CalculatorsFactory.CreateFullCalculator(className);
                    var link = (Link) db.link.Single(l => l.id == linkId).id;

                    characteristics.Add(calculator.Calculate(tempChain, link));
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(
                        new SelectListItem {Value = i.ToString(), Text = characteristicNames[i], Selected = false});
                }

                return new Dictionary<string, object>
                {
                    {"characteristics", characteristics},
                    {"characteristicIds", new List<int>(characteristicIds)},
                    {"characteristicNames", characteristicNames},
                    {"characteristicsList", characteristicsList}
                };
            });
        }
    }
}