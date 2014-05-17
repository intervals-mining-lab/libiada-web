namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    public class LocalCalculationController : Controller
    {
        private readonly LibiadaWebEntities db;

        private readonly MatterRepository matterRepository;

        private readonly CharacteristicTypeRepository characteristicRepository;

        private readonly NotationRepository notationRepository;

        private readonly LinkRepository linkRepository;

        private readonly ChainRepository chainRepository;


        public LocalCalculationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkRepository = new LinkRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /LocalCalculation/
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);
            List<matter> matters = db.matter.Include("nature").ToList();
            ViewBag.matterCheckBoxes = matterRepository.GetSelectListItems(matters, null);
            ViewBag.matters = matters;

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristicsList, null);

            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linksList = linkRepository.GetSelectListItems(null);
            ViewBag.languagesList = new SelectList(db.language, "id", "name");

            return View();
        }

        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicIds,
            int[] linkIds,
            int languageId,
            int notationId,
            int length,
            int step,
            int startCoordinate,
            int beginOfChain,
            int endOfChain,
            bool isDelta,
            bool isFurie,
            bool isGrowingWindow,
            bool isMoveCoordinate,
            bool isSetBeginAndEnd,
            bool isAutocorelation)
        {

            List<List<List<double>>> characteristics = CalculateCharacteristics(
                matterIds,
                isSetBeginAndEnd,
                isGrowingWindow,
                notationId,
                languageId,
                length,
                characteristicIds,
                linkIds,
                step,
                beginOfChain,
                endOfChain);

            var chainNames = new List<string>();
            var characteristicNames = new List<string>();
            var partNames = new List<List<string>>();


            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                partNames.Add(new List<string>());

                long chainId;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    chainId =
                        db.literature_chain.Single(
                            l => l.matter_id == matterId && l.notation_id == notationId && l.language_id == languageId)
                            .id;
                }
                else
                {
                    chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                }

                Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);

                CutRule cutRule;
                if (isSetBeginAndEnd)
                {

                    cutRule = isGrowingWindow
                                  ? (CutRule)
                                    new CutRuleWithShiftedAndFixedStart(endOfChain - beginOfChain, step, beginOfChain)
                                  : new SimpleCutRuleWithShiftedStart(
                                        endOfChain - beginOfChain,
                                        step,
                                        length,
                                        beginOfChain);
                }
                else
                {
                    cutRule = isGrowingWindow
                                  ? (CutRule)new CutRuleWithFixedStart(libiadaChain.Length, step)
                                  : new SimpleCutRule(libiadaChain.Length, step, length);
                }
                CutRuleIterator iter = cutRule.GetIterator();

                while (iter.Next())
                {
                    var tempChain = new Chain();
                    tempChain.ClearAndSetNewLength(iter.GetEndPosition() - iter.GetStartPosition());

                    for (int i = 0; iter.GetStartPosition() + i < iter.GetEndPosition(); i++)
                    {
                        tempChain.Add(libiadaChain[iter.GetStartPosition() + i], i);
                    }
                    partNames.Last().Add(tempChain.ToString());
                }
                if (isMoveCoordinate)
                {
                    List<List<List<double>>> characteristicsParts = CalculateCharacteristics(
                        matterIds,
                        isSetBeginAndEnd,
                        isGrowingWindow,
                        notationId,
                        languageId,
                        startCoordinate,
                        characteristicIds,
                        linkIds,
                        step,
                        beginOfChain,
                        endOfChain);
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
                        for (int g = 0; g < characteristics.Last().Count; g++)
                        {
                            characteristics.Last()[g][i] = data[g].Real;
                        }
                    }
                }
            }
            if (isAutocorelation)
            {
                AutoCorrelation autoCorellation = new AutoCorrelation();
                for (int i = 0; i < characteristics.Last().Last().Count; i++)
                {
                    double[] temp = new double[characteristics.Last().Count];
                    //Для всех фрагментов цепочек
                    for (int j = 0; j < characteristics.Last().Count; j++)
                    {
                        temp[j] = characteristics.Last()[j][i];
                    }

                    double[] res = autoCorellation.Execute(temp);
                    for (int j = 0; j < res.Length; j++)
                    {
                        characteristics.Last()[j][i] = res[j];
                    }
                    characteristics.Last().RemoveRange(res.Length, characteristics.Last().Count - res.Length);
                }
            }
            for (int i = 0; i < characteristicIds.Length; i++)
            {
                int characteristicId = characteristicIds[i];
                int linkId = linkIds[i];
                characteristicNames.Add(
                    db.characteristic_type.Single(c => c.id == characteristicId).name + " "
                    + db.link.Single(l => l.id == linkId).name);
            }

            TempData["characteristics"] = characteristics;
            TempData["chainNames"] = chainNames;
            TempData["partNames"] = partNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["characteristicNames"] = characteristicNames;
            TempData["chainIds"] = matterIds;
            return RedirectToAction("Result");
        }

        private List<List<List<double>>> CalculateCharacteristics(
            long[] matterIds,
            bool isSetBeginAndEnd,
            bool isGrowingWindow,
            int notationId,
            int languageId,
            int length,
            int[] characteristicIds,
            int[] linkIds,
            int step,
            int beginOfChain,
            int endOfChain)
        {
            var characteristics = new List<List<List<double>>>();
            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                characteristics.Add(new List<List<double>>());

                long chainId;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    chainId =
                        db.literature_chain.Single(
                            l => l.matter_id == matterId && l.notation_id == notationId && l.language_id == languageId)
                            .id;
                }
                else
                {
                    chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                }

                Chain libiadaChain = chainRepository.ToLibiadaChain(chainId);

                CutRule cutRule;
                if (isSetBeginAndEnd)
                {

                    cutRule = isGrowingWindow
                                  ? (CutRule)
                                    new CutRuleWithShiftedAndFixedStart(endOfChain - beginOfChain, step, beginOfChain)
                                  : new SimpleCutRuleWithShiftedStart(
                                        endOfChain - beginOfChain,
                                        step,
                                        length,
                                        beginOfChain);
                }
                else
                {
                    cutRule = isGrowingWindow
                                  ? (CutRule)new CutRuleWithFixedStart(libiadaChain.Length, step)
                                  : new SimpleCutRule(libiadaChain.Length, step, length);
                }
                CutRuleIterator iter = cutRule.GetIterator();


                while (iter.Next())
                {
                    characteristics.Last().Add(new List<Double>());
                    var tempChain = new Chain();
                    tempChain.ClearAndSetNewLength(iter.GetEndPosition() - iter.GetStartPosition());

                    for (int i = 0; iter.GetStartPosition() + i < iter.GetEndPosition(); i++)
                    {
                        tempChain.Add(libiadaChain[iter.GetStartPosition() + i], i);
                    }
                    for (int i = 0; i < characteristicIds.Length; i++)
                    {
                        int characteristicId = characteristicIds[i];
                        int linkId = linkIds[i];
                        string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

                        ICalculator calculator = CalculatorsFactory.Create(className);
                        var link = (Link)db.link.Single(l => l.id == linkId).id;
                        characteristics.Last().Last().Add(calculator.Calculate(tempChain, link));
                    }
                }
            }
            return characteristics;
        }

        public ActionResult Result()
        {
            var characteristics = TempData["characteristics"] as List<List<List<double>>>;
            ViewBag.chainIds = TempData["chainIds"] as List<long>;
            int[] characteristicIds = TempData["characteristicIds"] as int[];
            var characteristicNames = TempData["characteristicNames"] as List<string>;
            var characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicIds.Length; i++)
            {
                characteristicsList.Add(
                    new SelectListItem { Value = i.ToString(), Text = characteristicNames[i], Selected = false });
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