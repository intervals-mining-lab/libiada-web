﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb;

namespace LibiadaWeb.Controllers
{ 
    public class InstrumentController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /Instrument/

        public ViewResult Index()
        {
            return View(db.instrument.ToList());
        }

        //
        // GET: /Instrument/Details/5

        public ViewResult Details(int id)
        {
            instrument instrument = db.instrument.Single(i => i.id == id);
            return View(instrument);
        }

        //
        // GET: /Instrument/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Instrument/Create

        [HttpPost]
        public ActionResult Create(instrument instrument)
        {
            if (ModelState.IsValid)
            {
                db.instrument.AddObject(instrument);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(instrument);
        }
        
        //
        // GET: /Instrument/Edit/5
 
        public ActionResult Edit(int id)
        {
            instrument instrument = db.instrument.Single(i => i.id == id);
            return View(instrument);
        }

        //
        // POST: /Instrument/Edit/5

        [HttpPost]
        public ActionResult Edit(instrument instrument)
        {
            if (ModelState.IsValid)
            {
                db.instrument.Attach(instrument);
                db.ObjectStateManager.ChangeObjectState(instrument, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(instrument);
        }

        //
        // GET: /Instrument/Delete/5
 
        public ActionResult Delete(int id)
        {
            instrument instrument = db.instrument.Single(i => i.id == id);
            return View(instrument);
        }

        //
        // POST: /Instrument/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            instrument instrument = db.instrument.Single(i => i.id == id);
            db.instrument.DeleteObject(instrument);
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