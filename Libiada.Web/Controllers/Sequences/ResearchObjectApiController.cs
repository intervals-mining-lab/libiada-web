namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Models;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Web.Models.CalculatorsData;

/// <summary>
/// Web api controller for reaserch object search and filtration.
/// </summary>
/// <seealso cref="ControllerBase" />
[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class ResearchObjectApiController(IResearchObjectsCache cache) : ControllerBase
{
    private readonly IResearchObjectsCache cache = cache;

    // GET: api/ResearchObjectApi
    [HttpGet]
    public ActionResult<IEnumerable<ResearchObjectTableRow>> GetResearchObjects(Nature nature, bool refSeqOnly, string? searchQuery, Group? group, SequenceType? sequenceType)
    {
        bool hasGroup = group.HasValue;
        bool hasSequenceType = sequenceType.HasValue;

        if (string.IsNullOrEmpty(searchQuery))
        {
            if (group.HasValue && sequenceType.HasValue) return GetResearchObjects(nature, refSeqOnly, group.Value, sequenceType.Value);
            return new List<ResearchObjectTableRow>();
        }

        if (searchQuery.Length < 4) return new List<ResearchObjectTableRow>();

        return cache.ResearchObjects
                    .Where(r => r.Nature == nature
                             && r.Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase)
                             && (!hasGroup || r.Group == group)
                             && (!hasSequenceType || r.SequenceType == sequenceType)
                             && (r.Nature != Nature.Genetic || !refSeqOnly || IsRefSeq(r)))
                    .OrderBy(m => m.Created)
                    .Select(m => new ResearchObjectTableRow(m, false))
                    .ToList();
    }

    private ActionResult<IEnumerable<ResearchObjectTableRow>> GetResearchObjects(Nature nature, bool refSeqOnly, Group group, SequenceType sequenceType)
    {
        return cache.ResearchObjects
                    .Where(r => r.Nature == nature
                             && r.Group == group
                             && r.SequenceType == sequenceType
                             && (r.Nature != Nature.Genetic || !refSeqOnly || IsRefSeq(r)))
                    .OrderBy(m => m.Created)
                    .Select(m => new ResearchObjectTableRow(m, false))
                    .ToList();
    }

    private static bool IsRefSeq(ResearchObject researchObject) => researchObject.Name.Split("|").Last().Contains('_');
}
