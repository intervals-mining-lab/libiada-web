using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{ 
    public class LinkUpController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /LinkUp/

        public ActionResult Index()
        {
            return View(db.link_up.ToList());
        }

        //
        // GET: /LinkUp/Details/5

        public ActionResult Details(int id)
        {
            link_up link_up = db.link_up.Single(l => l.id == id);
            if (link_up == null)
            {
                return HttpNotFound();
            }
            return View(link_up);
        }

        //
        // GET: /LinkUp/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /LinkUp/Create

        [HttpPost]
        public ActionResult Create(link_up link_up)
        {
            if (ModelState.IsValid)
            {
                db.link_up.AddObject(link_up);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(link_up);
        }
        
        //
        // GET: /LinkUp/Edit/5
 
        public ActionResult Edit(int id)
        {
            link_up link_up = db.link_up.Single(l => l.id == id);
            if (link_up == null)
            {
                return HttpNotFound();
            }
            return View(link_up);
        }

        //
        // POST: /LinkUp/Edit/5

        [HttpPost]
        public ActionResult Edit(link_up link_up)
        {
            if (ModelState.IsValid)
            {
                db.link_up.Attach(link_up);
                db.ObjectStateManager.ChangeObjectState(link_up, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(link_up);
        }

        //
        // GET: /LinkUp/Delete/5
 
        public ActionResult Delete(int id)
        {
            link_up link_up = db.link_up.Single(l => l.id == id);
            if (link_up == null)
            {
                return HttpNotFound();
            }
            return View(link_up);
        }

        //
        // POST: /LinkUp/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            link_up link_up = db.link_up.Single(l => l.id == id);
            db.link_up.DeleteObject(link_up);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}