namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The chain mixer controller.
    /// </summary>
    public class ChainMixerController : Controller
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
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The dna chain repository.
        /// </summary>
        private readonly DnaChainRepository dnaChainRepository;

        /// <summary>
        /// The literature chain repository.
        /// </summary>
        private readonly LiteratureChainRepository literatureChainRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The rnd generator.
        /// </summary>
        private readonly Random rndGenerator = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainMixerController"/> class.
        /// </summary>
        public ChainMixerController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            notationRepository = new NotationRepository(db);
            chainRepository = new ChainRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
            literatureChainRepository = new LiteratureChainRepository(db);
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
            ViewBag.matters = db.matter.ToList();
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="mixes">
        /// The mixes.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(long matterId, int notationId, int languageId, int mixes)
        {
            matter matter = db.matter.Single(m => m.id == matterId);
            chain dataBaseChain;
            if (matter.nature_id == Aliases.NatureLiterature)
            {
                long chainId =
                    db.literature_chain.Single(l => l.matter_id == matterId && 
                                               l.notation_id == notationId && 
                                               l.language_id == languageId).id;
                dataBaseChain = db.chain.Single(c => c.id == chainId);
            }
            else
            {
                dataBaseChain = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId);
            }

            BaseChain libiadaChain = chainRepository.ToLibiadaBaseChain(dataBaseChain.id);
            for (int i = 0; i < mixes; i++)
            {
                int firstIndex = rndGenerator.Next(libiadaChain.GetLength());
                int secondIndex = rndGenerator.Next(libiadaChain.GetLength());

                IBaseObject firstElement = libiadaChain[firstIndex];
                IBaseObject secondElement = libiadaChain[secondIndex];
                libiadaChain[firstIndex] = secondElement;
                libiadaChain[secondIndex] = firstElement;
            }

            var resultMatter = new matter
                {
                    nature_id = matter.nature_id,
                    name = matter.name + " " + mixes + " перемешиваний"
                };
            db.matter.Add(resultMatter);

            var resultChain = new chain
                {
                    dissimilar = false,
                    notation_id = notationId,
                    piece_type_id = dataBaseChain.piece_type_id,
                    piece_position = dataBaseChain.piece_position,
                    matter_id = resultMatter.id
                };

            long[] alphabet = this.elementRepository.ToDbElements(
                        libiadaChain.Alphabet,
                        dataBaseChain.notation_id,
                        false);

            switch (matter.nature_id)
            {
                case Aliases.NatureGenetic:
                    dna_chain dnaChain = db.dna_chain.Single(c => c.id == dataBaseChain.id);

                    this.dnaChainRepository.Insert(
                        resultChain,
                        dnaChain.fasta_header,
                        null,
                        dnaChain.product_id,
                        dnaChain.complement,
                        dnaChain.partial,
                        alphabet,
                        libiadaChain.Building);
                    break;
                case Aliases.NatureMusic:
                    break;
                case Aliases.NatureLiterature:

                    literature_chain literatureChain = db.literature_chain.Single(c => c.id == dataBaseChain.id);

                    literatureChainRepository.Insert(
                        resultChain,
                        literatureChain.original,
                        literatureChain.language_id,
                        literatureChain.translator_id,
                        alphabet,
                        libiadaChain.Building);
                    break;
            }

            return RedirectToAction("Index", "Matter");
        }
    }
}