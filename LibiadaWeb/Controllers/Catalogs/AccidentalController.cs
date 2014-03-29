using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;

    public class AccidentalController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Accidental/

        public ActionResult Index()
        {
            return View(db.accidental.ToList());
        }

        //
        // GET: /Accidental/Details/5

        public ActionResult Details(int id)
        {
            accidental accidental = db.accidental.Single(a => a.id == id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        //
        // GET: /Accidental/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Accidental/Create

        [HttpPost]
        public ActionResult Create(accidental accidental)
        {
            if (ModelState.IsValid)
            {
                db.accidental.AddObject(accidental);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(accidental);
        }
        
        //
        // GET: /Accidental/Edit/5
 
        public ActionResult Edit(int id)
        {
            accidental accidental = db.accidental.Single(a => a.id == id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        //
        // POST: /Accidental/Edit/5

        [HttpPost]
        public ActionResult Edit(accidental accidental)
        {
            if (ModelState.IsValid)
            {
                db.accidental.Attach(accidental);
                db.ObjectStateManager.ChangeObjectState(accidental, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(accidental);
        }

        //
        // GET: /Accidental/Delete/5
 
        public ActionResult Delete(int id)
        {
            accidental accidental = db.accidental.Single(a => a.id == id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        //
        // POST: /Accidental/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            accidental accidental = db.accidental.Single(a => a.id == id);
            db.accidental.DeleteObject(accidental);
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