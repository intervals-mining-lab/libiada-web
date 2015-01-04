namespace LibiadaWeb.Controllers.Chains
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.DataTransformers;

    using Models;
    using Models.Repositories.Chains;

    /// <summary>
    /// The dna transformation controller.
    /// </summary>
    public class ChainTransformerController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The dna chain repository.
        /// </summary>
        private readonly DnaChainRepository dnaChainRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainTransformerController"/> class.
        /// </summary>
        public ChainTransformerController()
        {
            db = new LibiadaWebEntities();
            dnaChainRepository = new DnaChainRepository(db);
            chainRepository = new ChainRepository(db);
            elementRepository = new ElementRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var chains = db.dna_chain.Where(d => d.notation_id == Aliases.NotationNucleotide).Include("matter");
            ViewBag.chains = chains.ToList();
            ViewBag.chainsList = dnaChainRepository.GetSelectListItems(chains, null);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="chainIds">
        /// The chain ids.
        /// </param>
        /// <param name="toAmino">
        /// The to amino.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(IEnumerable<long> chainIds, bool toAmino)
        {
            int notationId = toAmino ? Aliases.NotationAminoAcid : Aliases.NotationTriplet;

            foreach (var chainId in chainIds)
            {
                chain dataBaseChain = db.chain.Single(c => c.id == chainId);
                Chain sourceChain = chainRepository.ToLibiadaChain(chainId);

                BaseChain transformedChain = toAmino
                                                 ? DnaTransformer.Encode(sourceChain)
                                                 : DnaTransformer.EncodeTriplets(sourceChain);

                var result = new dna_chain
                    {
                        matter_id = dataBaseChain.matter_id,
                        notation_id = notationId,
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