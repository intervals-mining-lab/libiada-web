namespace Libiada.Web.Controllers.Sequences;

using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Libiada.Database.Models.Repositories.Sequences;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        List<Element> sequenceElements = commonSequenceRepository.GetElements(id);
        return sequenceElements.ConvertAll(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = e.Name,
            Selected = false
        });
    }
}
