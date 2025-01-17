namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Models.Repositories.Sequences;

/// <summary>
/// The sequence elements controller.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SequenceElementsController"/> class.
/// </remarks>
/// <param name="sequenceRepositoryFactory">
/// The sequence repository.
/// </param>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SequenceElementsController(ICombinedSequenceEntityRepositoryFactory sequenceRepositoryFactory) : Controller
{

    /// <summary>
    /// The get.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{SelectListItem}"/>.
    /// </returns>
    public IEnumerable<SelectListItem> Get(int id)
    {
        using var sequenceRepository = sequenceRepositoryFactory.Create();
        Element[] sequenceElements = sequenceRepository.GetElements(id);
        return sequenceElements.Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = e.Name,
            Selected = false
        });
    }
}
