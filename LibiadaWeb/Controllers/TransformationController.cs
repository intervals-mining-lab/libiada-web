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
            int notationId = 3;
            foreach (var chainId in chainIds)
            {
                Alphabet tempAlphabet = new Alphabet();
                //tempAlphabet.Add(NullValue.Instance());
                IEnumerable<element> elements =
                    db.alphabet.Where(a => a.chain_id == chainId).Select(a => a.element);
                foreach (var element in elements)
                {
                    tempAlphabet.Add(new ValueString(element.value));
                }
                chain dbChain = db.chain.Single(c => c.id == chainId);
                Chain tempChain = new Chain(dbChain.building.OrderBy(b => b.index).Select(b => b.number).ToArray(), tempAlphabet);
                BaseChain tempTripletChain = Coder.Encode(tempChain);
                chain result = new chain();
                int[] build = tempTripletChain.Building;
                for (int i = 0; i < build.Length; i++)
                {
                    building buildingElement = new building();
                    buildingElement.chain = result;
                    buildingElement.index = i;
                    buildingElement.number = build[i];
                    db.building.AddObject(buildingElement);
                }

                for (int i = 0; i < tempTripletChain.Alphabet.Power; i++)
                {
                    String strElem = tempTripletChain.Alphabet[i].ToString();
                    element elem;
                    if (db.element.Any(e => e.notation_id == notationId && e.value.Equals(strElem)))
                    {
                        elem = db.element.Single(e => e.notation_id == notationId && e.value.Equals(strElem));
                    }
                    else
                    {
                        elem = new element();
                        elem.name = strElem;
                        elem.value = strElem;
                        elem.notation_id = 3;
                        elem.creation_date = new DateTimeOffset(DateTime.Now);
                        db.element.AddObject(elem);
                    }
                    alphabet alphabetElement = new alphabet();
                    alphabetElement.chain = result;
                    alphabetElement.number = i + 1;
                    alphabetElement.element = elem;
                    db.alphabet.AddObject(alphabetElement);
                }
                result.matter = dbChain.matter;
                result.building_type = dbChain.building_type;
                result.dissimilar = false;
                result.notation_id = notationId;
                result.creation_date = new DateTimeOffset(DateTime.Now);
                db.chain.AddObject(result);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Chain");
        }
    }
}
