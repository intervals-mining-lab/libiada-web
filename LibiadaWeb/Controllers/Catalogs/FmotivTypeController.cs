namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The fmotiv type controller.
    /// </summary>
    public class FmotivTypeController : Controller
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
            return View(db.fmotiv_type.ToList());
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

            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }

            return View(fmotiv_type);
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
        /// <param name="fmotiv_type">
        /// The fmotiv_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,description")] fmotiv_type fmotiv_type)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv_type.Add(fmotiv_type);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fmotiv_type);
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

            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }

            return View(fmotiv_type);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="fmotiv_type">
        /// The fmotiv_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,description")] fmotiv_type fmotiv_type)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fmotiv_type).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fmotiv_type);
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

            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return HttpNotFound();
            }

            return View(fmotiv_type);
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
            fmotiv_type fmotiv_type = db.fmotiv_type.Find(id);
            db.fmotiv_type.Remove(fmotiv_type);
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
