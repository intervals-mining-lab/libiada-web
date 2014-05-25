namespace LibiadaWeb.Controllers.Catalogs
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic type controller.
    /// </summary>
    public class CharacteristicTypeController : Controller
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
            var characteristic_type = this.db.characteristic_type.Include(c => c.characteristic_group);
            return View(characteristic_type.ToList());
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

            characteristic_type characteristic_type = this.db.characteristic_type.Find(id);
            if (characteristic_type == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_type);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.characteristic_group_id = new SelectList(this.db.characteristic_group, "id", "name");
            return this.View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="characteristic_type">
        /// The characteristic_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,description,characteristic_group_id,class_name,linkable,full_chain_applicable,congeneric_chain_applicable,binary_chain_applicable")] characteristic_type characteristic_type)
        {
            if (this.ModelState.IsValid)
            {
                this.db.characteristic_type.Add(characteristic_type);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.characteristic_group_id = new SelectList(this.db.characteristic_group, "id", "name", characteristic_type.characteristic_group_id);
            return View(characteristic_type);
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

            characteristic_type characteristic_type = this.db.characteristic_type.Find(id);
            if (characteristic_type == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.characteristic_group_id = new SelectList(this.db.characteristic_group, "id", "name", characteristic_type.characteristic_group_id);
            return View(characteristic_type);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="characteristic_type">
        /// The characteristic_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,description,characteristic_group_id,class_name,linkable,full_chain_applicable,congeneric_chain_applicable,binary_chain_applicable")] characteristic_type characteristic_type)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(characteristic_type).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.characteristic_group_id = new SelectList(this.db.characteristic_group, "id", "name", characteristic_type.characteristic_group_id);
            return View(characteristic_type);
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

            characteristic_type characteristic_type = this.db.characteristic_type.Find(id);
            if (characteristic_type == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_type);
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
            characteristic_type characteristic_type = this.db.characteristic_type.Find(id);
            this.db.characteristic_type.Remove(characteristic_type);
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
