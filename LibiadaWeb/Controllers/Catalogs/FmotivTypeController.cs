using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{ 
    public class FmotivTypeController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /FmotivType/

        public ActionResult Index()
        {
            return View(db.fmotiv_type.ToList());
        }

        //
        // GET: /FmotivType/Details/5

        public ActionResult Details(int id)
        {
            fmotiv_type fmotiv_type = db.fmotiv_type.Single(f => f.id == id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv_type);
        }

        //
        // GET: /FmotivType/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /FmotivType/Create

        [HttpPost]
        public ActionResult Create(fmotiv_type fmotiv_type)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv_type.AddObject(fmotiv_type);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(fmotiv_type);
        }
        
        //
        // GET: /FmotivType/Edit/5
 
        public ActionResult Edit(int id)
        {
            fmotiv_type fmotiv_type = db.fmotiv_type.Single(f => f.id == id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv_type);
        }

        //
        // POST: /FmotivType/Edit/5

        [HttpPost]
        public ActionResult Edit(fmotiv_type fmotiv_type)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv_type.Attach(fmotiv_type);
                db.ObjectStateManager.ChangeObjectState(fmotiv_type, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fmotiv_type);
        }

        //
        // GET: /FmotivType/Delete/5
 
        public ActionResult Delete(int id)
        {
            fmotiv_type fmotiv_type = db.fmotiv_type.Single(f => f.id == id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv_type);
        }

        //
        // POST: /FmotivType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            fmotiv_type fmotiv_type = db.fmotiv_type.Single(f => f.id == id);
            db.fmotiv_type.DeleteObject(fmotiv_type);
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