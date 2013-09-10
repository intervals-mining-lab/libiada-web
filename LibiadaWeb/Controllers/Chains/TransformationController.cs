using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Misc.DataTransformators;
using LibiadaCore.Classes.Root;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class TransformationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly DnaChainRepository dnaChainRepository;
        private readonly AlphabetRepository alphabetRepository;
        private readonly BuildingRepository buildingRepository;

        public TransformationController()
        {
            dnaChainRepository = new DnaChainRepository(db);
            alphabetRepository = new AlphabetRepository(db);
            buildingRepository = new BuildingRepository(db);
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
            int notationId = toAmino ? Aliases.NotationAminoAcid : Aliases.NotationTriplet;

            foreach (var chainId in chainIds)
            {
                dna_chain dbParentChain = db.dna_chain.Single(c => c.id == chainId);
                Chain tempChain = new Chain(buildingRepository.ToArray(dbParentChain.id),
                                            alphabetRepository.ToLibiadaAlphabet(dbParentChain.id));
                BaseChain transformedChain = toAmino
                                                 ? DnaTransformator.Encode(tempChain)
                                                 : DnaTransformator.EncodeTriplets(tempChain);

                dna_chain result = new dna_chain
                    {
                        id = db.ExecuteStoreQuery<long>("SELECT seq_next_value('chains_id_seq')").First(),
                        matter = dbParentChain.matter,
                        dissimilar = false,
                        notation_id = notationId,
                        creation_date = DateTime.Now
                    };
                db.dna_chain.AddObject(result);
                alphabetRepository.ToDbAlphabet(transformedChain.Alphabet, notationId, result.id,
                                                                   false);
                buildingRepository.ToDbBuilding(result.id, transformedChain.Building);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Chain");
        }
    }
}
