namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    using Microsoft.Ajax.Utilities;

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
            var chainIds = db.gene.Select(g => g.chain_id).Distinct().ToList();
            var chains = db.dna_chain.Where(c => chainIds.Contains(c.id));
                var matterIds = chains.Select(c => c.matter_id);

            List<matter> matters = db.matter.Where(m => matterIds.Contains(m.id)).ToList();

            IEnumerable<characteristic_type> characteristicsList = db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = this.characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var pieceTypeIds =
                db.piece_type.Where(p => p.nature_id == Aliases.NatureGenetic 
                                         && p.id != Aliases.PieceTypeFullGenome 
                                         && p.id != Aliases.PieceTypeChloroplastGenome 
                                         && p.id != Aliases.PieceTypeMitochondrionGenome).Select(p => p.id);

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", new SelectList(matters, "id", "name") }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", new SelectList(db.notation.Where(n => n.nature_id == Aliases.NatureGenetic), "id", "name") }, 
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
            var chainsPosition = new List<List<long>>();
            var chainsPieceTypes = new List<List<string>>();
            var characteristicNames = new List<string>();

            // Перебор всех цепочек; первый уровень массива характеристик
            for (int w = 0; w < matterIds.Length; w++)
            {
                long matterId = matterIds[w];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                chainsProduct.Add(new List<string>());
                chainsPosition.Add(new List<long>());
                chainsPieceTypes.Add(new List<string>());
                characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                var notationId = notationIds[w];

                var chainId = db.dna_chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;

                var genes = db.gene.Where(g => g.chain_id == chainId && pieceTypeIds.Contains(g.piece_type_id)).Include("piece").ToArray();

                var pieces = genes.Select(g => g.piece).First().ToList();

                var starts = pieces.Select(p => p.start).ToList();

                var stops = pieces.Select(p => p.start + p.length).ToList();

                BaseChain parentChain = chainRepository.ToLibiadaBaseChain(chainId);

                var iterator = new DefaultCutRule(starts, stops);

                var stringChains = DiffCutter.Cut(parentChain.ToString(), iterator);

                var chains = new List<Chain>();

                for (int i = 0; i < stringChains.Count; i++)
                {
                    chains.Add(new Chain(stringChains[i]));
                }

                // Перебор всех характеристик и форм записи; второй уровень массива характеристик
                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int linkId = linkIds[i];

                    string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                    Link link = (Link)db.link.Single(l => l.id == linkId).id;

                    for (int j = 0; j < chains.Count; j++)
                    {
                        long geneId = genes[j].id;

                        if (!db.characteristic.Any(b => b.chain_id == geneId &&
                                                        b.characteristic_type_id == characteristicId &&
                                                        b.link_id == linkId))
                        {
                            double value = calculator.Calculate(chains[i], link);
                            var currentCharacteristic = new characteristic
                            {
                                chain_id = geneId,
                                characteristic_type_id = characteristicId,
                                link_id = linkId,
                                value = value,
                                value_string = value.ToString()
                            };

                            db.characteristic.Add(currentCharacteristic);
                            db.SaveChanges();
                        }
                    }

                    for (int d = 0; d < chains.Count; d++)
                    {
                        long geneId = genes[d].id;
                        double? characteristic = db.characteristic.Single(b =>
                                    b.chain_id == geneId &&
                                    b.characteristic_type_id == characteristicId &&
                                    b.link_id == linkId).value;

                        characteristics.Last().Last().Add(new KeyValuePair<int, double>(d, (double)characteristic));

                        if (i == 0)
                        {
                            var productId = genes[d].product_id;
                            var pieceTypeId = genes[d].piece_type_id;

                            chainsProduct.Last().Add(productId == null ? string.Empty : db.product.Single(p => productId == p.id).name);
                            chainsPosition.Last().Add(pieces[d].start);

                            chainsPieceTypes.Last().Add(db.piece_type.Single(p => pieceTypeId == p.id).name);
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
                                         { "chainsPosition", chainsPosition }, 
                                         { "chainsPieceTypes", chainsPieceTypes }, 
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
                ViewBag.chainsPosition = result["chainsPosition"];  
                ViewBag.chainsPieceTypes = result["chainsPieceTypes"];
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