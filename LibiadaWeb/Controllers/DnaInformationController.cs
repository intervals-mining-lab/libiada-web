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
    public class DnaInformationController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /DnaInformation/

        public ViewResult Index()
        {
            var dna_information = db.dna_information.Include("matter");
            return View(dna_information.ToList());
        }

        //
        // GET: /DnaInformation/Details/5

        public ViewResult Details(long id)
        {
            dna_information dna_information = db.dna_information.Single(d => d.matter_id == id);
            return View(dna_information);
        }

        //
        // GET: /DnaInformation/Create

        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            return View();
        } 

        //
        // POST: /DnaInformation/Create

        [HttpPost]
        public ActionResult Create(dna_information dna_information)
        {
            if (ModelState.IsValid)
            {
                db.dna_information.AddObject(dna_information);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", dna_information.matter_id);
            return View(dna_information);
        }
        
        //
        // GET: /DnaInformation/Edit/5
 
        public ActionResult Edit(long id)
        {
            dna_information dna_information = db.dna_information.Single(d => d.matter_id == id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", dna_information.matter_id);
            return View(dna_information);
        }

        //
        // POST: /DnaInformation/Edit/5

        [HttpPost]
        public ActionResult Edit(dna_information dna_information)
        {
            if (ModelState.IsValid)
            {
                db.dna_information.Attach(dna_information);
                db.ObjectStateManager.ChangeObjectState(dna_information, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", dna_information.matter_id);
            return View(dna_information);
        }

        //
        // GET: /DnaInformation/Delete/5
 
        public ActionResult Delete(long id)
        {
            dna_information dna_information = db.dna_information.Single(d => d.matter_id == id);
            return View(dna_information);
        }

        //
        // POST: /DnaInformation/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            dna_information dna_information = db.dna_information.Single(d => d.matter_id == id);
            db.dna_information.DeleteObject(dna_information);
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