using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb;

namespace LibiadaWeb.Controllers
{ 
    public class HomogeneousCharacteristicController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /HomogeneousCharacteristic/

        public ActionResult Index()
        {
            var homogeneous_characteristic = db.homogeneous_characteristic.Include("characteristic_type").Include("element").Include("link_up");
            return View(homogeneous_characteristic.ToList());
        }

        //
        // GET: /HomogeneousCharacteristic/Details/5

        public ActionResult Details(long id)
        {
            homogeneous_characteristic homogeneous_characteristic = db.homogeneous_characteristic.Single(h => h.id == id);
            if (homogeneous_characteristic == null)
            {
                return HttpNotFound();
            }
            return View(homogeneous_characteristic);
        }

        //
        // GET: /HomogeneousCharacteristic/Create

        public ActionResult Create()
        {
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            ViewBag.element_id = new SelectList(db.element, "id", "value");
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name");
            return View();
        } 

        //
        // POST: /HomogeneousCharacteristic/Create

        [HttpPost]
        public ActionResult Create(homogeneous_characteristic homogeneous_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.homogeneous_characteristic.AddObject(homogeneous_characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", homogeneous_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", homogeneous_characteristic.element_id);
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name", homogeneous_characteristic.link_up_id);
            return View(homogeneous_characteristic);
        }
        
        //
        // GET: /HomogeneousCharacteristic/Edit/5
 
        public ActionResult Edit(long id)
        {
            homogeneous_characteristic homogeneous_characteristic = db.homogeneous_characteristic.Single(h => h.id == id);
            if (homogeneous_characteristic == null)
            {
                return HttpNotFound();
            }
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", homogeneous_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", homogeneous_characteristic.element_id);
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name", homogeneous_characteristic.link_up_id);
            return View(homogeneous_characteristic);
        }

        //
        // POST: /HomogeneousCharacteristic/Edit/5

        [HttpPost]
        public ActionResult Edit(homogeneous_characteristic homogeneous_characteristic)
        {
            if (ModelState.IsValid)
            {
                db.homogeneous_characteristic.Attach(homogeneous_characteristic);
                db.ObjectStateManager.ChangeObjectState(homogeneous_characteristic, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", homogeneous_characteristic.characteristic_type_id);
            ViewBag.element_id = new SelectList(db.element, "id", "value", homogeneous_characteristic.element_id);
            ViewBag.link_up_id = new SelectList(db.link_up, "id", "name", homogeneous_characteristic.link_up_id);
            return View(homogeneous_characteristic);
        }

        //
        // GET: /HomogeneousCharacteristic/Delete/5
 
        public ActionResult Delete(long id)
        {
            homogeneous_characteristic homogeneous_characteristic = db.homogeneous_characteristic.Single(h => h.id == id);
            if (homogeneous_characteristic == null)
            {
                return HttpNotFound();
            }
            return View(homogeneous_characteristic);
        }

        //
        // POST: /HomogeneousCharacteristic/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            homogeneous_characteristic homogeneous_characteristic = db.homogeneous_characteristic.Single(h => h.id == id);
            db.homogeneous_characteristic.DeleteObject(homogeneous_characteristic);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}