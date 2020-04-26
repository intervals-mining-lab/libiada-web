using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb.Helpers;

namespace LibiadaWeb.Controllers.Sequences
{
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