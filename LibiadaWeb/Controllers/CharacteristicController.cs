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
    public class CharacteristicController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Characteristic/
        public ActionResult Index()
        {
            var characteristic = db.characteristic.Include(c => c.link).Include(c => c.chain).Include(c => c.characteristic_type);
            return View(characteristic.ToList());
        }

        // GET: /Characteristic/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic characteristic = db.characteristic.Find(id);
            if (characteristic == null)
            {
                return HttpNotFound();
            }
            return View(characteristic);
        }

        // GET: /Characteristic/Create
        public ActionResult Create()
        {
            ViewBag.link_id = new SelectList(db.link, "id", "name");
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id");
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name");
            return View();
        }

        // POST: /Characteristic/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,chain_id,characteristic_type_id,value,value_string,link_id,created,modified")] characteristic characteristic)
        {
            if (ModelState.IsValid)
            {
                db.characteristic.Add(characteristic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.link_id = new SelectList(db.link, "id", "name", characteristic.link_id);
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id", characteristic.chain_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }

        // GET: /Characteristic/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic characteristic = db.characteristic.Find(id);
            if (characteristic == null)
            {
                return HttpNotFound();
            }
            ViewBag.link_id = new SelectList(db.link, "id", "name", characteristic.link_id);
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id", characteristic.chain_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }

        // POST: /Characteristic/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,chain_id,characteristic_type_id,value,value_string,link_id,created,modified")] characteristic characteristic)
        {
            if (ModelState.IsValid)
            {
                db.Entry(characteristic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.link_id = new SelectList(db.link, "id", "name", characteristic.link_id);
            ViewBag.chain_id = new SelectList(db.chain, "id", "remote_id", characteristic.chain_id);
            ViewBag.characteristic_type_id = new SelectList(db.characteristic_type, "id", "name", characteristic.characteristic_type_id);
            return View(characteristic);
        }

        // GET: /Characteristic/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            characteristic characteristic = db.characteristic.Find(id);
            if (characteristic == null)
            {
                return HttpNotFound();
            }
            return View(characteristic);
        }

        // POST: /Characteristic/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            characteristic characteristic = db.characteristic.Find(id);
            db.characteristic.Remove(characteristic);
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
