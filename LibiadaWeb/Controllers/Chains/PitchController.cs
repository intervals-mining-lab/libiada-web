using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    public class PitchController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Pitch/
        public ActionResult Index()
        {
            var pitch = db.pitch.Include(p => p.note).Include(p => p.instrument).Include(p => p.accidental).Include(p => p.note_symbol);
            return View(pitch.ToList());
        }

        // GET: /Pitch/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pitch pitch = db.pitch.Find(id);
            if (pitch == null)
            {
                return HttpNotFound();
            }
            return View(pitch);
        }

        // GET: /Pitch/Create
        public ActionResult Create()
        {
            ViewBag.note_id = new SelectList(db.note, "id", "value");
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name");
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name");
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name");
            return View();
        }

        // POST: /Pitch/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,octave,midinumber,instrument_id,note_id,accidental_id,note_symbol_id,created,modified")] pitch pitch)
        {
            if (ModelState.IsValid)
            {
                db.pitch.Add(pitch);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.note_id = new SelectList(db.note, "id", "value", pitch.note_id);
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name", pitch.instrument_id);
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name", pitch.accidental_id);
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name", pitch.note_symbol_id);
            return View(pitch);
        }

        // GET: /Pitch/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pitch pitch = db.pitch.Find(id);
            if (pitch == null)
            {
                return HttpNotFound();
            }
            ViewBag.note_id = new SelectList(db.note, "id", "value", pitch.note_id);
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name", pitch.instrument_id);
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name", pitch.accidental_id);
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name", pitch.note_symbol_id);
            return View(pitch);
        }

        // POST: /Pitch/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,octave,midinumber,instrument_id,note_id,accidental_id,note_symbol_id,created,modified")] pitch pitch)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pitch).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.note_id = new SelectList(db.note, "id", "value", pitch.note_id);
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name", pitch.instrument_id);
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name", pitch.accidental_id);
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name", pitch.note_symbol_id);
            return View(pitch);
        }

        // GET: /Pitch/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            pitch pitch = db.pitch.Find(id);
            if (pitch == null)
            {
                return HttpNotFound();
            }
            return View(pitch);
        }

        // POST: /Pitch/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            pitch pitch = db.pitch.Find(id);
            db.pitch.Remove(pitch);
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
