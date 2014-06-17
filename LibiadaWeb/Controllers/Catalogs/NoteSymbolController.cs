namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The note symbol controller.
    /// </summary>
    public class NoteSymbolController : Controller
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
            return View(db.note_symbol.ToList());
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

            note_symbol note_symbol = db.note_symbol.Find(id);
            if (note_symbol == null)
            {
                return this.HttpNotFound();
            }

            return View(note_symbol);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="note_symbol">
        /// The note_symbol.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,description")] note_symbol note_symbol)
        {
            if (this.ModelState.IsValid)
            {
                db.note_symbol.Add(note_symbol);
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(note_symbol);
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

            note_symbol note_symbol = db.note_symbol.Find(id);
            if (note_symbol == null)
            {
                return this.HttpNotFound();
            }

            return View(note_symbol);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="note_symbol">
        /// The note_symbol.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,description")] note_symbol note_symbol)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(note_symbol).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(note_symbol);
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

            note_symbol note_symbol = db.note_symbol.Find(id);
            if (note_symbol == null)
            {
                return this.HttpNotFound();
            }

            return View(note_symbol);
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
            note_symbol note_symbol = db.note_symbol.Find(id);
            db.note_symbol.Remove(note_symbol);
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
