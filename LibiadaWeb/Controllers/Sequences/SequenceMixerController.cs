namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Music;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The sequence mixer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
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
        /// The DNA sequence repository.
        /// </summary>
        private readonly GeneticSequenceRepository dnaSequenceRepository;

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
            dnaSequenceRepository = new GeneticSequenceRepository(db);
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
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(1, 1, "Mix"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <param name="language">
        /// The language id.
        /// </param>
        /// <param name="translator">
        /// The translator id.
        /// </param>
        /// <param name="scrambling">
        /// The mixes.
        /// </param
        /// <param name="pauseTreatment">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfer">
        /// Sequential transfer flag used in music sequences.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if sequence nature is unknown.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long matterId,
                                  Notation notation,
                                  Language? language,
                                  Translator? translator,
                                  PauseTreatment? pauseTreatment,
                                  bool? sequentialTransfer,
                                  int scrambling)
        {
            Matter matter = db.Matter.Single(m => m.Id == matterId);
            long sequenceId;
            switch (matter.Nature)
            {
                case Nature.Literature:
                    sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                && l.Notation == notation
                                                                && l.Language == language
                                                               && l.Translator == translator).Id;
                    break;
                case Nature.Music:
                    sequenceId = db.MusicSequence.Single(m => m.MatterId == matterId
                                                           && m.Notation == notation
                                                           && m.PauseTreatment == pauseTreatment
                                                           && m.SequentialTransfer == sequentialTransfer).Id;
                    break;
                default:
                    sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                    break;
            }

            BaseChain chain = sequenceRepository.GetLibiadaBaseChain(sequenceId);
            for (int i = 0; i < scrambling; i++)
            {
                int firstIndex = randomGenerator.Next(chain.Length);
                int secondIndex = randomGenerator.Next(chain.Length);

                IBaseObject firstElement = chain[firstIndex];
                IBaseObject secondElement = chain[secondIndex];
                chain[firstIndex] = secondElement;
                chain[secondIndex] = firstElement;
            }

            var resultMatter = new Matter
                {
                    Nature = matter.Nature,
                    Name = $"{matter.Name} {scrambling} mixes"
                };
            db.Matter.Add(resultMatter);
            db.SaveChanges();

            var result = new CommonSequence
                {
                    Notation = notation,
                    MatterId = resultMatter.Id
                };

            long[] alphabet = elementRepository.ToDbElements(chain.Alphabet, notation, false);

            switch (matter.Nature)
            {
                case Nature.Genetic:
                    DnaSequence dnaSequence = db.DnaSequence.Single(c => c.Id == sequenceId);

                    dnaSequenceRepository.Create(result, dnaSequence.Partial, alphabet, chain.Building);
                    break;
                case Nature.Music:
                    musicSequenceRepository.Create(result, alphabet, chain.Building);
                    break;
                case Nature.Literature:
                    LiteratureSequence sequence = db.LiteratureSequence.Single(c => c.Id == sequenceId);

                    literatureSequenceRepository.Create(result, sequence.Original, sequence.Language, sequence.Translator, alphabet, chain.Building);
                    break;
                case Nature.MeasurementData:
                    dataSequenceRepository.Create(result, alphabet, chain.Building);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(matter.Nature), (int)matter.Nature, typeof(Nature));
            }

            return RedirectToAction("Index", "Matters");
        }
    }
}
