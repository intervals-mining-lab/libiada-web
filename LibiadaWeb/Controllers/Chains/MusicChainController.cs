namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The music chain controller.
    /// </summary>
    public class MusicChainController : Controller
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
            var music_chain = db.music_chain.Include(m => m.matter).Include(m => m.notation).Include(m => m.piece_type).Include(m => m.remote_db);
            return View(music_chain.ToList());
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

            music_chain music_chain = db.music_chain.Find(id);
            if (music_chain == null)
            {
                return this.HttpNotFound();
            }

            return View(music_chain);
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
        /// <param name="music_chain">
        /// The music_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,notation_id,created,matter_id,piece_type_id,piece_position,remote_db_id,remote_id,modified,description")] music_chain music_chain)
        {
            if (this.ModelState.IsValid)
            {
                db.music_chain.Add(music_chain);
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", music_chain.remote_db_id);
            return View(music_chain);
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

            music_chain music_chain = db.music_chain.Find(id);
            if (music_chain == null)
            {
                return this.HttpNotFound();
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", music_chain.remote_db_id);
            return View(music_chain);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="music_chain">
        /// The music_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,notation_id,created,matter_id,piece_type_id,piece_position,remote_db_id,remote_id,modified,description")] music_chain music_chain)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(music_chain).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", music_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", music_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", music_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", music_chain.remote_db_id);
            return View(music_chain);
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

            music_chain music_chain = db.music_chain.Find(id);
            if (music_chain == null)
            {
                return this.HttpNotFound();
            }

            return View(music_chain);
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
            music_chain music_chain = db.music_chain.Find(id);
            db.music_chain.Remove(music_chain);
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
