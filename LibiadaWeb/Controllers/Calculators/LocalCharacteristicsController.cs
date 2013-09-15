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
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class LocalCharacteristicsController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;


        public LocalCharacteristicsController()
        {
            db = new LibiadaWebEntities();
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
            IEnumerable<characteristic_type> characteristics =
                db.characteristic_type.Where(c => new List<int> { 1, 4, 5, 7 }.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristics, null);
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.languagesList = new SelectList(db.language, "id", "name");

            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, int[] characteristicIds, int[] linkUpIds, int languageId, int notationId, int length,
                                  int step, int startCoordinate, int beginOfChain, int endOfChain, bool isDelta, bool isFurie, bool isGrowingWindow, bool isMoveCoordinate, bool isSetBeginAndEnd)
        {

            List<List<List<Double>>> characteristics = CalculateCharacteristics(matterIds, isSetBeginAndEnd, isGrowingWindow,
                                             notationId, languageId, length, characteristicIds, linkUpIds, step, beginOfChain, endOfChain);
   
            List<String> chainNames = new List<string>();
            List<string> characteristicNames = new List<string>();
            List<List<String>> partNames = new List<List<String>>();


            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                partNames.Add(new List<string>());

                long chainId;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    chainId = db.literature_chain.Single(l => l.matter_id == matterId &&
                                l.notation_id == notationId
                                && l.language_id == languageId).id;
                }
                else
                {
                    chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                }

                Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);
                
                CutRule cutRule;
                if (isSetBeginAndEnd)
                {

                    cutRule = isGrowingWindow ? (CutRule)new FromFixStartCutRuleWithBeginEnd(endOfChain - beginOfChain, step, beginOfChain) : new SimpleCutRuleWithBeginEnd(endOfChain - beginOfChain, step, length, beginOfChain);
                }
                else
                {
                    cutRule = isGrowingWindow ? (CutRule)new FromFixStartCutRule(libiadaChain.Length, step) : new SimpleCutRule(libiadaChain.Length, step, length);
                }
                CutRuleIterator iter = cutRule.GetIterator();
               
                while (iter.Next())
                {
                    Chain tempChain = new Chain();
                    tempChain.ClearAndSetNewLength(iter.GetStopPos() - iter.GetStartPos());

                    for (int i = 0; iter.GetStartPos() + i < iter.GetStopPos(); i++)
                    {
                        tempChain.Add(libiadaChain[iter.GetStartPos() + i], i);
                    }
                    partNames.Last().Add(tempChain.ToString());
                }
                if (isMoveCoordinate)
                {
                    List<List<List<Double>>> characteristicsParts = CalculateCharacteristics(matterIds, isSetBeginAndEnd,isGrowingWindow,
                                             notationId, languageId, startCoordinate, characteristicIds, linkUpIds, step, beginOfChain, endOfChain);
                    for (int i = 0; i < characteristics.Count; i++)
                    {
                        for (int j = 0; j < characteristics[i].Count; j++)
                        {
                            for (int l = 0; l < characteristics[i][j].Count; l++)
                            {
                                characteristics[i][j][l] -= characteristicsParts[i][j][l];
                            }
                        }
                    }
                    
                }

                if (isDelta)
                {
                    //Перебираем характеристики 
                    for (int i = 0; i < characteristics.Last().Last().Count; i++)
                    {
                        //перебираем фрагменты цепочек
                        for (int j = (characteristics.Last().Count) - 1; j > 0; j--)
                        {
                            characteristics.Last()[j][i] -= characteristics.Last()[j - 1][i];
                        }
                    }
                    characteristics.Last().RemoveAt(0);
                }

                if (isFurie)
                {

                    //переводим в комлексный вид
                    // Для всех характеристик
                    for (int i = 0; i < characteristics.Last().Last().Count; i++)
                    {
                        List<Complex> comp = new List<Complex>();
                        int j;

                        //Для всех фрагментов цепочек
                        for (j = 0; j < characteristics.Last().Count; j++)
                        {
                            comp.Add(new Complex(characteristics.Last()[j][i], 0));
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

                        //переводим в массив double
                        for (int g = 0; g < characteristics.Last().Count; g++ )
                        {
                            characteristics.Last()[g][i] = data[g].Real;
                        }
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

        private List<List<List<Double>>> CalculateCharacteristics(long[] matterIds, bool isSetBeginAndEnd,
                                                                  bool isGrowingWindow, int notationId, int languageId,
                                                                  int length, int[] characteristicIds, int[] linkUpIds, int step, int beginOfChain, int endOfChain)
        {
            List<List<List<Double>>> characteristics = new List<List<List<Double>>>();
            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                characteristics.Add(new List<List<double>>());

                long chainId;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    chainId = db.literature_chain.Single(l => l.matter_id == matterId &&
                                                              l.notation_id == notationId
                                                              && l.language_id == languageId).id;
                }
                else
                {
                    chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                }

                Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);

                CutRule cutRule;
                if (isSetBeginAndEnd)
                {

                    cutRule = isGrowingWindow ? (CutRule)new FromFixStartCutRuleWithBeginEnd(endOfChain - beginOfChain, step, beginOfChain) : new SimpleCutRuleWithBeginEnd(endOfChain - beginOfChain, step, length, beginOfChain);
                }
                else
                {
                    cutRule = isGrowingWindow ? (CutRule)new FromFixStartCutRule(libiadaChain.Length, step) : new SimpleCutRule(libiadaChain.Length, step, length);
                }
                CutRuleIterator iter = cutRule.GetIterator();
               

                while (iter.Next())
                {
                    characteristics.Last().Add(new List<Double>());
                    Chain tempChain = new Chain();
                    tempChain.ClearAndSetNewLength(iter.GetStopPos() - iter.GetStartPos());

                    for (int i = 0; iter.GetStartPos() + i < iter.GetStopPos(); i++)
                    {
                        tempChain.Add(libiadaChain[iter.GetStartPos() + i], i);
                    }
                    for (int i = 0; i < characteristicIds.Length; i++)
                    {
                        int characteristicId = characteristicIds[i];
                        int linkUpId = linkUpIds[i];
                        String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

                        ICalculator calculator = CalculatorsFactory.Create(className);
                        LinkUp linkUp = (LinkUp) db.link_up.Single(l => l.id == linkUpId).id;
                        characteristics.Last().Last().Add(calculator.Calculate(tempChain, linkUp));
                    }
                }
            }
            return characteristics;
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

            TempData.Keep();
            
            return View();
        }
    }
}
