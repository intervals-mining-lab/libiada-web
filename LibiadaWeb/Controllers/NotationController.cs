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
    public class NotationController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Notation/
        public ActionResult Index()
        {
            var notation = db.notation.Include(n => n.nature);
            return View(notation.ToList());
        }

        // GET: /Notation/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            notation notation = db.notation.Find(id);
            if (notation == null)
            {
                return HttpNotFound();
            }
            return View(notation);
        }

        // GET: /Notation/Create
        public ActionResult Create()
        {
            ViewBag.nature_id = new SelectList(db.nature, "id", "name");
            return View();
        }

        // POST: /Notation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,nature_id")] notation notation)
        {
            if (ModelState.IsValid)
            {
                db.notation.Add(notation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", notation.nature_id);
            return View(notation);
        }

        // GET: /Notation/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            notation notation = db.notation.Find(id);
            if (notation == null)
            {
                return HttpNotFound();
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", notation.nature_id);
            return View(notation);
        }

        // POST: /Notation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,nature_id")] notation notation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", notation.nature_id);
            return View(notation);
        }

        // GET: /Notation/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            notation notation = db.notation.Find(id);
            if (notation == null)
            {
                return HttpNotFound();
            }
            return View(notation);
        }

        // POST: /Notation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            notation notation = db.notation.Find(id);
            db.notation.Remove(notation);
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
