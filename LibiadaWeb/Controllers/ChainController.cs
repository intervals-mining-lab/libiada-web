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
    public class ChainController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Chain/

        public ViewResult Index()
        {
            var chain = db.chain.Include("building_type").Include("matter").Include("notation");
            return View(chain.ToList());
        }

        //
        // GET: /Chain/Details/5

        public ViewResult Details(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            return View(chain);
        }

        //
        // GET: /Chain/Create

        public ActionResult Create()
        {
            ViewBag.building_type_id = new SelectList(db.building_type, "id", "name");
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            return View();
        } 

        //
        // POST: /Chain/Create

        [HttpPost]
        public ActionResult Create(chain chain)
        {
            if (ModelState.IsValid)
            {
                db.chain.AddObject(chain);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.building_type_id = new SelectList(db.building_type, "id", "name", chain.building_type_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            return View(chain);
        }
        
        //
        // GET: /Chain/Edit/5
 
        public ActionResult Edit(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            ViewBag.building_type_id = new SelectList(db.building_type, "id", "name", chain.building_type_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            return View(chain);
        }

        //
        // POST: /Chain/Edit/5

        [HttpPost]
        public ActionResult Edit(chain chain)
        {
            if (ModelState.IsValid)
            {
                db.chain.Attach(chain);
                db.ObjectStateManager.ChangeObjectState(chain, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.building_type_id = new SelectList(db.building_type, "id", "name", chain.building_type_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", chain.notation_id);
            return View(chain);
        }

        //
        // GET: /Chain/Delete/5
 
        public ActionResult Delete(long id)
        {
            chain chain = db.chain.Single(c => c.id == id);
            return View(chain);
        }

        //
        // POST: /Chain/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            chain chain = db.chain.Single(c => c.id == id);
            db.chain.DeleteObject(chain);
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