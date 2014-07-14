namespace LibiadaWeb.Controllers.Calculators
{
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The building comparison controller.
    /// </summary>
    public class BuildingComparisonController : Controller
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
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingComparisonController"/> class.
        /// </summary>
        public BuildingComparisonController()
        {
            db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(db);
            this.chainRepository = new ChainRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.mattersList = this.matterRepository.GetSelectListItems(null);

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="firstMatterId">
        /// The first matter id.
        /// </param>
        /// <param name="secondMatterId">
        /// The second matter id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="congeneric">
        /// The congeneric.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(long firstMatterId, long secondMatterId, int length, bool congeneric)
        {
            string chainName1 = db.matter.Single(m => m.id == firstMatterId).name;
            string chainName2 = db.matter.Single(m => m.id == secondMatterId).name;
            matter matter1 = db.matter.Single(m => m.id == firstMatterId);
            long chainId1 = matter1.chain.Single(c => c.notation_id == Aliases.NotationNucleotide).id;
            Chain libiadaChain1 = this.chainRepository.ToLibiadaChain(chainId1);
            matter matter2 = db.matter.Single(m => m.id == secondMatterId);
            long chainId2 = matter2.chain.Single(c => c.notation_id == Aliases.NotationNucleotide).id;
            Chain libiadaChain2 = this.chainRepository.ToLibiadaChain(chainId2);

            BaseChain res1 = null;
            BaseChain res2 = null;

            int i = 0;
            int j = 0;
            var iter1 = new IteratorStart(libiadaChain1, length, 1);
            bool duplicate = false;
            while (!duplicate && iter1.Next())
            {
                i++;
                var tempChain1 = (BaseChain)iter1.Current();
                var iter2 = new IteratorStart(libiadaChain2, length, 1);
                j = 0;
                while (!duplicate && iter2.Next())
                {
                    j++;
                    var tempChain2 = (BaseChain)iter2.Current();

                    if (congeneric)
                    {
                        for (int a = 0; a < tempChain1.Alphabet.Cardinality; a++)
                        {
                          /*  CongenericChain firstChain = tempChain1.CongenericChain(a);
                            for (int b = 0; b < tempChain2.Alphabet.Cardinality; b++)
                            {

                                CongenericChain secondChain = tempChain2.CongenericChain(b);
                                if (!firstChain.Equals(secondChain) && this.CompareBuldings(firstChain.Building, secondChain.Building))
                                {
                                    res1 = firstChain;
                                    res2 = secondChain;
                                    duplicate = true;
                                }
                            }*/
                        }
                    }
                    else
                    {
                        if (!tempChain1.Equals(tempChain2) && this.CompareBuldings(tempChain2.Building, tempChain1.Building))
                        {
                            res1 = tempChain1;
                            res2 = tempChain2;
                            duplicate = true;
                        }
                    }
                }
            }

            this.TempData["duplicate"] = duplicate;
            this.TempData["chainName1"] = chainName1;
            this.TempData["chainName2"] = chainName2;
            this.TempData["res1"] = res1;
            this.TempData["res2"] = res2;
            this.TempData["pos1"] = i;
            this.TempData["pos2"] = j;
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
            ViewBag.duplicate = this.TempData["duplicate"];
            if (ViewBag.duplicate)
            {
                ViewBag.chainName1 = this.TempData["chainName1"] as string;
                ViewBag.chainName2 = this.TempData["chainName2"] as string;
                ViewBag.res1 = this.TempData["res1"] as BaseChain;
                ViewBag.res2 = this.TempData["res2"] as BaseChain;
                ViewBag.pos1 = this.TempData["pos1"] is int ? (int)this.TempData["pos1"] : 0;
                ViewBag.pos2 = this.TempData["pos2"] is int ? (int)this.TempData["pos2"] : 0;
            }

            this.TempData.Keep();

            return View();
        }

        /// <summary>
        /// The compare buldings.
        /// </summary>
        /// <param name="firstBuilding">
        /// The first building.
        /// </param>
        /// <param name="secondbuilding">
        /// The secondbuilding.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CompareBuldings(int[] firstBuilding, int[] secondbuilding)
        {
            if (firstBuilding.Length != secondbuilding.Length)
            {
                return false;
            }

            for (int i = 0; i < firstBuilding.Length; i++)
            {
                if (firstBuilding[i] != secondbuilding[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}