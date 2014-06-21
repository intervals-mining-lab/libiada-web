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
    /// The genes calculation controller.
    /// </summary>
    public class GenesCalculationController : Controller
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
        /// The link repository.
        /// </summary>
        private readonly LinkRepository linkRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesCalculationController"/> class.
        /// </summary>
        public GenesCalculationController()
        {
            db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(db);
            this.characteristicRepository = new CharacteristicTypeRepository(db);
            this.notationRepository = new NotationRepository(db);
            this.linkRepository = new LinkRepository(db);
            this.chainRepository = new ChainRepository(db);
            this.pieceTypeRepository = new PieceTypeRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            IEnumerable<long> matterIds =
                db.dna_chain.Where(c => c.notation_id == Aliases.NotationNucleotide).Select(c => c.matter_id);
            IEnumerable<long> filterdMatterIds = matterIds.GroupBy(s => s).SelectMany(g => g.Skip(1));

            List<matter> matters = db.matter.ToList();
            matters = matters.Where(m => filterdMatterIds.Contains(m.id)).ToList();

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = this.characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var notationIds = db.notation.Where(n => n.nature_id == Aliases.NatureGenetic).Select(n => n.id).ToList();

            var pieceTypeIds =
                db.piece_type.Where(p => p.nature_id == Aliases.NatureGenetic && p.id != Aliases.PieceTypeFullGenome).Select(p => p.id);

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", new SelectList(matters, "id", "name") }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", this.notationRepository.GetSelectListWithNature(notationIds) }, 
                    { "links", new SelectList(db.link, "id", "name") }, 
                    { "matterCheckBoxes", this.matterRepository.GetSelectListItems(matters, null) }, 
                    { "pieceTypesCheckBoxes", this.pieceTypeRepository.GetSelectListWithNature(pieceTypeIds, pieceTypeIds) }
                };
            return View();
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
        /// <param name="pieceTypeIds">
        /// The piece type ids.
        /// </param>
        /// <param name="isSort">
        /// The is sort.
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
            int[] pieceTypeIds,
            bool isSort)
        {
            var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
            var chainNames = new List<string>();
            var chainsProduct = new List<List<string>>();
            var characteristicNames = new List<string>();

            // Перебор всех цепочек; первый уровень массива характеристик
            for (int w = 0; w < matterIds.Length; w++)
            {
                long matterId = matterIds[w];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                chainsProduct.Add(new List<string>());
                characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                var chains = db.dna_chain.Where(c => c.matter_id == matterId &&
                        pieceTypeIds.Contains(c.piece_type_id) &&
                        notationIds.Contains(c.notation_id)).ToArray();

                // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int linkId = linkIds[i];

                    string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICalculator calculator = CalculatorsFactory.Create(className);
                    Link link = (Link)db.link.Single(l => l.id == linkId).id;

                    for (int j = 0; j < chains.Length; j++)
                    {
                        long chainId = chains[j].id;

                        Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);
                        libiadaChain.FillIntervalManagers();

                        if (!db.characteristic.Any(b =>
                                                        b.chain_id == chainId &&
                                                        b.characteristic_type_id == characteristicId &&
                                                        b.link_id == linkId))
                        {
                            double value = calculator.Calculate(libiadaChain, link);
                            var currentCharacteristic = new characteristic
                            {
                                chain_id = chainId,
                                characteristic_type_id = characteristicId,
                                link_id = linkId,
                                value = value,
                                value_string = value.ToString()
                            };
                            db.characteristic.Add(currentCharacteristic);
                            db.SaveChanges();
                        }
                    }

                    for (int d = 0; d < chains.Length; d++)
                    {
                        long chainId = chains[d].id;
                        double? characteristic = db.characteristic.Single(b =>
                                    b.chain_id == chainId &&
                                    b.characteristic_type_id == characteristicId &&
                                    b.link_id == linkId).value;

                        characteristics.Last().Last().Add(
                            new KeyValuePair<int, double>(d, (double)characteristic));

                        if (i == 0)
                        {
                            chainsProduct.Last().Add(db.product.Single(p => chains[d].product_id == p.id).name);
                        }
                    }
                }
            }

            // подписи для характеристик
            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];

                string characteristicType = db.characteristic_type.Single(c => c.id == characteristicId).name;
                characteristicNames.Add(characteristicType);
            }

            // ранговая сортировка
            if (isSort)
            {
                for (int f = 0; f < matterIds.Length; f++)
                {
                    for (int p = 0; p < characteristics[f].Count; p++)
                    {
                        SortKeyValuePairList(characteristics[f][p]);
                    }
                }
            }

            this.TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "chainNames", chainNames }, 
                                         { "chainsProduct", chainsProduct }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "matterIds", matterIds }
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
                var result = this.TempData["result"] as Dictionary<string, object>;
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

                ViewBag.matterIds = result["matterIds"];
                ViewBag.characteristicsList = characteristicsList;
                ViewBag.characteristics = result["characteristics"];
                ViewBag.chainNames = result["chainNames"];
                ViewBag.chainsProduct = result["chainsProduct"];
                ViewBag.characteristicNames = characteristicNames;

                TempData.Keep();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);
            }

            return View();
        }
    }
}