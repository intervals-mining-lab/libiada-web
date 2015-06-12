namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using LibiadaCore.Core;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
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
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository sequenceRepository;

        /// <summary>
        /// The dna sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// The music sequence repository.
        /// </summary>
        private readonly MusicSequenceRepository musicSequenceRepository;

        /// <summary>
        /// The literature sequence repository.
        /// </summary>
        private readonly LiteratureSequenceRepository literatureSequenceRepository;

        /// <summary>
        /// The data sequence repository.
        /// </summary>
        private readonly DataSequenceRepository dataSequenceRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The random generator.
        /// </summary>
        private readonly Random randomGenerator = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceMixerController"/> class.
        /// </summary>
        public SequenceMixerController()
        {
            db = new LibiadaWebEntities();
            sequenceRepository = new CommonSequenceRepository(db);
            dnaSequenceRepository = new DnaSequenceRepository(db);
            musicSequenceRepository = new MusicSequenceRepository(db);
            literatureSequenceRepository = new LiteratureSequenceRepository(db);
            dataSequenceRepository = new DataSequenceRepository(db);
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
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = viewDataHelper.FillViewData(1, 1, false, "Mix");

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
        /// <param name="translatorId">
        /// The translator id.
        /// </param>
        /// <param name="scrambling">
        /// The mixes.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if sequence nature is unknown.
        /// </exception>
        [HttpPost]
        public ActionResult Index(long matterId, int notationId, int? languageId, int? translatorId, int scrambling)
        {
            Matter matter = db.Matter.Single(m => m.Id == matterId);
            CommonSequence dataBaseSequence;
            if (matter.NatureId == Aliases.Nature.Literature)
            {
                long sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId 
                                                                    && l.NotationId == notationId
                                                                    && l.LanguageId == languageId
                                                                    && ((translatorId == null && l.TranslatorId == null)
                                                                        || (translatorId == l.TranslatorId))).Id;
                dataBaseSequence = db.CommonSequence.Single(c => c.Id == sequenceId);
            }
            else
            {
                dataBaseSequence = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId);
            }

            BaseChain chain = sequenceRepository.ToLibiadaBaseChain(dataBaseSequence.Id);
            for (int i = 0; i < scrambling; i++)
            {
                int firstIndex = randomGenerator.Next(chain.GetLength());
                int secondIndex = randomGenerator.Next(chain.GetLength());

                IBaseObject firstElement = chain[firstIndex];
                IBaseObject secondElement = chain[secondIndex];
                chain[firstIndex] = secondElement;
                chain[secondIndex] = firstElement;
            }

            var resultMatter = new Matter
                {
                    NatureId = matter.NatureId,
                    Name = matter.Name + " " + scrambling + " mixes"
                };
            db.Matter.Add(resultMatter);
            db.SaveChanges();

            var resultsequence = new CommonSequence
                {
                    NotationId = notationId,
                    FeatureId = dataBaseSequence.FeatureId,
                    PiecePosition = dataBaseSequence.PiecePosition,
                    MatterId = resultMatter.Id
                };

            long[] alphabet = elementRepository.ToDbElements(chain.Alphabet, dataBaseSequence.NotationId, false);

            switch (matter.NatureId)
            {
                case Aliases.Nature.Genetic:
                    DnaSequence dnaSequence = db.DnaSequence.Single(c => c.Id == dataBaseSequence.Id);

                    dnaSequenceRepository.Create(
                        resultsequence,
                        dnaSequence.FastaHeader,
                        null,
                        dnaSequence.Complementary,
                        dnaSequence.Partial,
                        alphabet,
                        chain.Building);
                    break;
                case Aliases.Nature.Music:
                    musicSequenceRepository.Create(resultsequence, alphabet, chain.Building);
                    break;
                case Aliases.Nature.Literature:

                    LiteratureSequence literatureSequence = db.LiteratureSequence.Single(c => c.Id == dataBaseSequence.Id);

                    literatureSequenceRepository.Create(
                        resultsequence,
                        literatureSequence.Original,
                        literatureSequence.LanguageId,
                        literatureSequence.TranslatorId,
                        alphabet,
                        chain.Building);
                    break;
                case Aliases.Nature.Data:
                    dataSequenceRepository.Create(resultsequence, alphabet, chain.Building);
                    break;
                default:
                    throw new Exception("Unknown sequence nature.");
            }

            return RedirectToAction("Index", "Matters");
        }
    }
}
