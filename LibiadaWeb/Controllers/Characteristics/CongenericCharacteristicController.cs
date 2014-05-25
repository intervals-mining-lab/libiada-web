namespace LibiadaWeb.Controllers.Characteristics
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The congeneric characteristic controller.
    /// </summary>
    public class CongenericCharacteristicController : Controller
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
            var congeneric_characteristic = this.db.congeneric_characteristic.Include(c => c.characteristic_type).Include(c => c.element).Include(c => c.link);
            return View(congeneric_characteristic.ToList());
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

            congeneric_characteristic congeneric_characteristic = this.db.congeneric_characteristic.Find(id);
            if (congeneric_characteristic == null)
            {
                return this.HttpNotFound();
            }

            return View(congeneric_characteristic);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name");
            this.ViewBag.element_id = new SelectList(this.db.element, "id", "value");
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name");
            return this.View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="congeneric_characteristic">
        /// The congeneric_characteristic.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,chain_id,characteristic_type_id,value,value_string,link_id,created,element_id,modified")] congeneric_characteristic congeneric_characteristic)
        {
            if (this.ModelState.IsValid)
            {
                this.db.congeneric_characteristic.Add(congeneric_characteristic);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            this.ViewBag.element_id = new SelectList(this.db.element, "id", "value", congeneric_characteristic.element_id);
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", congeneric_characteristic.link_id);
            return View(congeneric_characteristic);
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

            congeneric_characteristic congeneric_characteristic = this.db.congeneric_characteristic.Find(id);
            if (congeneric_characteristic == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            this.ViewBag.element_id = new SelectList(this.db.element, "id", "value", congeneric_characteristic.element_id);
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", congeneric_characteristic.link_id);
            return View(congeneric_characteristic);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="congeneric_characteristic">
        /// The congeneric_characteristic.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,chain_id,characteristic_type_id,value,value_string,link_id,created,element_id,modified")] congeneric_characteristic congeneric_characteristic)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(congeneric_characteristic).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            this.ViewBag.element_id = new SelectList(this.db.element, "id", "value", congeneric_characteristic.element_id);
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", congeneric_characteristic.link_id);
            return View(congeneric_characteristic);
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

            congeneric_characteristic congeneric_characteristic = this.db.congeneric_characteristic.Find(id);
            if (congeneric_characteristic == null)
            {
                return this.HttpNotFound();
            }

            return View(congeneric_characteristic);
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
            congeneric_characteristic congeneric_characteristic = this.db.congeneric_characteristic.Find(id);
            this.db.congeneric_characteristic.Remove(congeneric_characteristic);
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
