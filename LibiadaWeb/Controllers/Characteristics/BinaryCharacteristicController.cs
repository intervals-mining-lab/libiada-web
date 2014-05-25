// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryCharacteristicController.cs" company="">
//   
// </copyright>
// <summary>
//   The binary characteristic controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Characteristics
{
    /// <summary>
    /// The binary characteristic controller.
    /// </summary>
    public class BinaryCharacteristicController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /BinaryCharacteristic/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var binary_characteristic = this.db.binary_characteristic.Include(b => b.element).Include(b => b.link).Include(b => b.element1).Include(b => b.chain).Include(b => b.characteristic_type);
            return View(binary_characteristic.ToList());
        }

        // GET: /BinaryCharacteristic/Details/5
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

            binary_characteristic binary_characteristic = this.db.binary_characteristic.Find(id);
            if (binary_characteristic == null)
            {
                return this.HttpNotFound();
            }

            return View(binary_characteristic);
        }

        // GET: /BinaryCharacteristic/Create
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.first_element_id = new SelectList(this.db.element, "id", "value");
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name");
            this.ViewBag.second_element_id = new SelectList(this.db.element, "id", "value");
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id");
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name");
            return this.View();
        }

        // POST: /BinaryCharacteristic/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        public ActionResult Create([Bind(Include="id,chain_id,characteristic_type_id,value,value_string,link_id,created,first_element_id,second_element_id,modified")] binary_characteristic binary_characteristic)
        {
            if (this.ModelState.IsValid)
            {
                this.db.binary_characteristic.Add(binary_characteristic);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.first_element_id = new SelectList(this.db.element, "id", "value", binary_characteristic.first_element_id);
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", binary_characteristic.link_id);
            this.ViewBag.second_element_id = new SelectList(this.db.element, "id", "value", binary_characteristic.second_element_id);
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id", binary_characteristic.chain_id);
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }

        // GET: /BinaryCharacteristic/Edit/5
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

            binary_characteristic binary_characteristic = this.db.binary_characteristic.Find(id);
            if (binary_characteristic == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.first_element_id = new SelectList(this.db.element, "id", "value", binary_characteristic.first_element_id);
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", binary_characteristic.link_id);
            this.ViewBag.second_element_id = new SelectList(this.db.element, "id", "value", binary_characteristic.second_element_id);
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id", binary_characteristic.chain_id);
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }

        // POST: /BinaryCharacteristic/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        public ActionResult Edit([Bind(Include="id,chain_id,characteristic_type_id,value,value_string,link_id,created,first_element_id,second_element_id,modified")] binary_characteristic binary_characteristic)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(binary_characteristic).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.first_element_id = new SelectList(this.db.element, "id", "value", binary_characteristic.first_element_id);
            this.ViewBag.link_id = new SelectList(this.db.link, "id", "name", binary_characteristic.link_id);
            this.ViewBag.second_element_id = new SelectList(this.db.element, "id", "value", binary_characteristic.second_element_id);
            this.ViewBag.chain_id = new SelectList(this.db.chain, "id", "remote_id", binary_characteristic.chain_id);
            this.ViewBag.characteristic_type_id = new SelectList(this.db.characteristic_type, "id", "name", binary_characteristic.characteristic_type_id);
            return View(binary_characteristic);
        }

        // GET: /BinaryCharacteristic/Delete/5
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

            binary_characteristic binary_characteristic = this.db.binary_characteristic.Find(id);
            if (binary_characteristic == null)
            {
                return this.HttpNotFound();
            }

            return View(binary_characteristic);
        }

        // POST: /BinaryCharacteristic/Delete/5
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
            binary_characteristic binary_characteristic = this.db.binary_characteristic.Find(id);
            this.db.binary_characteristic.Remove(binary_characteristic);
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
