// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuickCalculationController.cs" company="">
//   
// </copyright>
// <summary>
//   The quick calculation controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;

    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The quick calculation controller.
    /// </summary>
    public class QuickCalculationController : Controller
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
            this.db = new LibiadaWebEntities();
            this.characteristicRepository = new CharacteristicTypeRepository(this.db);
            this.linkRepository = new LinkRepository(this.db);
        }

        // GET: /QuickCalculation/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            IEnumerable<characteristic_type> characteristicsList =
                this.db.characteristic_type.Where(c => c.full_chain_applicable);
            this.ViewBag.characteristicsList = this.characteristicRepository.GetSelectListItems(
                characteristicsList, 
                null);

            this.ViewBag.linksList = this.linkRepository.GetSelectListItems(null);
            return this.View();
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
            var characteristics = new List<double>();
            var characteristicNames = new List<string>();

            for (int i = 0; i < characteristicIds.Length; i++)
            {
                var characteristicId = characteristicIds[i];
                var linkId = linkIds[i];

                var tempChain = new Chain(chain);

                characteristicNames.Add(
                    this.db.characteristic_type.Single(charact => charact.id == characteristicId).name);
                var className = this.db.characteristic_type.Single(charact => charact.id == characteristicId).class_name;
                var calculator = CalculatorsFactory.Create(className);
                var link = (Link)this.db.link.Single(l => l.id == linkId).id;

                characteristics.Add(calculator.Calculate(tempChain, link));
            }

            this.TempData["characteristics"] = characteristics;
            this.TempData["characteristicNames"] = characteristicNames;
            this.TempData["characteristicIds"] = characteristicIds;
            return this.RedirectToAction("Result");
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Result()
        {
            var characteristics = this.TempData["characteristics"] as List<double>;
            var characteristicNames = this.TempData["characteristicNames"] as List<string>;
            var characteristicIds = this.TempData["characteristicIds"] as int[];
            var characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicNames.Count; i++)
            {
                characteristicsList.Add(
                    new SelectListItem { Value = i.ToString(), Text = characteristicNames[i], Selected = false });
            }

            this.ViewBag.characteristicIds = new List<int>(characteristicIds);
            this.ViewBag.characteristicsList = characteristicsList;
            this.ViewBag.characteristics = characteristics;
            this.ViewBag.characteristicNames = characteristicNames;

            this.TempData.Keep();

            return this.View();
        }
    }
}