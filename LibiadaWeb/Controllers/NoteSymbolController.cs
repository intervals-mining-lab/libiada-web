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
    public class NoteSymbolController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /NoteSymbol/
        public ActionResult Index()
        {
            return View(db.note_symbol.ToList());
        }

        // GET: /NoteSymbol/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            note_symbol note_symbol = db.note_symbol.Find(id);
            if (note_symbol == null)
            {
                return HttpNotFound();
            }
            return View(note_symbol);
        }

        // GET: /NoteSymbol/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /NoteSymbol/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] note_symbol note_symbol)
        {
            if (ModelState.IsValid)
            {
                db.note_symbol.Add(note_symbol);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(note_symbol);
        }

        // GET: /NoteSymbol/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            note_symbol note_symbol = db.note_symbol.Find(id);
            if (note_symbol == null)
            {
                return HttpNotFound();
            }
            return View(note_symbol);
        }

        // POST: /NoteSymbol/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] note_symbol note_symbol)
        {
            if (ModelState.IsValid)
            {
                db.Entry(note_symbol).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(note_symbol);
        }

        // GET: /NoteSymbol/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            note_symbol note_symbol = db.note_symbol.Find(id);
            if (note_symbol == null)
            {
                return HttpNotFound();
            }
            return View(note_symbol);
        }

        // POST: /NoteSymbol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            note_symbol note_symbol = db.note_symbol.Find(id);
            db.note_symbol.Remove(note_symbol);
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
