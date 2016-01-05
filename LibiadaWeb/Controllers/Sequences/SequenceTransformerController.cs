namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.DataTransformers;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The DNA transformation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequenceTransformerController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The DNA sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceTransformerController"/> class.
        /// </summary>
        public SequenceTransformerController()
        {
            db = new LibiadaWebEntities();
            dnaSequenceRepository = new DnaSequenceRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
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
            var matterIds = db.DnaSequence.Where(d => d.NotationId == Aliases.Notation.Nucleotide).Select(d => d.MatterId).ToList();

            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillMattersData(1, int.MaxValue, true, m => matterIds.Contains(m.Id), "Transform");
            data.Add("natureId", Aliases.Nature.Genetic);
            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The sequence ids.
        /// </param>
        /// <param name="transformType">
        /// The to amino.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IEnumerable<long> matterIds, string transformType)
        {
            int notationId = transformType.Equals("toAmino") ? Aliases.Notation.AminoAcid : Aliases.Notation.Triplet;

            foreach (var matterId in matterIds)
            {
                var sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == Aliases.Notation.Nucleotide).Id;
                Chain sourceChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                BaseChain transformedChain = transformType.Equals("toAmino")
                                                 ? DnaTransformer.EncodeAmino(sourceChain)
                                                 : DnaTransformer.EncodeTriplets(sourceChain);

                var result = new DnaSequence
                    {
                        MatterId = matterId,
                        NotationId = notationId,
                        FeatureId = Aliases.Feature.FullGenome,
                        PiecePosition = 0
                    };

                long[] alphabet = elementRepository.ToDbElements(transformedChain.Alphabet, notationId, false);
                dnaSequenceRepository.Insert(result, alphabet, transformedChain.Building);
            }

            return RedirectToAction("Index", "CommonSequences");
        }
    }
}
