namespace LibiadaWeb.Controllers.Chains
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using Models;
    using Models.Repositories.Catalogs;
    using Models.Repositories.Chains;

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
        private readonly CommonSequenceRepository sequenceRepository;

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
            sequenceRepository = new CommonSequenceRepository(db);
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
            ViewBag.matters = db.Matter.ToList();
            ViewBag.language_id = new SelectList(db.Language, "id", "name");
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
            Matter matter = db.Matter.Single(m => m.Id == matterId);
            CommonSequence dataBaseSequence;
            if (matter.NatureId == Aliases.Nature.Literature)
            {
                long chainId =
                    db.LiteratureSequence.Single(l => l.MatterId == matterId && 
                                               l.NotationId == notationId && 
                                               l.LanguageId == languageId).Id;
                dataBaseSequence = db.CommonSequence.Single(c => c.Id == chainId);
            }
            else
            {
                dataBaseSequence = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId);
            }

            BaseChain libiadaChain = sequenceRepository.ToLibiadaBaseChain(dataBaseSequence.Id);
            for (int i = 0; i < mixes; i++)
            {
                int firstIndex = rndGenerator.Next(libiadaChain.GetLength());
                int secondIndex = rndGenerator.Next(libiadaChain.GetLength());

                IBaseObject firstElement = libiadaChain[firstIndex];
                IBaseObject secondElement = libiadaChain[secondIndex];
                libiadaChain[firstIndex] = secondElement;
                libiadaChain[secondIndex] = firstElement;
            }

            var resultMatter = new Matter
                {
                    NatureId = matter.NatureId,
                    Name = matter.Name + " " + mixes + " перемешиваний"
                };
            db.Matter.Add(resultMatter);

            var resultChain = new CommonSequence
                {
                    NotationId = notationId,
                    PieceTypeId = dataBaseSequence.PieceTypeId,
                    PiecePosition = dataBaseSequence.PiecePosition,
                    MatterId = resultMatter.Id
                };

            long[] alphabet = elementRepository.ToDbElements(
                        libiadaChain.Alphabet,
                        dataBaseSequence.NotationId,
                        false);

            switch (matter.NatureId)
            {
                case Aliases.Nature.Genetic:
                    DnaSequence dnaSequence = db.DnaSequence.Single(c => c.Id == dataBaseSequence.Id);

                    dnaChainRepository.Insert(
                        resultChain,
                        dnaSequence.FastaHeader,
                        null,
                        dnaSequence.ProductId,
                        dnaSequence.Complement,
                        dnaSequence.Partial,
                        alphabet,
                        libiadaChain.Building);
                    break;
                case Aliases.Nature.Music:
                    break;
                case Aliases.Nature.Literature:

                    LiteratureSequence literatureSequence = db.LiteratureSequence.Single(c => c.Id == dataBaseSequence.Id);

                    literatureChainRepository.Insert(
                        resultChain,
                        literatureSequence.Original,
                        literatureSequence.LanguageId,
                        literatureSequence.TranslatorId,
                        alphabet,
                        libiadaChain.Building);
                    break;
            }

            return RedirectToAction("Index", "Matter");
        }
    }
}
