using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class ConnectionController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Connection/

        public ViewResult Index()
        {
            var connection = db.connection.Include("chain").Include("chain1").Include("connection_type");
            return View(connection.ToList());
        }

        //
        // GET: /Connection/Details/5

        public ViewResult Details(long id)
        {
            connection connection = db.connection.Single(c => c.id == id);
            return View(connection);
        }

        //
        // GET: /Connection/Create

        public ActionResult Create()
        {
            ViewBag.child_chain_id = new SelectList(db.chain, "id", "building");
            ViewBag.parent_chain_id = new SelectList(db.chain, "id", "building");
            ViewBag.connection_type_id = new SelectList(db.connection_type, "id", "name");
            return View();
        } 

        //
        // POST: /Connection/Create

        [HttpPost]
        public ActionResult Create(connection connection)
        {
            if (ModelState.IsValid)
            {
                db.connection.AddObject(connection);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.child_chain_id = new SelectList(db.chain, "id", "building", connection.child_chain_id);
            ViewBag.parent_chain_id = new SelectList(db.chain, "id", "building", connection.parent_chain_id);
            ViewBag.connection_type_id = new SelectList(db.connection_type, "id", "name", connection.connection_type_id);
            return View(connection);
        }
        
        //
        // GET: /Connection/Edit/5
 
        public ActionResult Edit(long id)
        {
            connection connection = db.connection.Single(c => c.id == id);
            ViewBag.child_chain_id = new SelectList(db.chain, "id", "building", connection.child_chain_id);
            ViewBag.parent_chain_id = new SelectList(db.chain, "id", "building", connection.parent_chain_id);
            ViewBag.connection_type_id = new SelectList(db.connection_type, "id", "name", connection.connection_type_id);
            return View(connection);
        }

        //
        // POST: /Connection/Edit/5

        [HttpPost]
        public ActionResult Edit(connection connection)
        {
            if (ModelState.IsValid)
            {
                db.connection.Attach(connection);
                db.ObjectStateManager.ChangeObjectState(connection, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.child_chain_id = new SelectList(db.chain, "id", "building", connection.child_chain_id);
            ViewBag.parent_chain_id = new SelectList(db.chain, "id", "building", connection.parent_chain_id);
            ViewBag.connection_type_id = new SelectList(db.connection_type, "id", "name", connection.connection_type_id);
            return View(connection);
        }

        //
        // GET: /Connection/Delete/5
 
        public ActionResult Delete(long id)
        {
            connection connection = db.connection.Single(c => c.id == id);
            return View(connection);
        }

        //
        // POST: /Connection/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            connection connection = db.connection.Single(c => c.id == id);
            db.connection.DeleteObject(connection);
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