namespace LibiadaWeb.Controllers.Chains
{
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Mvc;

    using Models.Repositories.Chains;

    /// <summary>
    /// The chain elements controller.
    /// </summary>
    public class ChainElementsController : ApiController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The element repository.
        /// </summary>
        private readonly ElementRepository elementRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly CommonSequenceRepository sequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainElementsController"/> class.
        /// </summary>
        public ChainElementsController()
        {
            elementRepository = new ElementRepository(db);
            sequenceRepository = new CommonSequenceRepository(db);
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
            List<Element> sequenceElements = sequenceRepository.GetElements(id);
            return elementRepository.GetSelectListItems(sequenceElements, null);
        }
    }
}
