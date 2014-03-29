using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;

    public class NatureController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Nature/

        public ActionResult Index()
        {
            return View(db.nature.ToList());
        }

        //
        // GET: /Nature/Details/5

        public ActionResult Details(int id)
        {
            nature nature = db.nature.Single(n => n.id == id);
            if (nature == null)
            {
                return HttpNotFound();
            }
            return View(nature);
        }

        //
        // GET: /Nature/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Nature/Create

        [HttpPost]
        public ActionResult Create(nature nature)
        {
            if (ModelState.IsValid)
            {
                db.nature.AddObject(nature);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(nature);
        }
        
        //
        // GET: /Nature/Edit/5
 
        public ActionResult Edit(int id)
        {
            nature nature = db.nature.Single(n => n.id == id);
            if (nature == null)
            {
                return HttpNotFound();
            }
            return View(nature);
        }

        //
        // POST: /Nature/Edit/5

        [HttpPost]
        public ActionResult Edit(nature nature)
        {
            if (ModelState.IsValid)
            {
                db.nature.Attach(nature);
                db.ObjectStateManager.ChangeObjectState(nature, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nature);
        }

        //
        // GET: /Nature/Delete/5
 
        public ActionResult Delete(int id)
        {
            nature nature = db.nature.Single(n => n.id == id);
            if (nature == null)
            {
                return HttpNotFound();
            }
            return View(nature);
        }

        //
        // POST: /Nature/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            nature nature = db.nature.Single(n => n.id == id);
            db.nature.DeleteObject(nature);
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