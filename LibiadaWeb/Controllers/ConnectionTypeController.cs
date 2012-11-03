using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class ConnectionTypeController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /ConnectionType/

        public ViewResult Index()
        {
            return View(db.connection_type.ToList());
        }

        //
        // GET: /ConnectionType/Details/5

        public ViewResult Details(int id)
        {
            connection_type connection_type = db.connection_type.Single(c => c.id == id);
            return View(connection_type);
        }

        //
        // GET: /ConnectionType/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /ConnectionType/Create

        [HttpPost]
        public ActionResult Create(connection_type connection_type)
        {
            if (ModelState.IsValid)
            {
                db.connection_type.AddObject(connection_type);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(connection_type);
        }
        
        //
        // GET: /ConnectionType/Edit/5
 
        public ActionResult Edit(int id)
        {
            connection_type connection_type = db.connection_type.Single(c => c.id == id);
            return View(connection_type);
        }

        //
        // POST: /ConnectionType/Edit/5

        [HttpPost]
        public ActionResult Edit(connection_type connection_type)
        {
            if (ModelState.IsValid)
            {
                db.connection_type.Attach(connection_type);
                db.ObjectStateManager.ChangeObjectState(connection_type, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(connection_type);
        }

        //
        // GET: /ConnectionType/Delete/5
 
        public ActionResult Delete(int id)
        {
            connection_type connection_type = db.connection_type.Single(c => c.id == id);
            return View(connection_type);
        }

        //
        // POST: /ConnectionType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            connection_type connection_type = db.connection_type.Single(c => c.id == id);
            db.connection_type.DeleteObject(connection_type);
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