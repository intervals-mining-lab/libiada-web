﻿using System;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Misc.Iterators;
using LibiadaCore.Classes.Root;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class BuildingCompareController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;

        public BuildingCompareController()
        {
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /BuildingCompare/

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
        public ActionResult Index(long matterId1, long matterId2, int length)
        {
            String chainName1 = db.matter.Single(m => m.id == matterId1).name;
            String chainName2 = db.matter.Single(m => m.id == matterId2).name;
            matter matter1 = db.matter.Single(m => m.id == matterId1);
            chain chain1 = matter1.chain.Single(c => c.building_type_id == 1 && c.notation_id == 1);
            Chain libiadaChain1 = chainRepository.FromDbChainToLibiadaChain(chain1);
            matter matter2 = db.matter.Single(m => m.id == matterId2);
            chain chain2 = matter2.chain.Single(c => c.building_type_id == 1 && c.notation_id == 1);
            Chain libiadaChain2 = chainRepository.FromDbChainToLibiadaChain(chain2);

            Chain res1 = null;
            Chain res2 = null;

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
                    if (CompareBuldings(tempChain2.Building, tempChain1.Building) && !tempChain1.Equals(tempChain2))
                    {
                        res1 = tempChain1;
                        res2 = tempChain2;
                        duplicate = true;
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
            ViewBag.duplicate = TempData["duplicate"] is bool ? (bool) TempData["duplicate"] : false;
            if (ViewBag.duplicate)
            {
                ViewBag.chainName1 = TempData["chainName1"] as String;
                ViewBag.chainName2 = TempData["chainName2"] as String;
                ViewBag.res1 = TempData["res1"] as Chain;
                ViewBag.res2 = TempData["res2"] as Chain;
                ViewBag.pos1 = TempData["pos1"] is int ? (int)TempData["pos1"] : 0;
                ViewBag.pos2 = TempData["pos2"] is int ? (int)TempData["pos2"] : 0;
            }
            
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