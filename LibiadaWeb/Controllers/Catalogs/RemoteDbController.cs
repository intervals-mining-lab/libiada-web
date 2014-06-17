namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The remote db controller.
    /// </summary>
    public class RemoteDbController : Controller
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
            var remote_db = db.remote_db.Include(r => r.nature);
            return View(remote_db.ToList());
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

            remote_db remote_db = db.remote_db.Find(id);
            if (remote_db == null)
            {
                return this.HttpNotFound();
            }

            return View(remote_db);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="remote_db">
        /// The remote_db.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,description,url,nature_id")] remote_db remote_db)
        {
            if (this.ModelState.IsValid)
            {
                db.remote_db.Add(remote_db);
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
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

            remote_db remote_db = db.remote_db.Find(id);
            if (remote_db == null)
            {
                return this.HttpNotFound();
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="remote_db">
        /// The remote_db.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,description,url,nature_id")] remote_db remote_db)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(remote_db).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
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

            remote_db remote_db = db.remote_db.Find(id);
            if (remote_db == null)
            {
                return this.HttpNotFound();
            }

            return View(remote_db);
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
            remote_db remote_db = db.remote_db.Find(id);
            db.remote_db.Remove(remote_db);
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
