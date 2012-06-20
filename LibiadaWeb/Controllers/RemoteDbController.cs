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
    public class RemoteDbController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /RemoteDb/

        public ViewResult Index()
        {
            return View(db.remote_db.ToList());
        }

        //
        // GET: /RemoteDb/Details/5

        public ViewResult Details(int id)
        {
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            return View(remote_db);
        }

        //
        // GET: /RemoteDb/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /RemoteDb/Create

        [HttpPost]
        public ActionResult Create(remote_db remote_db)
        {
            if (ModelState.IsValid)
            {
                db.remote_db.AddObject(remote_db);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(remote_db);
        }
        
        //
        // GET: /RemoteDb/Edit/5
 
        public ActionResult Edit(int id)
        {
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            return View(remote_db);
        }

        //
        // POST: /RemoteDb/Edit/5

        [HttpPost]
        public ActionResult Edit(remote_db remote_db)
        {
            if (ModelState.IsValid)
            {
                db.remote_db.Attach(remote_db);
                db.ObjectStateManager.ChangeObjectState(remote_db, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(remote_db);
        }

        //
        // GET: /RemoteDb/Delete/5
 
        public ActionResult Delete(int id)
        {
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            return View(remote_db);
        }

        //
        // POST: /RemoteDb/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            remote_db remote_db = db.remote_db.Single(r => r.id == id);
            db.remote_db.DeleteObject(remote_db);
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