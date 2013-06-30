using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using LibiadaWeb.Models.Repositories;

namespace LibiadaWeb.Controllers
{
    public class ChainElementsController : ApiController
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly ElementRepository elementRepository;

        public ChainElementsController()
        {
            elementRepository = new ElementRepository(db);
        }

        // GET api/webapi/5
        public IEnumerable<SelectListItem> Get(int id)
        {
            IQueryable<alphabet> chainAlphabet = db.alphabet.Where(a => a.chain_id == id);
            IQueryable<element> chainElements = chainAlphabet.Select(a => a.element);
            return elementRepository.GetSelectListItems(chainElements, null);
        }
    }
}
