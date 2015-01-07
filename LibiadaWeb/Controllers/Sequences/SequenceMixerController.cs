namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using LibiadaCore.Core;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The sequence mixer controller.
    /// </summary>
    public class SequenceMixerController : Controller
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
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository sequenceRepository;

        /// <summary>
        /// The dna sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// The literature sequence repository.
        /// </summary>
        private readonly LiteratureSequenceRepository literatureSequenceRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The rnd generator.
        /// </summary>
        private readonly Random rndGenerator = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceMixerController"/> class.
        /// </summary>
        public SequenceMixerController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            notationRepository = new NotationRepository(db);
            sequenceRepository = new CommonSequenceRepository(db);
            dnaSequenceRepository = new DnaSequenceRepository(db);
            literatureSequenceRepository = new LiteratureSequenceRepository(db);
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
                long sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId 
                                                                    && l.NotationId == notationId 
                                                                    && l.LanguageId == languageId).Id;
                dataBaseSequence = db.CommonSequence.Single(c => c.Id == sequenceId);
            }
            else
            {
                dataBaseSequence = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId);
            }

            BaseChain chain = sequenceRepository.ToLibiadaBaseChain(dataBaseSequence.Id);
            for (int i = 0; i < mixes; i++)
            {
                int firstIndex = rndGenerator.Next(chain.GetLength());
                int secondIndex = rndGenerator.Next(chain.GetLength());

                IBaseObject firstElement = chain[firstIndex];
                IBaseObject secondElement = chain[secondIndex];
                chain[firstIndex] = secondElement;
                chain[secondIndex] = firstElement;
            }

            var resultMatter = new Matter
                {
                    NatureId = matter.NatureId,
                    Name = matter.Name + " " + mixes + " перемешиваний"
                };
            db.Matter.Add(resultMatter);

            var resultsequence = new CommonSequence
                {
                    NotationId = notationId,
                    PieceTypeId = dataBaseSequence.PieceTypeId,
                    PiecePosition = dataBaseSequence.PiecePosition,
                    MatterId = resultMatter.Id
                };

            long[] alphabet = elementRepository.ToDbElements(chain.Alphabet, dataBaseSequence.NotationId, false);

            switch (matter.NatureId)
            {
                case Aliases.Nature.Genetic:
                    DnaSequence dnaSequence = db.DnaSequence.Single(c => c.Id == dataBaseSequence.Id);

                    dnaSequenceRepository.Insert(
                        resultsequence,
                        dnaSequence.FastaHeader,
                        null,
                        dnaSequence.ProductId,
                        dnaSequence.Complement,
                        dnaSequence.Partial,
                        alphabet,
                        chain.Building);
                    break;
                case Aliases.Nature.Music:
                    break;
                case Aliases.Nature.Literature:

                    LiteratureSequence literatureSequence = db.LiteratureSequence.Single(c => c.Id == dataBaseSequence.Id);

                    literatureSequenceRepository.Insert(
                        resultsequence,
                        literatureSequence.Original,
                        literatureSequence.LanguageId,
                        literatureSequence.TranslatorId,
                        alphabet,
                        chain.Building);
                    break;
            }

            return RedirectToAction("Index", "Matter");
        }
    }
}
