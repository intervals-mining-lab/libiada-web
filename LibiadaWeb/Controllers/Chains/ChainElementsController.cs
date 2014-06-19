namespace LibiadaWeb.Controllers.Chains
{
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Mvc;

    using LibiadaWeb.Models.Repositories.Chains;

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
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainElementsController"/> class.
        /// </summary>
        public ChainElementsController()
        {
            elementRepository = new ElementRepository(db);
            chainRepository = new ChainRepository(db);
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<SelectListItem> Get(int id)
        {
            List<element> chainElements = chainRepository.GetElements(id);
            return elementRepository.GetSelectListItems(chainElements, null);
        }
    }
}
