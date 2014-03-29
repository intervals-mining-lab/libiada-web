using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;

    public class RemoteDbController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /RemoteDb/

        public ActionResult Index()
        {
            return View(db.remote_db.ToList());
        }

        //
        // GET: /RemoteDb/Details/5

        public ActionResult Details(int id)
        {
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            if (remote_db == null)
            {
                return HttpNotFound();
            }
            return View(remote_db);
        }

        //
        // GET: /RemoteDb/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /RemoteDb/Create

        [HttpPost]
        public ActionResult Create(remote_db remote_db)
        {
            if (ModelState.IsValid)
            {
                db.remote_db.AddObject(remote_db);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(remote_db);
        }
        
        //
        // GET: /RemoteDb/Edit/5
 
        public ActionResult Edit(int id)
        {
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            if (remote_db == null)
            {
                return HttpNotFound();
            }
            return View(remote_db);
        }

        //
        // POST: /RemoteDb/Edit/5

        [HttpPost]
        public ActionResult Edit(remote_db remote_db)
        {
            if (ModelState.IsValid)
            {
                db.remote_db.Attach(remote_db);
                db.ObjectStateManager.ChangeObjectState(remote_db, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(remote_db);
        }

        //
        // GET: /RemoteDb/Delete/5
 
        public ActionResult Delete(int id)
        {
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            if (remote_db == null)
            {
                return HttpNotFound();
            }
            return View(remote_db);
        }

        //
        // POST: /RemoteDb/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            db.remote_db.DeleteObject(remote_db);
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