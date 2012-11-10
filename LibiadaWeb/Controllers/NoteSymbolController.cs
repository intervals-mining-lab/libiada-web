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
    public class NoteSymbolController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        //
        // GET: /NoteSymbol/

        public ViewResult Index()
        {
            return View(db.note_symbol.ToList());
        }

        //
        // GET: /NoteSymbol/Details/5

        public ViewResult Details(int id)
        {
            note_symbol note_symbol = db.note_symbol.Single(n => n.id == id);
            return View(note_symbol);
        }

        //
        // GET: /NoteSymbol/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /NoteSymbol/Create

        [HttpPost]
        public ActionResult Create(note_symbol note_symbol)
        {
            if (ModelState.IsValid)
            {
                db.note_symbol.AddObject(note_symbol);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(note_symbol);
        }
        
        //
        // GET: /NoteSymbol/Edit/5
 
        public ActionResult Edit(int id)
        {
            note_symbol note_symbol = db.note_symbol.Single(n => n.id == id);
            return View(note_symbol);
        }

        //
        // POST: /NoteSymbol/Edit/5

        [HttpPost]
        public ActionResult Edit(note_symbol note_symbol)
        {
            if (ModelState.IsValid)
            {
                db.note_symbol.Attach(note_symbol);
                db.ObjectStateManager.ChangeObjectState(note_symbol, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(note_symbol);
        }

        //
        // GET: /NoteSymbol/Delete/5
 
        public ActionResult Delete(int id)
        {
            note_symbol note_symbol = db.note_symbol.Single(n => n.id == id);
            return View(note_symbol);
        }

        //
        // POST: /NoteSymbol/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            note_symbol note_symbol = db.note_symbol.Single(n => n.id == id);
            db.note_symbol.DeleteObject(note_symbol);
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