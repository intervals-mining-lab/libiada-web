namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The note controller.
    /// </summary>
    public class NoteController : Controller
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
            var note = db.note.Include(n => n.notation).Include(n => n.tie);
            return View(note.ToList());
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

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.tie_id = new SelectList(db.tie, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="note">
        /// The note.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,value,description,name,notation_id,created,numerator,denominator,ticks,onumerator,odenominator,triplet,priority,tie_id,modified")] note note)
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

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="note">
        /// The note.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,value,description,name,notation_id,created,numerator,denominator,ticks,onumerator,odenominator,triplet,priority,tie_id,modified")] note note)
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

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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
        public ActionResult DeleteConfirmed(long id)
        {
            note note = db.note.Find(id);
            db.note.Remove(note);
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
