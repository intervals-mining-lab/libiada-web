namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Models.Repositories.Sequences;

/// <summary>
/// The sequence elements controller.
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SequenceElementsController : Controller
{
    /// <summary>
    /// The sequence repository.
    /// </summary>
    private readonly ICommonSequenceRepository commonSequenceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceElementsController"/> class.
    /// </summary>
    public SequenceElementsController(ICommonSequenceRepository commonSequenceRepository)
    {
        this.commonSequenceRepository = commonSequenceRepository;
    }

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
        Element[] sequenceElements = commonSequenceRepository.GetElements(id);
        return sequenceElements.Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = e.Name,
            Selected = false
        });
    }
}
