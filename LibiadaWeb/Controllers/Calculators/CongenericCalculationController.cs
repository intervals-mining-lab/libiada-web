// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CongenericCalculationController.cs" company="">
//   
// </copyright>
// <summary>
//   The congeneric calculation controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The congeneric calculation controller.
    /// </summary>
    public class CongenericCalculationController : Controller
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
        /// Initializes a new instance of the <see cref="CongenericCalculationController"/> class.
        /// </summary>
        public CongenericCalculationController()
        {
            this.db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(this.db);
            this.characteristicRepository = new CharacteristicTypeRepository(this.db);
            this.notationRepository = new NotationRepository(this.db);
            this.chainRepository = new ChainRepository(this.db);
        }

        // GET: /CongenericCalculation/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            this.ViewBag.dbName = DbHelper.GetDbName(this.db);
            List<matter> matters = this.db.matter.Include("nature").ToList();
            this.ViewBag.matterCheckBoxes = this.matterRepository.GetSelectListItems(matters, null);
            this.ViewBag.matters = matters;

            IEnumerable<characteristic_type> characteristicsList =
                this.db.characteristic_type.Where(c => c.congeneric_chain_applicable);

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
        /// <param name="isSort">
        /// The is sort.
        /// </param>
        /// <param name="theoretical">
        /// The theoretical.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicIds, 
            int[] linkIds, 
            int[] notationIds, 
            int[] languageIds, 
            int?[] translatorIds, 
            bool isSort, 
            bool theoretical)
        {
            var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
            var teoreticalRanks = new List<List<List<double>>>();
            var chainNames = new List<string>();
            var elementNames = new List<List<string>>();
            var characteristicNames = new List<string>();

            bool isLiteratureChain = false;

            // Перебор всех цепочек; первый уровень массива характеристик
            for (int w = 0; w < matterIds.Length; w++)
            {
                long matterId = matterIds[w];
                chainNames.Add(this.db.matter.Single(m => m.id == matterId).name);
                elementNames.Add(new List<string>());
                characteristics.Add(new List<List<KeyValuePair<int, double>>>());
                teoreticalRanks.Add(new List<List<double>>());

                // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    int notationId = notationIds[i];
                    int languageId = languageIds[i];
                    int? translatorId = translatorIds[i];
                    long chainId;

                    if (this.db.matter.Single(m => m.id == matterId).nature_id == 3)
                    {
                        isLiteratureChain = true;
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

                    Chain libiadaChain = this.chainRepository.ToLibiadaChain(chainId);
                    libiadaChain.FillIntervalManagers();
                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int linkId = linkIds[i];

                    string className = this.db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICalculator calculator = CalculatorsFactory.Create(className);
                    Link link = (Link)this.db.link.Single(l => l.id == linkId).id;
                    List<long> chainElements = this.chainRepository.GetElementIds(chainId);
                    int calculated = this.db.congeneric_characteristic.Count(b => b.chain_id == chainId &&
                                                                              b.characteristic_type_id == characteristicId &&
                                                                              b.link_id == linkId);
                    if (calculated < libiadaChain.Alphabet.Cardinality)
                    {

                        for (int j = 0; j < libiadaChain.Alphabet.Cardinality; j++)
                        {
                            long elementId = chainElements[j];

                            CongenericChain tempChain = libiadaChain.CongenericChain(j);

                            if (!this.db.congeneric_characteristic.Any(b =>
                                                                   b.chain_id == chainId &&
                                                                   b.characteristic_type_id == characteristicId &&
                                                                   b.element_id == elementId && b.link_id == linkId))
                            {
                                double value = calculator.Calculate(tempChain, link);
                                var currentCharacteristic = new congeneric_characteristic
                                {
                                    chain_id = chainId, 
                                    characteristic_type_id = characteristicId, 
                                    link_id = linkId, 
                                    element_id = elementId, 
                                    value = value, 
                                    value_string = value.ToString(), 
                                    created = DateTime.Now
                                };
                                this.db.congeneric_characteristic.Add(currentCharacteristic);
                                this.db.SaveChanges();
                            }
                        }
                    }

                    // Перебор всех элементов алфавита; третий уровень массива характеристик
                    for (int d = 0; d < libiadaChain.Alphabet.Cardinality; d++)
                    {
                        long elementId = chainElements[d];

                        double? characteristic = this.db.congeneric_characteristic.Single(b =>
                                    b.chain_id == chainId &&
                                    b.characteristic_type_id == characteristicId &&
                                    b.element_id == elementId &&
                                    b.link_id == linkId).value;

                        characteristics.Last().Last().Add(
                            new KeyValuePair<int, double>(d, (double)characteristic));

                        if (i == 0)
                        {
                            elementNames.Last().Add(libiadaChain.Alphabet[d].ToString());
                        }
                    }

                    // теоретические частоты по критерию Орлова
                    if (theoretical)
                    {

                        teoreticalRanks[w].Add(new List<double>());
                        ICalculator countCalculator = CalculatorsFactory.Create("Count");
                        var counts = new List<int>();
                        for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                        {
                            counts.Add((int)countCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                        }

                        ICalculator frequencyCalculator = CalculatorsFactory.Create("Probability");
                        var frequency = new List<double>();
                        for (int f = 0; f < libiadaChain.Alphabet.Cardinality; f++)
                        {
                            frequency.Add(frequencyCalculator.Calculate(libiadaChain.CongenericChain(f), Link.End));
                        }

                        double maxFrequency = frequency.Max();
                        double K = 1 / Math.Log(counts.Max());
                        double B = (K / maxFrequency) - 1;
                        int n = 1;
                        double Plow = libiadaChain.Length;
                        double P = K / (B + n);
                        while (P >= (1 / Plow))
                        {
                            teoreticalRanks.Last().Last().Add(P);
                            n++;
                            P = K / (B + n);
                        }
                    }
                }

            }

            // подписи для характеристик
            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int languageId = languageIds[k];
                string characteristicType = this.db.characteristic_type.Single(c => c.id == characteristicId).name;
                if (isLiteratureChain)
                {
                    string language = this.db.language.Single(l => l.id == languageId).name;
                    characteristicNames.Add(characteristicType + " " + language);
                }
                else
                {
                    characteristicNames.Add(characteristicType);
                }
            }

            // ранговая сортировка
            if (isSort)
            {
                for (int f = 0; f < matterIds.Length; f++)
                {
                    for (int p = 0; p < characteristics[f].Count; p++)
                    {
                        this.SortKeyValuePairList(characteristics[f][p]);
                    }
                }
            }

            this.TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "chainNames", chainNames }, 
                                         { "elementNames", elementNames }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "matterIds", matterIds }, 
                                         { "teoreticalRanks", teoreticalRanks }
                                     };

            return this.RedirectToAction("Result");
        }

        /// <summary>
        /// The sort key value pair list.
        /// </summary>
        /// <param name="arrayForSort">
        /// The array for sort.
        /// </param>
        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
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

                var characteristicNames = (List<string>)result["characteristicNames"];
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

                this.ViewBag.matterIds = result["matterIds"];
                this.ViewBag.characteristicsList = characteristicsList;
                this.ViewBag.characteristics = result["characteristics"];
                this.ViewBag.chainNames = result["chainNames"];
                this.ViewBag.elementNames = result["elementNames"];
                this.ViewBag.characteristicNames = characteristicNames;
                this.ViewBag.theoreticalRanks = result["teoreticalRanks"];

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