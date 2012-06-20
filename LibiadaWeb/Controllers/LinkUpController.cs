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
    public class LinkUpController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /LinkUp/

        public ViewResult Index()
        {
            return View(db.link_up.ToList());
        }

        //
        // GET: /LinkUp/Details/5

        public ViewResult Details(int id)
        {
            link_up link_up = db.link_up.Single(l => l.id == id);
            return View(link_up);
        }

        //
        // GET: /LinkUp/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /LinkUp/Create

        [HttpPost]
        public ActionResult Create(link_up link_up)
        {
            if (ModelState.IsValid)
            {
                db.link_up.AddObject(link_up);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(link_up);
        }
        
        //
        // GET: /LinkUp/Edit/5
 
        public ActionResult Edit(int id)
        {
            link_up link_up = db.link_up.Single(l => l.id == id);
            return View(link_up);
        }

        //
        // POST: /LinkUp/Edit/5

        [HttpPost]
        public ActionResult Edit(link_up link_up)
        {
            if (ModelState.IsValid)
            {
                db.link_up.Attach(link_up);
                db.ObjectStateManager.ChangeObjectState(link_up, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(link_up);
        }

        //
        // GET: /LinkUp/Delete/5
 
        public ActionResult Delete(int id)
        {
            link_up link_up = db.link_up.Single(l => l.id == id);
            return View(link_up);
        }

        //
        // POST: /LinkUp/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            link_up link_up = db.link_up.Single(l => l.id == id);
            db.link_up.DeleteObject(link_up);
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