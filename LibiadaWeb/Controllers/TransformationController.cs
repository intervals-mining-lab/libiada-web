using System;
using System.Data.Entity;
using System.Linq;
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
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly DnaChainRepository dnaChainRepository;
        private readonly AlphabetRepository alphabetRepository;

        public TransformationController()
        {
            dnaChainRepository = new DnaChainRepository(db);
            alphabetRepository = new AlphabetRepository(db);
        }

        //
        // GET: /Transformation/

        public ActionResult Index()
        {
            var chains = db.dna_chain.Where(d => d.notation_id == 1 && d.dissimilar == false).Include("matter");
            ViewBag.chains = chains.ToList();
            ViewBag.chainsList = dnaChainRepository.GetSelectListItems(chains, null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] chainIds, bool toAmino)
        {
            int notationId;
            if (toAmino)
            {
                notationId = 3;
            }
            else
            {
                notationId = 2;
            }

            foreach (var chainId in chainIds)
            {
                dna_chain dbParentChain = db.dna_chain.Single(c => c.id == chainId);
                Chain tempChain = new Chain(dnaChainRepository.FromDbBuildingToLibiadaBuilding(dbParentChain),
                                            alphabetRepository.FromDbAlphabetToLibiadaAlphabet(dbParentChain.id));
                BaseChain transformedChain;
                if (toAmino)
                {
                    transformedChain = Coder.Encode(tempChain);
                }
                else
                {
                    transformedChain = Coder.EncodeTriplets(tempChain);
                }

                dna_chain result = new dna_chain()
                    {
                        id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                        matter = dbParentChain.matter,
                        building_type = dbParentChain.building_type,
                        dissimilar = false,
                        notation_id = notationId,
                        creation_date = DateTime.Now
                    };
                db.dna_chain.AddObject(result);
                alphabetRepository.FromLibiadaAlphabetToDbAlphabet(transformedChain.Alphabet, notationId, result.id,
                                                                   false);
                dnaChainRepository.FromLibiadaBuildingToDbBuilding(result, transformedChain.Building);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Chain");
        }
    }
}
