using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Web.Mvc;
using LibiadaCore.Classes.Misc.Iterators;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class LocalCharacteristicsController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;


        public LocalCharacteristicsController()
        {
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /Transformation/

        public ActionResult Index()
        {
            ViewBag.characteristics = db.characteristic_type.ToList();

            ViewBag.linkUps = db.link_up.ToList();
            ViewBag.notations = db.notation.ToList();
            ViewBag.objects = db.matter.Include("chain").ToList();
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(null);
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);

            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, int[] characteristicIds, int[] linkUpIds, int notationId, int length,
                                  int step, bool isDelta, bool isSort, bool isFurie)
        {

            List<List<List<Double>>> characteristicsTemp = new List<List<List<Double>>>();
            List<String> chainNames = new List<string>();
            List<string> characteristicNames = new List<string>();
            List<List<String>> partNames = new List<List<String>>();


            for (int k = 0; k < matterIds.Length; k++)
            {
                long mattrId = matterIds[k];
                chainNames.Add(db.matter.Single(m => m.id == mattrId).name);
                partNames.Add(new List<string>());
                characteristicsTemp.Add(new List<List<double>>());

                matter matter = db.matter.Single(m => m.id == mattrId);
                chain chain = matter.chain.Single(c => c.building_type_id == 1 && c.notation_id == notationId);
                Chain libiadaChain = chainRepository.FromDbChainToLibiadaChain(chain);

                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    characteristicsTemp.Last().Add(new List<Double>());


                    int characteristicId = characteristicIds[i];
                    int linkUpId = linkUpIds[i];


                    String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                    LinkUp linkUp = (LinkUp)db.link_up.Single(l => l.id == linkUpId).id;

                    IteratorStart<Chain, Chain> iter = new IteratorStart<Chain, Chain>(libiadaChain, length, step);
                    while (iter.Next())
                    {
                        Chain tempChain = iter.Current();
                        partNames.Last().Add(tempChain.ToString());
                        characteristicsTemp.Last().Last().Add(calculator.Calculate(tempChain, linkUp));
                    }
                }

                if (isDelta)
                {
                    //Перебираем характеристики
                    for (int i = 0; i < characteristicsTemp.Count; i++)
                    {
                        //перебираем фрагменты цепочек
                        for (int j = (characteristicsTemp[i].Count) - 1; j > 0; j--)
                        {
                            characteristicsTemp.Last()[i][j] -= characteristicsTemp.Last()[i][j - 1];
                        }
                        characteristicsTemp.Last()[i].RemoveAt(0);
                    }
                }
                if (isSort)
                {
                    //Перебираем характеристики
                    for (int i = 0; i < characteristicsTemp.Count; i++)
                    {
                        //перебираем фрагменты цепочек
                        characteristicsTemp.Last()[i].Sort();
                    }
                }

                if (isFurie)
                {

                    //переводим в комлексный вид
                    for (int i = 0; i < characteristicsTemp.Count; i++)
                    {
                        List<Complex> comp = new List<Complex>();
                        int j = 0;

                        for (j = 0; j < characteristicsTemp[i].Count; j++)
                        {
                            comp.Add(new Complex(characteristicsTemp.Last()[i][j], 0));
                        }

                        int m = 1;

                        while (m < j)
                        {
                            m *= 2;
                        }

                        for (; j < m; j++)
                        {
                            comp.Add(new Complex(0, 0));
                        }

                        Complex[] data = FFT.Fft(comp.ToArray()); //вернёт массив
                        List<double> temp = new List<double>();
                        //переводим в массив double

                        foreach (var fftElement in data)
                        {
                            temp.Add(fftElement.Real);
                        }

                        characteristicsTemp.Last()[i] = temp;
                    }
                }
            }

            List<List<List<Double>>> characteristics = new List<List<List<Double>>>();

            //Перебираем цепочки
            for (int z = 0; z < matterIds.Length; z++)
            {
                characteristics.Add(new List<List<double>>());
                // перебираем характеристики
                for (int t = 0; t < characteristicsTemp[z][0].Count; t++)
                {
                    characteristics[z].Add(new List<double>());
                    //перебираем фрагменты
                    for (int w = 0; w < characteristicsTemp[z].Count; w++)
                    {
                        characteristics[z][t].Add(characteristicsTemp[z][w][t]);
                    }
                }
            }

            for (int i = 0; i < characteristicIds.Length; i++)
            {
                int characteristicId = characteristicIds[i];
                int linkUpId = linkUpIds[i];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                            db.link_up.Single(l => l.id == linkUpId).name);
            }

            TempData["characteristics"] = characteristics;
            TempData["chainNames"] = chainNames;
            TempData["partNames"] = partNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["characteristicNames"] = characteristicNames;
            TempData["chainIds"] = matterIds;
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<List<List<double>>> characteristics = TempData["characteristics"] as List<List<List<double>>>;
            ViewBag.chainIds = TempData["chainIds"] as List<long>;
            int[] characteristicIds = TempData["characteristicIds"] as int[];
            List<string> characteristicNames = TempData["characteristicNames"] as List<string>;
            List<SelectListItem> characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicIds.Length; i++)
            {
                characteristicsList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = characteristicNames[i],
                    Selected = false
                });
            }
            ViewBag.characteristicIds = new List<int>(characteristicIds);
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = characteristics;
            ViewBag.chainNames = TempData["chainNames"] as List<String>;
            ViewBag.partNames = TempData["partNames"] as List<List<String>>;
            ViewBag.characteristicNames = characteristicNames;
            return View();
        }
    }
}
