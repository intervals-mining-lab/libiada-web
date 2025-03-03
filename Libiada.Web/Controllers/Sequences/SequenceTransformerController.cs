﻿namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core;
using Libiada.Core.DataTransformers;

using Libiada.Web.Helpers;
using Libiada.Web.Extensions;

using Libiada.Database.Models.Repositories.Sequences;

using Newtonsoft.Json;


/// <summary>
/// The DNA transformation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequenceTransformerController : Controller
{
    private readonly LibiadaDatabaseEntities db;
    private readonly IViewDataBuilder viewDataBuilder;

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
                                         IViewDataBuilder viewDataBuilder,
                                         ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                         IResearchObjectsCache cache)
    {
        this.db = dbFactory.CreateDbContext();
        this.viewDataBuilder = viewDataBuilder;
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
        
        var data = viewDataBuilder.AddMinMaxResearchObjects()
                                  .AddSequenceGroups()
                                  .SetNature(Nature.Genetic)
                                  .AddNotations()
                                  .AddSequenceTypes()
                                  .AddGroups()
                                  .Build();
        ViewBag.data = JsonConvert.SerializeObject(data);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="researchObjectIds">
    /// The sequence ids.
    /// </param>
    /// <param name="transformType">
    /// The to amino.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(IEnumerable<long> researchObjectIds, string transformType)
    {
        // TODO: make transformType into enum
        Notation notation = transformType.Equals("toAmino") ? Notation.AminoAcids : Notation.Triplets;
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        foreach (var researchObjectId in researchObjectIds)
        {
            long sequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId && c.Notation == Notation.Nucleotides).Id;
            ComposedSequence sourceSequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);

            Sequence transformedSequence = transformType.Equals("toAmino")
                                             ? DnaTransformer.EncodeAmino(sourceSequence)
                                             : DnaTransformer.EncodeTriplets(sourceSequence);

            long[] alphabet = elementRepository.ToDbElements(transformedSequence.Alphabet, notation, false);

            var result = new GeneticSequence
            {
                ResearchObjectId = researchObjectId,
                Notation = notation,
                Alphabet = alphabet,
                Order = transformedSequence.Order,
                CreatorId = User.GetUserId(),
                ModifierId = User.GetUserId(),
                Partial = false
            };

            dnaSequenceRepository.Create(result);
        }

        return RedirectToAction("Index", "CombinedSequencesEntity");
    }
}
