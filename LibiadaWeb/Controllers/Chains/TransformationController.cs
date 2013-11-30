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
        private readonly LibiadaWebEntities db;
        private readonly DnaChainRepository dnaChainRepository;
        private readonly ChainRepository chainRepository;
        private readonly ElementRepository elementRepository;

        public TransformationController()
        {
            db = new LibiadaWebEntities();
            dnaChainRepository = new DnaChainRepository(db);
            chainRepository = new ChainRepository(db);
            elementRepository = new ElementRepository(db);
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
                chain dbChain = chainRepository.Find(chainId);
                Chain sourceChain = chainRepository.ToLibiadaChain(chainId);
                
                BaseChain transformedChain = toAmino
                                                 ? DnaTransformator.Encode(sourceChain)
                                                 : DnaTransformator.EncodeTriplets(sourceChain);

                dna_chain result = new dna_chain
                    {
                        matter_id = dbChain.matter_id,
                        dissimilar = false,
                        notation_id = notationId,
                        creation_date = DateTime.Now,
                        piece_type_id = Aliases.PieceTypeFullGenome,
                        piece_position = 0
                    };
                long[] alphabet = elementRepository.ToDbElements(transformedChain.Alphabet, notationId, false);
                dnaChainRepository.Insert(result, alphabet, transformedChain.Building);
            }
            return RedirectToAction("Index", "Chain");
        }
    }
}
