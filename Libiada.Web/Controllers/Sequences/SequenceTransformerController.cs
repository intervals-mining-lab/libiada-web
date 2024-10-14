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
    private readonly ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory;

    /// <summary>
    /// The element repository.
    /// </summary>
    private readonly ElementRepository elementRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceTransformerController"/> class.
    /// </summary>
    public SequenceTransformerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                         IViewDataHelper viewDataHelper,
                                         ICommonSequenceRepositoryFactory commonSequenceRepositoryFactory, 
                                         Cache cache)
    {
        this.dbFactory = dbFactory;
        this.db = dbFactory.CreateDbContext();
        this.viewDataHelper = viewDataHelper;
        dnaSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
        this.commonSequenceRepositoryFactory = commonSequenceRepositoryFactory;
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
        long[] matterIds = db.DnaSequences.Where(d => d.Notation == Notation.Nucleotides).Select(d => d.MatterId).ToArray();

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
    [ValidateAntiForgeryToken]
    public ActionResult Index(IEnumerable<long> matterIds, string transformType)
    {
        // TODO: make transformType into enum
        Notation notation = transformType.Equals("toAmino") ? Notation.AminoAcids : Notation.Triplets;
        using var commonSequenceRepository = commonSequenceRepositoryFactory.Create();
        foreach (var matterId in matterIds)
        {
            long sequenceId = db.CommonSequences.Single(c => c.MatterId == matterId && c.Notation == Notation.Nucleotides).Id;
            Chain sourceChain = commonSequenceRepository.GetLibiadaChain(sequenceId);

            BaseChain transformedChain = transformType.Equals("toAmino")
                                             ? DnaTransformer.EncodeAmino(sourceChain)
                                             : DnaTransformer.EncodeTriplets(sourceChain);

            long[] alphabet = elementRepository.ToDbElements(transformedChain.Alphabet, notation, false);

            var result = new CommonSequence
            {
                MatterId = matterId,
                Notation = notation,
                Alphabet = alphabet,
                Order = transformedChain.Order
            };

            dnaSequenceRepository.Create(result, false);
        }

        return RedirectToAction("Index", "CommonSequences");
    }
}
