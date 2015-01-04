﻿namespace LibiadaWeb.Controllers.Characteristics
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The binary characteristic controller.
    /// </summary>
    public class BinaryCharacteristicController : Controller
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
            var binary_characteristic = db.binary_characteristic.Include(b => b.element).Include(b => b.link).Include(b => b.element1).Include(b => b.chain).Include(b => b.characteristic_type);
            return View(binary_characteristic.ToList());
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

            binary_characteristic binary_characteristic = db.binary_characteristic.Find(id);
            if (binary_characteristic == null)
            {
                return HttpNotFound();
            }

            return View(binary_characteristic);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.first_element_id = new SelectList(db.element, "id", "value");
            ViewBag.link_id = new SelectList(db.link, "id", "name");
            ViewBag.second_element_id = new SelectList(db.element, "id", "value");
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id");
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="binary_characteristic">
        /// The binary_characteristic.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,chain_id,characteristic_type_id,value,value_string,link_id,created,first_element_id,second_element_id,modified")] binary_characteristic binary_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.binary_characteristic.Add(binary_characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.first_element_id = new SelectList(db.element, "id", "value", binary_characteristic.first_element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", binary_characteristic.link_id);
            ViewBag.second_element_id = new SelectList(db.element, "id", "value", binary_characteristic.second_element_id);
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id", binary_characteristic.chain_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
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

            binary_characteristic binary_characteristic = db.binary_characteristic.Find(id);
            if (binary_characteristic == null)
            {
                return HttpNotFound();
            }

            ViewBag.first_element_id = new SelectList(db.element, "id", "value", binary_characteristic.first_element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", binary_characteristic.link_id);
            ViewBag.second_element_id = new SelectList(db.element, "id", "value", binary_characteristic.second_element_id);
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id", binary_characteristic.chain_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="binary_characteristic">
        /// The binary_characteristic.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,chain_id,characteristic_type_id,value,value_string,link_id,created,first_element_id,second_element_id,modified")] binary_characteristic binary_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.Entry(binary_characteristic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.first_element_id = new SelectList(db.element, "id", "value", binary_characteristic.first_element_id);
            ViewBag.link_id = new SelectList(db.link, "id", "name", binary_characteristic.link_id);
            ViewBag.second_element_id = new SelectList(db.element, "id", "value", binary_characteristic.second_element_id);
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id", binary_characteristic.chain_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
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

            binary_characteristic binary_characteristic = db.binary_characteristic.Find(id);
            if (binary_characteristic == null)
            {
                return HttpNotFound();
            }

            return View(binary_characteristic);
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
            binary_characteristic binary_characteristic = db.binary_characteristic.Find(id);
            db.binary_characteristic.Remove(binary_characteristic);
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
