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
    public class ElementController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Element/

        public ViewResult Index()
        {
            var element = db.element.Include("notation");
            return View(element.ToList());
        }

        //
        // GET: /Element/Details/5

        public ViewResult Details(long id)
        {
            element element = db.element.Single(e => e.id == id);
            return View(element);
        }

        //
        // GET: /Element/Create

        public ActionResult Create()
        {
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            return View();
        } 

        //
        // POST: /Element/Create

        [HttpPost]
        public ActionResult Create(element element)
        {
            if (ModelState.IsValid)
            {
                db.element.AddObject(element);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.notation_id = new SelectList(db.notation, "id", "name", element.notation_id);
            return View(element);
        }
        
        //
        // GET: /Element/Edit/5
 
        public ActionResult Edit(long id)
        {
            element element = db.element.Single(e => e.id == id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", element.notation_id);
            return View(element);
        }

        //
        // POST: /Element/Edit/5

        [HttpPost]
        public ActionResult Edit(element element)
        {
            if (ModelState.IsValid)
            {
                db.element.Attach(element);
                db.ObjectStateManager.ChangeObjectState(element, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", element.notation_id);
            return View(element);
        }

        //
        // GET: /Element/Delete/5
 
        public ActionResult Delete(long id)
        {
            element element = db.element.Single(e => e.id == id);
            return View(element);
        }

        //
        // POST: /Element/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            element element = db.element.Single(e => e.id == id);
            db.element.DeleteObject(element);
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