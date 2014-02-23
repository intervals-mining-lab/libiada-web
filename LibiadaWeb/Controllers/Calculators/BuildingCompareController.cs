using System;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Misc.Iterators;
using LibiadaCore.Classes.Root;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class BuildingCompareController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly MatterRepository matterRepository;
        private readonly ChainRepository chainRepository;

        public BuildingCompareController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /BuildingCompare/

        public ActionResult Index()
        {
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);

            return View();
        }

        [HttpPost]
        public ActionResult Index(long matterId1, long matterId2, int length, bool congeneric)
        {
            String chainName1 = db.matter.Single(m => m.id == matterId1).name;
            String chainName2 = db.matter.Single(m => m.id == matterId2).name;
            matter matter1 = db.matter.Single(m => m.id == matterId1);
            long chainId1 = matter1.chain.Single(c => c.notation_id == Aliases.NotationNucleotide).id;
            Chain libiadaChain1 = chainRepository.ToLibiadaChain(chainId1);
            matter matter2 = db.matter.Single(m => m.id == matterId2);
            long chainId2 = matter2.chain.Single(c => c.notation_id == Aliases.NotationNucleotide).id;
            Chain libiadaChain2 = chainRepository.ToLibiadaChain(chainId2);

            BaseChain res1 = null;
            BaseChain res2 = null;

            int i = 0;
            int j = 0;
            IteratorStart<Chain, Chain> iter1 = new IteratorStart<Chain, Chain>(libiadaChain1, length, 1);
            bool duplicate = false;
            while (!duplicate && iter1.Next())
            {
                i++;
                Chain tempChain1 = iter1.Current();
                IteratorStart<Chain, Chain> iter2 = new IteratorStart<Chain, Chain>(libiadaChain2, length, 1);
                j = 0;
                while (!duplicate && iter2.Next())
                {
                    j++;
                    Chain tempChain2 = iter2.Current();

                    if (congeneric)
                    {
                        for (int a = 0; a < tempChain1.Alphabet.Cardinality; a++)
                        {
                            CongenericChain firstChain = tempChain1.CongenericChain(a);
                            for (int b = 0; b < tempChain2.Alphabet.Cardinality; b++)
                            {

                                CongenericChain secondChain = tempChain2.CongenericChain(b);
                                if (!firstChain.Equals(secondChain) && CompareBuldings(firstChain.Building, secondChain.Building))
                                {
                                    res1 = firstChain;
                                    res2 = secondChain;
                                    duplicate = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!tempChain1.Equals(tempChain2) && CompareBuldings(tempChain2.Building, tempChain1.Building))
                        {
                            res1 = tempChain1;
                            res2 = tempChain2;
                            duplicate = true;
                        }
                    }
                }
            }

            TempData["duplicate"] = duplicate;
            TempData["chainName1"] = chainName1;
            TempData["chainName2"] = chainName2;
            TempData["res1"] = res1;
            TempData["res2"] = res2;
            TempData["pos1"] = i;
            TempData["pos2"] = j;
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            ViewBag.duplicate = TempData["duplicate"] ;
            if (ViewBag.duplicate)
            {
                ViewBag.chainName1 = TempData["chainName1"] as String;
                ViewBag.chainName2 = TempData["chainName2"] as String;
                ViewBag.res1 = TempData["res1"] as BaseChain;
                ViewBag.res2 = TempData["res2"] as BaseChain;
                ViewBag.pos1 = TempData["pos1"] is int ? (int) TempData["pos1"] : 0;
                ViewBag.pos2 = TempData["pos2"] is int ? (int) TempData["pos2"] : 0;
            }

            TempData.Keep();

            return View();
        }

        private bool CompareBuldings(int[] building1, int[] building2)
        {
            if (building1.Length != building2.Length)
            {
                return false;
            }
            for (int i = 0; i < building1.Length; i++)
            {
                if (building1[i] != building2[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}
