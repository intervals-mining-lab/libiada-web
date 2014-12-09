namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Math;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The local calculation controller.
    /// </summary>
    public class LocalCalculationController : Controller
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
        /// Initializes a new instance of the <see cref="LocalCalculationController"/> class.
        /// </summary>
        public LocalCalculationController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkRepository = new LinkRepository(db);
            chainRepository = new ChainRepository(db);
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
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <param name="isDelta">
        /// The is delta.
        /// </param>
        /// <param name="isFourier">
        /// The Fourier transform flag.
        /// </param>
        /// <param name="isGrowingWindow">
        /// The is growing window.
        /// </param>
        /// <param name="isAutocorrelation">
        /// The is autocorelation.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicIds, 
            int[] linkIds, 
            int languageId, 
            int notationId, 
            int length, 
            int step,
            bool isDelta,
            bool isFourier, 
            bool isGrowingWindow,
            bool isAutocorrelation)
        {
            List<List<List<double>>> characteristics = CalculateCharacteristics(
                matterIds,
                isGrowingWindow, 
                notationId, 
                languageId, 
                length, 
                characteristicIds, 
                linkIds, 
                step);

            var chainNames = new List<string>();
            var characteristicNames = new List<string>();
            var partNames = new List<List<string>>();
            var starts = new List<List<int>>();
            var lengthes = new List<List<int>>();

            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                partNames.Add(new List<string>());
                starts.Add(new List<int>());
                lengthes.Add(new List<int>());

                long chainId;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    chainId =
                        db.literature_chain.Single(l => l.matter_id == matterId 
                                                && l.notation_id == notationId 
                                                && l.language_id == languageId).id;
                }
                else
                {
                    chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                }

                Chain libiadaChain = this.chainRepository.ToLibiadaChain(chainId);

                CutRule cutRule = isGrowingWindow
                                  ? (CutRule)new CutRuleWithFixedStart(libiadaChain.GetLength(), step)
                                  : new SimpleCutRule(libiadaChain.GetLength(), step, length);

                CutRuleIterator iter = cutRule.GetIterator();

                while (iter.Next())
                {
                    var tempChain = new Chain(iter.GetEndPosition() - iter.GetStartPosition());

                    for (int i = 0; iter.GetStartPosition() + i < iter.GetEndPosition(); i++)
                    {
                        tempChain.Set(libiadaChain[iter.GetStartPosition() + i], i);
                    }

                    partNames.Last().Add(tempChain.ToString());
                    starts.Last().Add(iter.GetStartPosition());
                    lengthes.Last().Add(tempChain.GetLength());
                }

                if (isDelta)
                {
                    CalculateDelta(characteristics);
                }

                if (isFourier)
                {
                    FastFourierTransform.FourierTransform(characteristics);
                }
            }

            if (isAutocorrelation)
            {
                var autoCorrelation = new AutoCorrelation();
                autoCorrelation.CalculateAutocorrelation(characteristics);
            }

            for (int i = 0; i < characteristicIds.Length; i++)
            {
                int characteristicId = characteristicIds[i];
                int linkId = linkIds[i];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " + db.link.Single(l => l.id == linkId).name);
            }

            var characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicIds.Length; i++)
            {
                characteristicsList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = characteristicNames[i],
                    Selected = false
                });
            }
            
            TempData["result"] = new Dictionary<string, object>
                                     {
                                        { "characteristics", characteristics },
                                        { "chainNames", chainNames },
                                        //{ "partNames", partNames },
                                        { "starts", starts },
                                        { "lengthes", lengthes },
                                        { "characteristicIds", new List<int>(characteristicIds) },
                                        { "characteristicNames", characteristicNames },
                                        { "chainIds", matterIds },
                                        { "characteristicsList", characteristicsList }
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
        /// Thrown if there is no data.
        /// </exception>
        public ActionResult Result()
        {
            try
            {
                var result = this.TempData["result"] as Dictionary<string, object>;
                if (result == null)
                {
                    throw new Exception("No data.");
                }

                foreach (var key in result.Keys)
                {
                    ViewData[key] = result[key];
                }

                this.TempData.Keep();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("Error", e.Message);

                ViewBag.Error = true;

                ViewBag.ErrorMessage = e.Message;
            }

            return View();
        }

        /// <summary>
        /// The calculate delta.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        private static void CalculateDelta(List<List<List<double>>> characteristics)
        {
            // Перебираем характеристики 
            for (int i = 0; i < characteristics.Last().Last().Count; i++)
            {
                // перебираем фрагменты цепочек
                for (int j = characteristics.Last().Count - 1; j > 0; j--)
                {
                    characteristics.Last()[j][i] -= characteristics.Last()[j - 1][i];
                }
            }

            characteristics.Last().RemoveAt(0);
        }

        /// <summary>
        /// The calculate characteristics.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="isGrowingWindow">
        /// The is growing window.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="step">
        /// The step.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<List<List<double>>> CalculateCharacteristics(
            long[] matterIds,
            bool isGrowingWindow,
            int notationId,
            int languageId,
            int length,
            int[] characteristicIds,
            int[] linkIds,
            int step)
        {
            var characteristics = new List<List<List<double>>>();
            for (int k = 0; k < matterIds.Length; k++)
            {
                long matterId = matterIds[k];
                characteristics.Add(new List<List<double>>());

                long chainId;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    chainId = db.literature_chain.Single(l => l.matter_id == matterId && 
                                                              l.notation_id == notationId && 
                                                              l.language_id == languageId).id;
                }
                else
                {
                    chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                }

                Chain libiadaChain = this.chainRepository.ToLibiadaChain(chainId);

                CutRule cutRule;


                cutRule = isGrowingWindow
                                ? (CutRule)new CutRuleWithFixedStart(libiadaChain.GetLength(), step)
                                : new SimpleCutRule(libiadaChain.GetLength(), step, length);

                CutRuleIterator iter = cutRule.GetIterator();


                while (iter.Next())
                {
                    characteristics.Last().Add(new List<double>());
                    var tempChain = new Chain();
                    tempChain.ClearAndSetNewLength(iter.GetEndPosition() - iter.GetStartPosition());

                    for (int i = 0; iter.GetStartPosition() + i < iter.GetEndPosition(); i++)
                    {
                        tempChain.Set(libiadaChain[iter.GetStartPosition() + i], i);
                    }

                    for (int i = 0; i < characteristicIds.Length; i++)
                    {
                        int characteristicId = characteristicIds[i];
                        int linkId = linkIds[i];
                        string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = (Link)db.link.Single(l => l.id == linkId).id;
                        characteristics.Last().Last().Add(calculator.Calculate(tempChain, link));
                    }
                }
            }

            return characteristics;
        }
    }
}