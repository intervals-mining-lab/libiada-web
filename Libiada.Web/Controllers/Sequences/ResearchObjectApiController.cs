namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Models;
using Libiada.Database.Models.Repositories.Sequences;

using Libiada.Web.Models.CalculatorsData;

using System.Linq;

/// <summary>
/// Web api controller for reaserch object search and filtration.
/// </summary>
/// <seealso cref="ControllerBase" />
[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class ResearchObjectApiController(IResearchObjectsCache cache, LibiadaDatabaseEntities db) : ControllerBase
{
    private readonly IResearchObjectsCache cache = cache;
    private readonly LibiadaDatabaseEntities db = db;

    // GET: api/ResearchObjectApi/GetAllResearchObject   
    /// <summary>
    /// Gets the list of all research object 
    /// fitting search query and filters.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetAllResearchObject(Nature nature,
                                                                                  bool refSeqOnly,
                                                                                  string? searchQuery,
                                                                                  Group? group,
                                                                                  SequenceType? sequenceType)
    {
        var result = GetResearchObjects(r => true, nature, refSeqOnly, searchQuery, group, sequenceType);

        return Ok(result);
    }

    // GET: api/ResearchObjectApi/GetResearchObjectsWithSubsequences    
    /// <summary>
    /// Gets the list of only research objects with subsequences 
    /// fitting search query and filters
    /// in form of table rows.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetResearchObjectsWithSubsequences(Nature nature,
                                                                                                bool refSeqOnly,
                                                                                                string? searchQuery,
                                                                                                Group? group,
                                                                                                SequenceType? sequenceType)
    {
        var sequenceIds = db.Subsequences
                            .Select(s => s.SequenceId)
                            .Distinct();
        var researchObjectIds = db.CombinedSequenceEntities
                                  .Where(c => sequenceIds.Contains(c.Id))
                                  .Select(c => c.ResearchObjectId)
                                  .ToList();
        Func<ResearchObject, bool> filter = new(r => researchObjectIds.Contains(r.Id));
        var result = GetResearchObjects(filter, nature, refSeqOnly, searchQuery, group, sequenceType);

        return Ok(result);
    }

    // GET: api/ResearchObjectApi/GetResearchObjectsForGenesImport    
    /// <summary>
    /// Gets the list of only research objects without subsequences
    /// and suitable for genes import
    /// that are fitting search query and filters
    /// in form of table rows.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetResearchObjectsForGenesImport(Nature nature,
                                                                                              bool refSeqOnly,
                                                                                              string? searchQuery,
                                                                                              Group? group,
                                                                                              SequenceType? sequenceType)
    {
        var sequencesWithSubsequencesIds = db.Subsequences
                                             .Select(s => s.SequenceId)
                                             .Distinct();

        long[] researchObjectIds = db.CombinedSequenceEntities
                                     .Include(c => c.ResearchObject)
                                     .Where(c => !string.IsNullOrEmpty(c.RemoteId)
                                              && !sequencesWithSubsequencesIds.Contains(c.Id)
                                              && StaticCollections.SequenceTypesWithSubsequences.Contains(c.ResearchObject.SequenceType))
                                     .Select(c => c.ResearchObjectId)
                                     .ToArray();

        Func<ResearchObject, bool> filter = new(r => researchObjectIds.Contains(r.Id));
        var result = GetResearchObjects(filter, nature, refSeqOnly, searchQuery, group, sequenceType);

        return Ok(result);
    }

    // GET: api/ResearchObjectApi/GetResearchObjectsForMultisequence    
    /// <summary>
    /// Gets the list of research objects 
    /// that don't belong to any multisequences
    /// and are fitting search query and filters
    /// in form of table rows.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetResearchObjectsForMultisequence(Nature nature,
                                                                                                bool refSeqOnly,
                                                                                                string? searchQuery,
                                                                                                Group? group,
                                                                                                SequenceType? sequenceType)
    {
        var filter = MultisequenceRepository.ResearchObjectsFilter;

        var result = GetResearchObjects(filter, nature, refSeqOnly, searchQuery, group, sequenceType);

        return Ok(result);
    }

    // GET: api/ResearchObjectApi/GetPoemsResearchObjects    
    /// <summary>
    /// Gets the list of poems research objects 
    /// fitting search query and filters
    /// in form of table rows.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetPoemsResearchObjects(Nature nature,
                                                                                     bool refSeqOnly,
                                                                                     string? searchQuery,
                                                                                     Group? group,
                                                                                     SequenceType? sequenceType)
    {
        Func<ResearchObject, bool> filter = new(m => m.SequenceType == SequenceType.CompletePoem);

        var result = GetResearchObjects(filter, nature, refSeqOnly, searchQuery, group, sequenceType);

        return Ok(result);
    }

    // GET: api/ResearchObjectApi/GetResearchObjectsForTransformation    
    /// <summary>
    /// Gets the list of research objects having sequences
    /// suitable for genetic sequence transformation
    /// and are fitting search query and filters
    /// in form of table rows.
    /// </summary>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetResearchObjectsForTransformation(Nature nature,
                                                                                                 bool refSeqOnly,
                                                                                                 string? searchQuery,
                                                                                                 Group? group,
                                                                                                 SequenceType? sequenceType)
    {

        long[] researchObjectIds = db.CombinedSequenceEntities
                                     .Where(d => d.Notation == Notation.Nucleotides)
                                     .Select(d => d.ResearchObjectId)
                                     .ToArray();

        Func<ResearchObject, bool> filter = new (m => researchObjectIds.Contains(m.Id));

        var result = GetResearchObjects(filter, nature, refSeqOnly, searchQuery, group, sequenceType);

        return Ok(result);
    }

    /// <summary>
    /// Gets the list of research object fitting search query and filters.
    /// </summary>
    /// <param name="filter">
    /// The filter.
    /// </param>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    private IEnumerable<ResearchObjectTableRow> GetResearchObjects(Func<ResearchObject, bool> filter,
                                                                                 Nature nature,
                                                                                 bool refSeqOnly,
                                                                                 string? searchQuery,
                                                                                 Group? group,
                                                                                 SequenceType? sequenceType)
    {
        bool hasGroup = group.HasValue;
        bool hasSequenceType = sequenceType.HasValue;

        if (string.IsNullOrEmpty(searchQuery))
        {
            bool hasGroupAndSequenceType = hasGroup && hasSequenceType;
            return hasGroupAndSequenceType ? GetResearchObjects(filter, nature, refSeqOnly, group!.Value, sequenceType!.Value) : [];
        }

        if (searchQuery.Length < 4) return [];

        return cache.ResearchObjects
                    .Where(r => filter(r)
                             && r.Nature == nature
                             && r.Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase)
                             && (!hasGroup || r.Group == group)
                             && (!hasSequenceType || r.SequenceType == sequenceType)
                             && (r.Nature != Nature.Genetic || !refSeqOnly || IsRefSeq(r)))
                    .OrderBy(m => m.Created)
                    .Select(m => new ResearchObjectTableRow(m, false));
    }

    /// <summary>
    /// Gets the list of research object fitting filters.
    /// </summary>
    /// <param name="filter">
    /// The filter.
    /// </param>
    /// <param name="nature">
    /// The nature.
    /// </param>
    /// <param name="refSeqOnly">
    /// If set to <c>true</c> returns only reference sequences.
    /// </param>
    /// <param name="searchQuery">
    /// The search query.
    /// </param>
    /// <param name="group">
    /// The group of the research objects.
    /// </param>
    /// <param name="sequenceType">
    /// Type of the sequence.
    /// </param>
    /// <returns></returns>
    private IEnumerable<ResearchObjectTableRow> GetResearchObjects(Func<ResearchObject, bool> filter,
                                                                                 Nature nature,
                                                                                 bool refSeqOnly,
                                                                                 Group group,
                                                                                 SequenceType sequenceType)
    {
        return cache.ResearchObjects
                    .Where(r => filter(r)
                        && r.Nature == nature
                        && r.Group == group
                        && r.SequenceType == sequenceType
                        && (r.Nature != Nature.Genetic || !refSeqOnly || IsRefSeq(r)))
                    .OrderBy(m => m.Created)
                    .Select(m => new ResearchObjectTableRow(m, false));
    }

    /// <summary>
    /// Determines whether specified research object is a reference sequense.
    /// Using its remote db id.
    /// </summary>
    /// <param name="researchObject">
    /// The research object.
    /// </param>
    /// <returns>
    /// <c>true</c> if specified research object is a reference sequence; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsRefSeq(ResearchObject researchObject) => researchObject.Name.Split("|").Last().Contains('_');
}
