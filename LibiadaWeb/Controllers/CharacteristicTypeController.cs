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
    public class CharacteristicTypeController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /CharacteristicType/

        public ViewResult Index()
        {
            var characteristic_type = db.characteristic_type.Include("characteristic_group1");
            return View(characteristic_type.ToList());
        }

        //
        // GET: /CharacteristicType/Details/5

        public ViewResult Details(int id)
        {
            characteristic_type characteristic_type = db.characteristic_type.Single(c => c.id == id);
            return View(characteristic_type);
        }

        //
        // GET: /CharacteristicType/Create

        public ActionResult Create()
        {
            ViewBag.characteristic_group = new SelectList(db.characteristic_group, "id", "name");
            return View();
        } 

        //
        // POST: /CharacteristicType/Create

        [HttpPost]
        public ActionResult Create(characteristic_type characteristic_type)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_type.AddObject(characteristic_type);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.characteristic_group = new SelectList(db.characteristic_group, "id", "name", characteristic_type.characteristic_group);
            return View(characteristic_type);
        }
        
        //
        // GET: /CharacteristicType/Edit/5
 
        public ActionResult Edit(int id)
        {
            characteristic_type characteristic_type = db.characteristic_type.Single(c => c.id == id);
            ViewBag.characteristic_group = new SelectList(db.characteristic_group, "id", "name", characteristic_type.characteristic_group);
            return View(characteristic_type);
        }

        //
        // POST: /CharacteristicType/Edit/5

        [HttpPost]
        public ActionResult Edit(characteristic_type characteristic_type)
        {
            if (ModelState.IsValid)
            {
                db.characteristic_type.Attach(characteristic_type);
                db.ObjectStateManager.ChangeObjectState(characteristic_type, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.characteristic_group = new SelectList(db.characteristic_group, "id", "name", characteristic_type.characteristic_group);
            return View(characteristic_type);
        }

        //
        // GET: /CharacteristicType/Delete/5
 
        public ActionResult Delete(int id)
        {
            characteristic_type characteristic_type = db.characteristic_type.Single(c => c.id == id);
            return View(characteristic_type);
        }

        //
        // POST: /CharacteristicType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            characteristic_type characteristic_type = db.characteristic_type.Single(c => c.id == id);
            db.characteristic_type.DeleteObject(characteristic_type);
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