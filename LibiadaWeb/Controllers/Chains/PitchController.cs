namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The pitch controller.
    /// </summary>
    public class PitchController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var pitch = db.pitch.Include(p => p.note).Include(p => p.instrument).Include(p => p.accidental).Include(p => p.note_symbol);
            return View(pitch.ToList());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.note_id = new SelectList(db.note, "id", "value");
            ViewBag.instrument_id = new SelectList(db.instrument, "id", "name");
            ViewBag.accidental_id = new SelectList(db.accidental, "id", "name");
            ViewBag.note_symbol_id = new SelectList(db.note_symbol, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="pitch">
        /// The pitch.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,octave,midinumber,instrument_id,note_id,accidental_id,note_symbol_id,created,modified")] pitch pitch)
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

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="pitch">
        /// The pitch.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,octave,midinumber,instrument_id,note_id,accidental_id,note_symbol_id,created,modified")] pitch pitch)
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

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            pitch pitch = db.pitch.Find(id);
            db.pitch.Remove(pitch);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
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
