namespace Libiada.Web.Controllers.Sequences;

using Libiada.Core.Core;
using Libiada.Core.DataTransformers;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Newtonsoft.Json;

/// <summary>
/// The DNA transformation controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class GeneticSequencesTransformationController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataBuilder viewDataBuilder;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory;

    /// <summary>
    /// The element repository.
    /// </summary>
    private readonly ElementRepository elementRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneticSequencesTransformationController"/> class.
    /// </summary>
    public GeneticSequencesTransformationController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                         ITaskManager taskManager,
                                         IViewDataBuilder viewDataBuilder,
                                         ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory,
                                         IResearchObjectsCache cache) : base(TaskType.GeneticSequencesTransformation, taskManager)
    {
        this.dbFactory = dbFactory;
        this.cache = cache;
        this.viewDataBuilder = viewDataBuilder;
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
    public ActionResult Index(List<long> researchObjectIds, string transformType)
    {
        int userId = User.GetUserId();
        return CreateTask(() =>
        {

            List<ResearchObjectImportResult> importResults = new(researchObjectIds.Count);

            using var geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);

            // TODO: make transformType into enum
            Notation notation = transformType.Equals("toAmino") ? Notation.AminoAcids : Notation.Triplets;
            using var sequenceRepository = sequenceRepositoryFactory.Create();
            using var db = dbFactory.CreateDbContext();

            var researchObjects = db.ResearchObjects
                                    .Include(ro => ro.Sequences)
                                    .Where(ro => researchObjectIds.Contains(ro.Id));

            var existingSequenes = researchObjects
                                     .Where(ro => ro.Sequences.Any(cs => cs.Notation == notation))
                                     .Select(ro => new { ro.Name, ro.Id })
                                     .ToArray();

            importResults.AddRange(existingSequenes.Select(s => new ResearchObjectImportResult
            {
                ResearchObjectName = s.Name,
                Result = "Sequence already exists",
                Status = "Exists"
            }));

            foreach (var item in existingSequenes)
            {
                researchObjectIds.Remove(item.Id);
            }

            foreach (var researchObjectId in researchObjectIds)
            {
                string name = researchObjects.Single(ro => ro.Id == researchObjectId).Name;
                var importResult = new ResearchObjectImportResult() { ResearchObjectName = name };
                try
                {
                    long sequenceId = db.CombinedSequenceEntities.Single(c => c.ResearchObjectId == researchObjectId && c.Notation == Notation.Nucleotides).Id;
                    ComposedSequence sourceSequence = sequenceRepository.GetLibiadaComposedSequence(sequenceId);

                    Sequence transformedSequence = transformType.Equals("toAmino")
                                                     ? DnaTransformer.EncodeAmino(sourceSequence)
                                                     : DnaTransformer.EncodeTriplets(sourceSequence);

                    long[] alphabet = elementRepository.ToDbElements(transformedSequence.Alphabet, notation, false);

                    var transformedDBSequence = new GeneticSequence
                    {
                        ResearchObjectId = researchObjectId,
                        Notation = notation,
                        Alphabet = alphabet,
                        Order = transformedSequence.Order,
                        CreatorId = userId,
                        ModifierId = userId,
                        Partial = false
                    };

                    geneticSequenceRepository.Create(transformedDBSequence);

                    (importResult.Result, importResult.Status) = ($"Successfully transformed sequence {transformType}", "Success");
                }
                catch (Exception exception)
                {
                    importResult.Status = "Error";
                    importResult.Result = $"Error: {exception.Message}";
                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                        importResult.Result += $" {exception.Message}";
                    }

                    foreach (var dbEntityEntry in db.ChangeTracker.Entries())
                    {
                        if (dbEntityEntry.Entity != null)
                        {
                            dbEntityEntry.State = EntityState.Detached;
                        }
                    }
                }
                finally
                {
                    importResults.Add(importResult);
                }
            }

            var result = new Dictionary<string, object> { { "result", importResults } };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
