using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class PitchController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Pitch/

        public ActionResult Index()
        {
            var pitch = db.pitch.Include("note").Include("accidental").Include("note_symbol");
            return View(pitch.ToList());
        }

        //
        // GET: /Pitch/Details/5

        public ActionResult Details(int id)
        {
            pitch pitch = db.pitch.Single(p => p.id == id);
            if (pitch == null)
            {
                return HttpNotFound();
            }
            return View(pitch);
        }

        //
        // GET: /Pitch/Create

        public ActionResult Create()
        {
            ViewBag.note_id = new SelectList(db.note, "id", "value");
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name");
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name");
            return View();
        } 

        //
        // POST: /Pitch/Create

        [HttpPost]
        public ActionResult Create(pitch pitch)
        {
            if (ModelState.IsValid)
            {
                db.pitch.AddObject(pitch);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.note_id = new SelectList(db.note, "id", "value", pitch.note_id);
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name", pitch.accidental_id);
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name", pitch.note_symbol_id);
            return View(pitch);
        }
        
        //
        // GET: /Pitch/Edit/5
 
        public ActionResult Edit(int id)
        {
            pitch pitch = db.pitch.Single(p => p.id == id);
            if (pitch == null)
            {
                return HttpNotFound();
            }
            ViewBag.note_id = new SelectList(db.note, "id", "value", pitch.note_id);
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name", pitch.accidental_id);
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name", pitch.note_symbol_id);
            return View(pitch);
        }

        //
        // POST: /Pitch/Edit/5

        [HttpPost]
        public ActionResult Edit(pitch pitch)
        {
            if (ModelState.IsValid)
            {
                db.pitch.Attach(pitch);
                db.ObjectStateManager.ChangeObjectState(pitch, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.note_id = new SelectList(db.note, "id", "value", pitch.note_id);
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name", pitch.accidental_id);
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name", pitch.note_symbol_id);
            return View(pitch);
        }

        //
        // GET: /Pitch/Delete/5
 
        public ActionResult Delete(int id)
        {
            pitch pitch = db.pitch.Single(p => p.id == id);
            if (pitch == null)
            {
                return HttpNotFound();
            }
            return View(pitch);
        }

        //
        // POST: /Pitch/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            pitch pitch = db.pitch.Single(p => p.id == id);
            db.pitch.DeleteObject(pitch);
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