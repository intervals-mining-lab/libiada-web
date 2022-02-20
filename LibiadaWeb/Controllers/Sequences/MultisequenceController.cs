namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    public class MultisequenceController : Controller
    {
        // GET: Multisequence
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                List<Multisequence> multisequences = db.Multisequence.ToList();
                return View(multisequences);
            }
        }
    }
}