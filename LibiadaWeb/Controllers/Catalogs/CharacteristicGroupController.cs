namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic group controller.
    /// </summary>
    public class CharacteristicGroupController : Controller
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
            return View(db.characteristic_group.ToList());
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

            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_group);
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
        /// <param name="characteristic_group">
        /// The characteristic_group.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,description")] characteristic_group characteristic_group)
        {
            if (this.ModelState.IsValid)
            {
                db.characteristic_group.Add(characteristic_group);
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(characteristic_group);
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

            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_group);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="characteristic_group">
        /// The characteristic_group.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,description")] characteristic_group characteristic_group)
        {
            if (this.ModelState.IsValid)
            {
                db.Entry(characteristic_group).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(characteristic_group);
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

            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_group);
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
            characteristic_group characteristic_group = db.characteristic_group.Find(id);
            db.characteristic_group.Remove(characteristic_group);
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
