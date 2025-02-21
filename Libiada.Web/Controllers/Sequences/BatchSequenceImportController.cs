namespace Libiada.Web.Controllers.Sequences;

using Bio.Extensions;
using Bio.IO.GenBank;

using Libiada.Core.Extensions;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Helpers;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Extensions;


/// <summary>
/// The batch sequence import controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class BatchSequenceImportController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly INcbiHelper ncbiHelper;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchSequenceImportController"/> class.
    /// </summary>
    public BatchSequenceImportController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                         ITaskManager taskManager,
                                         INcbiHelper ncbiHelper,
                                         IResearchObjectsCache cache)
        : base(TaskType.BatchSequenceImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.ncbiHelper = ncbiHelper;
        this.cache = cache;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Imports sequences from GenBank by their ids.
    /// Optionally imports all subsequences.
    /// </summary>
    /// <param name="accessions">
    /// Ids of imported sequences in ncbi (remote ids).
    /// </param>
    /// <param name="importGenes">
    /// Flag indicating if genes import is needed.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(string[] accessions, bool importGenes)
    {
        return CreateTask(() =>
        {
            accessions = accessions.Distinct().Select(a => a.Split('.')[0]).ToArray();
            List<ResearchObjectImportResult> importResults = new(accessions.Length);
            string[] accessionsToImport;

            var geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
            string[] existingAccessions;
            (existingAccessions, accessionsToImport) = geneticSequenceRepository.SplitAccessionsIntoExistingAndNotImported(accessions);

            importResults.AddRange(existingAccessions.ConvertAll(existingAccession => new ResearchObjectImportResult
            {
                ResearchObjectName = existingAccession,
                Result = "Sequence already exists",
                Status = "Exists"
            }));


            importResults.AddRange(accessionsToImport.AsParallel().Select(accession =>
            {
                using var db = dbFactory.CreateDbContext();
                var importResult = new ResearchObjectImportResult() { ResearchObjectName = accession };

                try
                {
                    Bio.ISequence bioSequence = ncbiHelper.DownloadGenBankSequence(accession);
                    GenBankMetadata metadata = NcbiHelper.GetMetadata(bioSequence);
                    importResult.ResearchObjectName = metadata.Version.CompoundAccession;

                    // TODO: refactor this to use DI (and probably factories) 
                    var researchObjectRepository = new ResearchObjectRepository(db, cache);
                    var geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
                    ResearchObject researchObject = researchObjectRepository.CreateResearchObjectFromGenBankMetadata(metadata);

                    importResult.SequenceType = researchObject.SequenceType.GetDisplayValue();
                    importResult.Group = researchObject.Group.GetDisplayValue();
                    importResult.ResearchObjectName = researchObject.Name;
                    importResult.AllNames = $"Common name = {metadata.Source.CommonName}, "
                                              + $"Species = {metadata.Source.Organism.Species}, "
                                              + $"Definition = {metadata.Definition}, "
                                              + $"Saved research object name = {importResult.ResearchObjectName}";

                    bool partial = metadata.Definition.ToLower().Contains("partial");

                    var sequence = new GeneticSequence
                    {
                        ResearchObject = researchObject,
                        Notation = Notation.Nucleotides,
                        RemoteDb = RemoteDb.GenBank,
                        RemoteId = metadata.Version.CompoundAccession,
                        Partial = partial,
                        CreatorId = User.GetUserId(),
                        ModifierId = User.GetUserId(),
                    };


                    geneticSequenceRepository.Create(sequence, bioSequence);

                    (importResult.Result, importResult.Status) = importGenes ?
                                                         ImportFeatures(metadata, sequence) :
                                                         ("Successfully imported sequence", "Success");

                    return importResult;
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

                return importResult;
            }));

            string[] names = importResults.Select(r => r.ResearchObjectName).ToArray();

            // removing research objects for which adding of sequence failed
            using var db = dbFactory.CreateDbContext();
            ResearchObject[] orphanResearchObjects = db.ResearchObjects
                                       .Include(m => m.Sequences)
                                       .Where(m => names.Contains(m.Name) && m.Sequences.Count == 0)
                                       .ToArray();

            if (orphanResearchObjects.Length > 0)
            {
                db.ResearchObjects.RemoveRange(orphanResearchObjects);
                db.SaveChanges();
            }


            var result = new Dictionary<string, object> { { "result", importResults } };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }

    /// <summary>
    /// Imports sequence features.
    /// </summary>
    /// <param name="metadata">
    /// The metadata.
    /// </param>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <returns>
    /// Returns tuple where first element is import result text
    /// and second element is import status as  string.
    /// </returns>
    [NonAction]
    private (string result, string status) ImportFeatures(GenBankMetadata metadata, GeneticSequence sequence)
    {
        try
        {
            using var db = dbFactory.CreateDbContext();
            var subsequenceImporter = new SubsequenceImporter(db, metadata.Features.All, sequence.Id);
            (int featuresCount, int nonCodingCount) = subsequenceImporter.CreateSubsequences();

            string result = $"Successfully imported sequence, {featuresCount} features "
                          + $"and {nonCodingCount} non-coding subsequences";
            return (result, "Success");

        }
        catch (Exception exception)
        {
            string result = $"Successfully imported sequence but failed to import genes: {exception.Message}";
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                result += $" {exception.Message}";
            }

            return (result, "Error");
        }
    }
}
