using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{ 
    public class LinkController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Link/

        public ActionResult Index()
        {
            return View(db.link.ToList());
        }

        //
        // GET: /Link/Details/5

        public ActionResult Details(int id)
        {
            link link = db.link.Single(l => l.id == id);
            if (link == null)
            {
                return HttpNotFound();
            }
            return View(link);
        }

        //
        // GET: /Link/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Link/Create

        [HttpPost]
        public ActionResult Create(link link)
        {
            if (ModelState.IsValid)
            {
                db.link.AddObject(link);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(link);
        }
        
        //
        // GET: /Link/Edit/5
 
        public ActionResult Edit(int id)
        {
            link link = db.link.Single(l => l.id == id);
            if (link == null)
            {
                return HttpNotFound();
            }
            return View(link);
        }

        //
        // POST: /Link/Edit/5

        [HttpPost]
        public ActionResult Edit(link link)
        {
            if (ModelState.IsValid)
            {
                db.link.Attach(link);
                db.ObjectStateManager.ChangeObjectState(link, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(link);
        }

        //
        // GET: /Link/Delete/5
 
        public ActionResult Delete(int id)
        {
            link link = db.link.Single(l => l.id == id);
            if (link == null)
            {
                return HttpNotFound();
            }
            return View(link);
        }

        //
        // POST: /Link/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            link link = db.link.Single(l => l.id == id);
            db.link.DeleteObject(link);
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