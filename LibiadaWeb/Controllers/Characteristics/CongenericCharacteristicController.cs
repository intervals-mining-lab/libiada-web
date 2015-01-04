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
            var congeneric_characteristic = db.congeneric_characteristic.Include(c => c.characteristic_type).Include(c => c.element).Include(c => c.link);
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

            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Find(id);
            if (congeneric_characteristic == null)
            {
                return HttpNotFound();
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
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            ViewBag.element_id = new SelectList(db.element, "id", "value");
            ViewBag.link_id = new SelectList(db.link, "id", "name");
            return View();
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
            if (ModelState.IsValid)
            {
                db.congeneric_characteristic.Add(congeneric_characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", congeneric_characteristic.element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", congeneric_characteristic.link_id);
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

            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Find(id);
            if (congeneric_characteristic == null)
            {
                return HttpNotFound();
            }

            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", congeneric_characteristic.element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", congeneric_characteristic.link_id);
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
            if (ModelState.IsValid)
            {
                db.Entry(congeneric_characteristic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", congeneric_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", congeneric_characteristic.element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", congeneric_characteristic.link_id);
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

            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Find(id);
            if (congeneric_characteristic == null)
            {
                return HttpNotFound();
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
            congeneric_characteristic congeneric_characteristic = db.congeneric_characteristic.Find(id);
            db.congeneric_characteristic.Remove(congeneric_characteristic);
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
