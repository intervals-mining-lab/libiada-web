using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers
{ 
    public class NoteController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Note/

        public ActionResult Index()
        {
            var note = db.note.Include("notation").Include("tie");
            return View(note.ToList());
        }

        //
        // GET: /Note/Details/5

        public ActionResult Details(long id)
        {
            note note = db.note.Single(n => n.id == id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        //
        // GET: /Note/Create

        public ActionResult Create()
        {
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.tie_id = new SelectList(db.tie, "id", "name");
            return View();
        } 

        //
        // POST: /Note/Create

        [HttpPost]
        public ActionResult Create(note note)
        {
            if (ModelState.IsValid)
            {
                db.note.AddObject(note);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.notation_id = new SelectList(db.notation, "id", "name", note.notation_id);
            ViewBag.tie_id = new SelectList(db.tie, "id", "name", note.tie_id);
            return View(note);
        }
        
        //
        // GET: /Note/Edit/5
 
        public ActionResult Edit(long id)
        {
            note note = db.note.Single(n => n.id == id);
            if (note == null)
            {
                return HttpNotFound();
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", note.notation_id);
            ViewBag.tie_id = new SelectList(db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        //
        // POST: /Note/Edit/5

        [HttpPost]
        public ActionResult Edit(note note)
        {
            if (ModelState.IsValid)
            {
                db.note.Attach(note);
                db.ObjectStateManager.ChangeObjectState(note, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", note.notation_id);
            ViewBag.tie_id = new SelectList(db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        //
        // GET: /Note/Delete/5
 
        public ActionResult Delete(long id)
        {
            note note = db.note.Single(n => n.id == id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        //
        // POST: /Note/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            note note = db.note.Single(n => n.id == id);
            db.note.DeleteObject(note);
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