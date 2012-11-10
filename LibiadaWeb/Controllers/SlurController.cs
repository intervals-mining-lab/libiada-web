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
    public class SlurController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Slur/

        public ViewResult Index()
        {
            return View(db.slur.ToList());
        }

        //
        // GET: /Slur/Details/5

        public ViewResult Details(int id)
        {
            slur slur = db.slur.Single(s => s.id == id);
            return View(slur);
        }

        //
        // GET: /Slur/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Slur/Create

        [HttpPost]
        public ActionResult Create(slur slur)
        {
            if (ModelState.IsValid)
            {
                db.slur.AddObject(slur);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(slur);
        }
        
        //
        // GET: /Slur/Edit/5
 
        public ActionResult Edit(int id)
        {
            slur slur = db.slur.Single(s => s.id == id);
            return View(slur);
        }

        //
        // POST: /Slur/Edit/5

        [HttpPost]
        public ActionResult Edit(slur slur)
        {
            if (ModelState.IsValid)
            {
                db.slur.Attach(slur);
                db.ObjectStateManager.ChangeObjectState(slur, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(slur);
        }

        //
        // GET: /Slur/Delete/5
 
        public ActionResult Delete(int id)
        {
            slur slur = db.slur.Single(s => s.id == id);
            return View(slur);
        }

        //
        // POST: /Slur/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            slur slur = db.slur.Single(s => s.id == id);
            db.slur.DeleteObject(slur);
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