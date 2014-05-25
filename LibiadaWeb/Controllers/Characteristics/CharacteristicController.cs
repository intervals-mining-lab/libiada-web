﻿namespace LibiadaWeb.Controllers.Characteristics
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic controller.
    /// </summary>
    public class CharacteristicController : Controller
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
            var characteristic = this.db.characteristic.Include(c => c.link).Include(c => c.chain).Include(c => c.characteristic_type);
            return View(characteristic.ToList());
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

            characteristic characteristic = this.db.characteristic.Find(id);
            if (characteristic == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name");
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id");
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name");
            return this.View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,chain_id,characteristic_type_id,value,value_string,link_id,created,modified")] characteristic characteristic)
        {
            if (this.ModelState.IsValid)
            {
                this.db.characteristic.Add(characteristic);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", characteristic.link_id);
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id", characteristic.chain_id);
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
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

            characteristic characteristic = this.db.characteristic.Find(id);
            if (characteristic == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", characteristic.link_id);
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id", characteristic.chain_id);
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="characteristic">
        /// The characteristic.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,chain_id,characteristic_type_id,value,value_string,link_id,created,modified")] characteristic characteristic)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(characteristic).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", characteristic.link_id);
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id", characteristic.chain_id);
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
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

            characteristic characteristic = this.db.characteristic.Find(id);
            if (characteristic == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic);
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
            characteristic characteristic = this.db.characteristic.Find(id);
            this.db.characteristic.Remove(characteristic);
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
