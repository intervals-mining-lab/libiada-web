namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The literature chain controller.
    /// </summary>
    public class LiteratureChainController : Controller
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
            var literature_chain = db.literature_chain.Include(l => l.language).Include(l => l.matter).Include(l => l.notation).Include(l => l.piece_type).Include(l => l.translator).Include(l => l.remote_db);
            return View(literature_chain.ToList());
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

            literature_chain literature_chain = db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return this.HttpNotFound();
            }

            return View(literature_chain);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.translator_id = new SelectList(db.translator, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="literature_chain">
        /// The literature_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,notation_id,created,matter_id,dissimilar,piece_type_id,translator_id,piece_position,original,language_id,remote_db_id,remote_id,modified,description")] literature_chain literature_chain)
        {
            if (this.ModelState.IsValid)
            {
                db.literature_chain.Add(literature_chain);
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.language_id = new SelectList(db.language, "id", "name", literature_chain.language_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", literature_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", literature_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", literature_chain.piece_type_id);
            ViewBag.translator_id = new SelectList(db.translator, "id", "name", literature_chain.translator_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
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

            literature_chain literature_chain = db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return this.HttpNotFound();
            }

            ViewBag.language_id = new SelectList(db.language, "id", "name", literature_chain.language_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", literature_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", literature_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", literature_chain.piece_type_id);
            ViewBag.translator_id = new SelectList(db.translator, "id", "name", literature_chain.translator_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="literature_chain">
        /// The literature_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,notation_id,created,matter_id,dissimilar,piece_type_id,translator_id,piece_position,original,language_id,remote_db_id,remote_id,modified,description")] literature_chain literature_chain)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(literature_chain).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.language_id = new SelectList(db.language, "id", "name", literature_chain.language_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", literature_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", literature_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", literature_chain.piece_type_id);
            ViewBag.translator_id = new SelectList(db.translator, "id", "name", literature_chain.translator_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
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

            literature_chain literature_chain = db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return this.HttpNotFound();
            }

            return View(literature_chain);
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
            literature_chain literature_chain = db.literature_chain.Find(id);
            db.literature_chain.Remove(literature_chain);
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
