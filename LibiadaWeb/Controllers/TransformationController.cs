using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaCore.Classes.TheoryOfSet;
using LibiadaWeb.Models;
using PhantomChains.Classes.PhantomChains;

namespace LibiadaWeb.Controllers
{
    public class TransformationController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();
        private ChainRepository chainRepository = new ChainRepository();

        //
        // GET: /Transformation/

        public ActionResult Index()
        {
            ViewBag.chains = db.chain.Include("building_type").Include("matter").Include("notation").ToList();
            ViewBag.chainsList = chainRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] chainIds)
        {
            foreach (var chainId in chainIds)
            {
                Alphabet tempAlphabet = new Alphabet();
                IEnumerable<element> elements =
                    db.alphabet.Where(a => a.chain_id == chainId).Select(a => a.element);
                foreach (var element in elements)
                {
                    tempAlphabet.Add(new ValueString(element.value));
                }
                chain dbChain = db.chain.Single(c => c.id == chainId);
                Chain tempChain = new Chain(dbChain.building, tempAlphabet);
                BaseChain tempAminoChain = Coder.Encode(tempChain);
                chain result = new chain();

                String stringBuilding = "";
                for (int j = 0; j < tempAminoChain.Building.Length; j++)
                {
                    stringBuilding += tempAminoChain.Building[j] + "|";
                }
                stringBuilding = stringBuilding.Substring(0, stringBuilding.Length - 1);
                result.building = stringBuilding;

                for (int i = 0; i < tempAminoChain.Alphabet.power; i++)
                {
                    alphabet alphabetElement = new alphabet();
                    alphabetElement.chain = result;
                    alphabetElement.number = i + 1;
                    String strElem = tempAminoChain.Alphabet[i].ToString();
                    alphabetElement.element = db.element.Single(e => e.notation_id == 3 && e.value.Equals(strElem));
                    db.alphabet.AddObject(alphabetElement);
                }
                result.matter = dbChain.matter;
                result.building_type = dbChain.building_type;
                result.dissimilar = false;
                result.notation = db.notation.Single(n => n.name == "Аминокислоты");
                result.creation_date = new DateTimeOffset(DateTime.Now);
                db.chain.AddObject(result);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Chain");
        }
    }
}
