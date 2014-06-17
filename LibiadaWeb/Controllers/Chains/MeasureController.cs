namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The measure controller.
    /// </summary>
    public class MeasureController : Controller
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
            var measure = db.measure.Include(m => m.matter).Include(m => m.notation).Include(m => m.piece_type).Include(m => m.remote_db);
            return View(measure.ToList());
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

            measure measure = db.measure.Find(id);
            if (measure == null)
            {
                return this.HttpNotFound();
            }

            return View(measure);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="measure">
        /// The measure.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,beats,beatbase,ticks_per_beat,fifths,remote_db_id,remote_id,modified")] measure measure)
        {
            if (this.ModelState.IsValid)
            {
                db.measure.Add(measure);
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
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

            measure measure = db.measure.Find(id);
            if (measure == null)
            {
                return this.HttpNotFound();
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="measure">
        /// The measure.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,beats,beatbase,ticks_per_beat,fifths,remote_db_id,remote_id,modified")] measure measure)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(measure).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
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

            measure measure = db.measure.Find(id);
            if (measure == null)
            {
                return this.HttpNotFound();
            }

            return View(measure);
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
            measure measure = db.measure.Find(id);
            db.measure.Remove(measure);
            db.SaveChanges();
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
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
