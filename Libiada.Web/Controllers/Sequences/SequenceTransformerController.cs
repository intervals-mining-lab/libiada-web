namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core;
using Libiada.Core.DataTransformers;

using Libiada.Web.Helpers;

using Libiada.Database.Models.Repositories.Sequences;

using Newtonsoft.Json;

/// <summary>
/// The DNA transformation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequenceTransformerController : Controller
{
    /// <summary>
    /// Database context factory.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// The DNA sequence repository.
    /// </summary>
    private readonly GeneticSequenceRepository dnaSequenceRepository;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;

    /// <summary>
    /// The element repository.
    /// </summary>
    private readonly ElementRepository elementRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceTransformerController"/> class.
    /// </summary>
    public SequenceTransformerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                         IViewDataHelper viewDataHelper,
                                         ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory, 
                                         Cache cache)
    {
        this.dbFactory = dbFactory;
        this.db = dbFactory.CreateDbContext();
        this.viewDataHelper = viewDataHelper;
        dnaSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
        this.sequenceRepositoryFactory = sequenceRepositoryFactory;
        elementRepository = new ElementRepository(dbFactory.CreateDbContext());
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        long[] matterIds = db.CombinedSequenceEntities.Where(d => d.Notation == Notation.Nucleotides).Select(d => d.MatterId).ToArray();

        var data = viewDataHelper.FillViewData(1, int.MaxValue, m => matterIds.Contains(m.Id), "Transform");
        data.Add("nature", (byte)Nature.Genetic);
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
    public ActionResult Index(IEnumerable<long> matterIds, string transformType)
    {
        // TODO: make transformType into enum
        Notation notation = transformType.Equals("toAmino") ? Notation.AminoAcids : Notation.Triplets;
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        foreach (var matterId in matterIds)
        {
            long sequenceId = db.CombinedSequenceEntities.Single(c => c.MatterId == matterId && c.Notation == Notation.Nucleotides).Id;
            ComposedSequence sourceSequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);

            Sequence transformedSequence = transformType.Equals("toAmino")
                                             ? DnaTransformer.EncodeAmino(sourceSequence)
                                             : DnaTransformer.EncodeTriplets(sourceSequence);

            long[] alphabet = elementRepository.ToDbElements(transformedSequence.Alphabet, notation, false);

            var result = new DnaSequence
            {
                MatterId = matterId,
                Notation = notation,
                Alphabet = alphabet,
                Order = transformedSequence.Order,
                Partial = false
            };

            dnaSequenceRepository.Create(result);
        }

        return RedirectToAction("Index", "CombinedSequencesEntity");
    }
}
