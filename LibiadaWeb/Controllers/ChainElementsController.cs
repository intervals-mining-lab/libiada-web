using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

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

       /* // GET api/webapi/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/webapi
        public void Post([FromBody]string value)
        {
        }

        // PUT api/webapi/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/webapi/5
        public void Delete(int id)
        {
        }*/
    }
}
