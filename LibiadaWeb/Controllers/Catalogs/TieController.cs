using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{ 
    public class TieController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /tie/

        public ActionResult Index()
        {
            return View(db.tie.ToList());
        }

        //
        // GET: /tie/Details/5

        public ActionResult Details(int id)
        {
            tie tie = db.tie.Single(s => s.id == id);
            if (tie == null)
            {
                return HttpNotFound();
            }
            return View(tie);
        }

        //
        // GET: /tie/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /tie/Create

        [HttpPost]
        public ActionResult Create(tie tie)
        {
            if (ModelState.IsValid)
            {
                db.tie.AddObject(tie);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(tie);
        }
        
        //
        // GET: /tie/Edit/5
 
        public ActionResult Edit(int id)
        {
            tie tie = db.tie.Single(s => s.id == id);
            if (tie == null)
            {
                return HttpNotFound();
            }
            return View(tie);
        }

        //
        // POST: /tie/Edit/5

        [HttpPost]
        public ActionResult Edit(tie tie)
        {
            if (ModelState.IsValid)
            {
                db.tie.Attach(tie);
                db.ObjectStateManager.ChangeObjectState(tie, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tie);
        }

        //
        // GET: /tie/Delete/5
 
        public ActionResult Delete(int id)
        {
            tie tie = db.tie.Single(s => s.id == id);
            if (tie == null)
            {
                return HttpNotFound();
            }
            return View(tie);
        }

        //
        // POST: /tie/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            tie tie = db.tie.Single(s => s.id == id);
            db.tie.DeleteObject(tie);
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