using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    public class NoteController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Note/
        public ActionResult Index()
        {
            var note = db.note.Include(n => n.notation).Include(n => n.tie);
            return View(note.ToList());
        }

        // GET: /Note/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            note note = db.note.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // GET: /Note/Create
        public ActionResult Create()
        {
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.tie_id = new SelectList(db.tie, "id", "name");
            return View();
        }

        // POST: /Note/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,value,description,name,notation_id,created,numerator,denominator,ticks,onumerator,odenominator,triplet,priority,tie_id,modified")] note note)
        {
            if (ModelState.IsValid)
            {
                db.note.Add(note);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.notation_id = new SelectList(db.notation, "id", "name", note.notation_id);
            ViewBag.tie_id = new SelectList(db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        // GET: /Note/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            note note = db.note.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", note.notation_id);
            ViewBag.tie_id = new SelectList(db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        // POST: /Note/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,value,description,name,notation_id,created,numerator,denominator,ticks,onumerator,odenominator,triplet,priority,tie_id,modified")] note note)
        {
            if (ModelState.IsValid)
            {
                db.Entry(note).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", note.notation_id);
            ViewBag.tie_id = new SelectList(db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        // GET: /Note/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            note note = db.note.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // POST: /Note/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            note note = db.note.Find(id);
            db.note.Remove(note);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
