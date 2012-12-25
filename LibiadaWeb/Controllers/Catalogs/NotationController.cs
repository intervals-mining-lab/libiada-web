using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{ 
    public class NotationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Notation/

        public ViewResult Index()
        {
            var notation = db.notation.Include("nature");
            return View(notation.ToList());
        }

        //
        // GET: /Notation/Details/5

        public ViewResult Details(int id)
        {
            notation notation = db.notation.Single(n => n.id == id);
            return View(notation);
        }

        //
        // GET: /Notation/Create

        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            return View();
        } 

        //
        // POST: /Notation/Create

        [HttpPost]
        public ActionResult Create(notation notation)
        {
            if (ModelState.IsValid)
            {
                notation.id = (int)db.ExecuteStoreQuery<long>("SELECT seq_next_value('catalog_table_id_seq')").First();
                db.notation.AddObject(notation);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", notation.nature_id);
            return View(notation);
        }
        
        //
        // GET: /Notation/Edit/5
 
        public ActionResult Edit(int id)
        {
            notation notation = db.notation.Single(n => n.id == id);
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", notation.nature_id);
            return View(notation);
        }

        //
        // POST: /Notation/Edit/5

        [HttpPost]
        public ActionResult Edit(notation notation)
        {
            if (ModelState.IsValid)
            {
                db.notation.Attach(notation);
                db.ObjectStateManager.ChangeObjectState(notation, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", notation.nature_id);
            return View(notation);
        }

        //
        // GET: /Notation/Delete/5
 
        public ActionResult Delete(int id)
        {
            notation notation = db.notation.Single(n => n.id == id);
            return View(notation);
        }

        //
        // POST: /Notation/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            notation notation = db.notation.Single(n => n.id == id);
            db.notation.DeleteObject(notation);
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