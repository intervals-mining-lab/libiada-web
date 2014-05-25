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
            var pitch = this.db.pitch.Include(p => p.note).Include(p => p.instrument).Include(p => p.accidental).Include(p => p.note_symbol);
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

            pitch pitch = this.db.pitch.Find(id);
            if (pitch == null)
            {
                return this.HttpNotFound();
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
            this.ViewBag.note_id = new SelectList(this.db.note, "id", "value");
            this.ViewBag.instrument_id = new SelectList(this.db.instrument, "id", "name");
            this.ViewBag.accidental_id = new SelectList(this.db.accidental, "id", "name");
            this.ViewBag.note_symbol_id = new SelectList(this.db.note_symbol, "id", "name");
            return this.View();
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
            if (this.ModelState.IsValid)
            {
                this.db.pitch.Add(pitch);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.note_id = new SelectList(this.db.note, "id", "value", pitch.note_id);
            this.ViewBag.instrument_id = new SelectList(this.db.instrument, "id", "name", pitch.instrument_id);
            this.ViewBag.accidental_id = new SelectList(this.db.accidental, "id", "name", pitch.accidental_id);
            this.ViewBag.note_symbol_id = new SelectList(this.db.note_symbol, "id", "name", pitch.note_symbol_id);
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

            pitch pitch = this.db.pitch.Find(id);
            if (pitch == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.note_id = new SelectList(this.db.note, "id", "value", pitch.note_id);
            this.ViewBag.instrument_id = new SelectList(this.db.instrument, "id", "name", pitch.instrument_id);
            this.ViewBag.accidental_id = new SelectList(this.db.accidental, "id", "name", pitch.accidental_id);
            this.ViewBag.note_symbol_id = new SelectList(this.db.note_symbol, "id", "name", pitch.note_symbol_id);
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
            if (this.ModelState.IsValid)
            {
                this.db.Entry(pitch).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.note_id = new SelectList(this.db.note, "id", "value", pitch.note_id);
            this.ViewBag.instrument_id = new SelectList(this.db.instrument, "id", "name", pitch.instrument_id);
            this.ViewBag.accidental_id = new SelectList(this.db.accidental, "id", "name", pitch.accidental_id);
            this.ViewBag.note_symbol_id = new SelectList(this.db.note_symbol, "id", "name", pitch.note_symbol_id);
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

            pitch pitch = this.db.pitch.Find(id);
            if (pitch == null)
            {
                return this.HttpNotFound();
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
            pitch pitch = this.db.pitch.Find(id);
            this.db.pitch.Remove(pitch);
            this.db.SaveChanges();
            return this.RedirectToAction("Index");
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
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
