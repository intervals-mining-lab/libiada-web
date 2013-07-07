using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{ 
    public class MeasureController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Measure/

        public ActionResult Index()
        {
            var measure = db.measure.Include("matter").Include("notation").Include("piece_type");
            return View(measure.ToList());
        }

        //
        // GET: /Measure/Details/5

        public ActionResult Details(long id)
        {
            measure measure = db.measure.Single(m => m.id == id);
            if (measure == null)
            {
                return HttpNotFound();
            }
            return View(measure);
        }

        //
        // GET: /Measure/Create

        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            return View();
        } 

        //
        // POST: /Measure/Create

        [HttpPost]
        public ActionResult Create(measure measure)
        {
            if (ModelState.IsValid)
            {
                db.measure.AddObject(measure);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            return View(measure);
        }
        
        //
        // GET: /Measure/Edit/5
 
        public ActionResult Edit(long id)
        {
            measure measure = db.measure.Single(m => m.id == id);
            if (measure == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            return View(measure);
        }

        //
        // POST: /Measure/Edit/5

        [HttpPost]
        public ActionResult Edit(measure measure)
        {
            if (ModelState.IsValid)
            {
                db.measure.Attach(measure);
                db.ObjectStateManager.ChangeObjectState(measure, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            return View(measure);
        }

        //
        // GET: /Measure/Delete/5
 
        public ActionResult Delete(long id)
        {
            measure measure = db.measure.Single(m => m.id == id);
            if (measure == null)
            {
                return HttpNotFound();
            }
            return View(measure);
        }

        //
        // POST: /Measure/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            measure measure = db.measure.Single(m => m.id == id);
            db.measure.DeleteObject(measure);
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