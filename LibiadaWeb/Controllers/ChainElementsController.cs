using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers
{
    public class ChainElementsController : ApiController
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly ElementRepository elementRepository;
        private readonly ChainRepository chainRepository;

        public ChainElementsController()
        {
            elementRepository = new ElementRepository(db);
            chainRepository = new ChainRepository(db);
        }

        // GET api/webapi/5
        public IEnumerable<SelectListItem> Get(int id)
        {
            List<element> chainElements = chainRepository.GetElements(id);
            return elementRepository.GetSelectListItems(chainElements, null);
        }
    }
}
