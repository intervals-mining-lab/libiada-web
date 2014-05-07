using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb;

namespace LibiadaWeb.Controllers
{
    public class BinaryCharacteristicController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /BinaryCharacteristic/
        public ActionResult Index()
        {
            var binary_characteristic = db.binary_characteristic.Include(b => b.element).Include(b => b.link).Include(b => b.element1).Include(b => b.chain).Include(b => b.characteristic_type);
            return View(binary_characteristic.ToList());
        }

        // GET: /BinaryCharacteristic/Details/5
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

        // GET: /BinaryCharacteristic/Create
        public ActionResult Create()
        {
            ViewBag.first_element_id = new SelectList(db.element, "id", "value");
            ViewBag.link_id = new SelectList(db.link, "id", "name");
            ViewBag.second_element_id = new SelectList(db.element, "id", "value");
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id");
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            return View();
        }

        // POST: /BinaryCharacteristic/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,chain_id,characteristic_type_id,value,value_string,link_id,created,first_element_id,second_element_id,modified")] binary_characteristic binary_characteristic)
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

        // GET: /BinaryCharacteristic/Edit/5
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

        // POST: /BinaryCharacteristic/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,chain_id,characteristic_type_id,value,value_string,link_id,created,first_element_id,second_element_id,modified")] binary_characteristic binary_characteristic)
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

        // GET: /BinaryCharacteristic/Delete/5
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

        // POST: /BinaryCharacteristic/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            binary_characteristic binary_characteristic = db.binary_characteristic.Find(id);
            db.binary_characteristic.Remove(binary_characteristic);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

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
