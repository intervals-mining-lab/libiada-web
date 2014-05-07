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
    public class RemoteDbController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /RemoteDb/
        public ActionResult Index()
        {
            var remote_db = db.remote_db.Include(r => r.nature);
            return View(remote_db.ToList());
        }

        // GET: /RemoteDb/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            remote_db remote_db = db.remote_db.Find(id);
            if (remote_db == null)
            {
                return HttpNotFound();
            }
            return View(remote_db);
        }

        // GET: /RemoteDb/Create
        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            return View();
        }

        // POST: /RemoteDb/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,url,nature_id")] remote_db remote_db)
        {
            if (ModelState.IsValid)
            {
                db.remote_db.Add(remote_db);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        // GET: /RemoteDb/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            remote_db remote_db = db.remote_db.Find(id);
            if (remote_db == null)
            {
                return HttpNotFound();
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        // POST: /RemoteDb/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,url,nature_id")] remote_db remote_db)
        {
            if (ModelState.IsValid)
            {
                db.Entry(remote_db).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        // GET: /RemoteDb/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            remote_db remote_db = db.remote_db.Find(id);
            if (remote_db == null)
            {
                return HttpNotFound();
            }
            return View(remote_db);
        }

        // POST: /RemoteDb/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            remote_db remote_db = db.remote_db.Find(id);
            db.remote_db.Remove(remote_db);
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
