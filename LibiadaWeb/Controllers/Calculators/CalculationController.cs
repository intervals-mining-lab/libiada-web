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
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The calculation controller.
    /// </summary>
    public class CalculationController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationController"/> class.
        /// </summary>
        public CalculationController()
        {
            this.db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(this.db);
            this.characteristicRepository = new CharacteristicTypeRepository(this.db);
            this.notationRepository = new NotationRepository(this.db);
            this.chainRepository = new ChainRepository(this.db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            this.ViewBag.dbName = DbHelper.GetDbName(this.db);
            IEnumerable<characteristic_type> characteristicsList =
                this.db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = this.characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var translators = new SelectList(this.db.translator, "id", "name").ToList();

            translators.Add(new SelectListItem { Value = null, Text = "Нет" });

            this.ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", this.matterRepository.GetSelectListWithNature() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", this.notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(this.db.nature, "id", "name") }, 
                    { "links", new SelectList(this.db.link, "id", "name") }, 
                    { "languages", new SelectList(this.db.language, "id", "name") }, 
                    { "translators", translators }, 
                    { "natureLiterature", Aliases.NatureLiterature }
                };
            return this.View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="languageIds">
        /// The language ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translator ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicIds, 
            int?[] linkIds, 
            int[] notationIds, 
            int[] languageIds, 
            int?[] translatorIds)
        {
            var characteristics = new List<List<double>>();
            var chainNames = new List<string>();
            var characteristicNames = new List<string>();

            foreach (var matterId in matterIds)
            {
                chainNames.Add(this.db.matter.Single(m => m.id == matterId).name);

                characteristics.Add(new List<double>());
                for (int i = 0; i < notationIds.Length; i++)
                {
                    int notationId = notationIds[i];
                    int languageId = languageIds[i];
                    int? translatorId = translatorIds[i];

                    long chainId;
                    if (this.db.matter.Single(m => m.id == matterId).nature_id == Aliases.NatureLiterature)
                    {
                        chainId = this.db.literature_chain.Single(l => l.matter_id == matterId &&
                                    l.notation_id == notationId
                                    && l.language_id == languageId
                                    && ((translatorId == null && l.translator_id == null)
                                                    || (translatorId == l.translator_id))).id;
                    }
                    else
                    {
                        chainId = this.db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                    }

                    int characteristicId = characteristicIds[i];
                    int? linkId = linkIds[i];
                    if (this.db.characteristic.Any(c =>
                                              linkId == c.link.id && c.chain_id == chainId &&
                                              c.characteristic_type_id == characteristicId))
                    {
                        characteristic dbCharacteristic = this.db.characteristic.Single(c =>
                                                                       linkId == c.link.id && c.chain_id == chainId &&
                                                                       c.characteristic_type_id == characteristicId);
                        characteristics.Last().Add(dbCharacteristic.value.Value);
                    }
                    else
                    {
                        Chain tempChain = this.chainRepository.ToLibiadaChain(chainId);
                        tempChain.FillIntervalManagers();
                        string className =
                            this.db.characteristic_type.Single(ct => ct.id == characteristicId).class_name;
                        ICalculator calculator = CalculatorsFactory.Create(className);
                        var link = (Link)this.db.link.Single(l => l.id == linkId).id;
                        var characteristicValue = calculator.Calculate(tempChain, link);
                        int characteristicType = characteristicIds[i];
                        var dbCharacteristic = new characteristic
                        {
                            chain_id = chainId, 
                            characteristic_type_id = characteristicIds[i], 
                            link_id = this.db.characteristic_type.Single(c => c.id == characteristicType).linkable ? linkId : null, 
                            value = characteristicValue, 
                            value_string = characteristicValue.ToString(), 
                            created = DateTime.Now
                        };
                        this.db.characteristic.Add(dbCharacteristic);
                        this.db.SaveChanges();
                        characteristics.Last().Add(characteristicValue);
                    }
                }
            }

            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int? linkId = linkIds[k];
                int notationId = notationIds[k];
                int? translatorId = translatorIds[k];
                characteristicNames.Add(this.db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        this.db.link.Single(l => l.id == linkId).name + " " +
                                        this.db.notation.Single(n => n.id == notationId).name
                                        + (translatorId == null ? string.Empty : " " + this.db.translator.Single(t => t.id == translatorId)));
            }

            this.TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "chainNames", chainNames }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "characteristicIds", characteristicIds }, 
                                         { "chainIds", new List<long>(matterIds) }
                                     };

            return this.RedirectToAction("Result");
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public ActionResult Result()
        {
            try
            {
                var result = this.TempData["characteristics"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("Нет данных для отображения");
                }

                var characteristics = result["characteristics"];
                var characteristicNames = (List<string>)result["characteristicNames"];
                this.ViewBag.chainIds = result["chainIds"];
                var characteristicIds = (int[])result["characteristicIds"];
                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(), 
                        Text = characteristicNames[i], 
                        Selected = false
                    });
                }

                this.ViewBag.characteristicIds = new List<int>(characteristicIds);
                this.ViewBag.characteristicsList = characteristicsList;
                this.ViewBag.characteristics = characteristics;
                this.ViewBag.chainNames = result["chainNames"];
                this.ViewBag.characteristicNames = characteristicNames;

                this.TempData.Keep();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("Error", e.Message);
            }

            return this.View();
        }
    }
}