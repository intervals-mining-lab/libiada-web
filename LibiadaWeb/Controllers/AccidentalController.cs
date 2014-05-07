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
    public class AccidentalController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Accidental/
        public ActionResult Index()
        {
            return View(db.accidental.ToList());
        }

        // GET: /Accidental/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            accidental accidental = db.accidental.Find(id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        // GET: /Accidental/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Accidental/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] accidental accidental)
        {
            if (ModelState.IsValid)
            {
                db.accidental.Add(accidental);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(accidental);
        }

        // GET: /Accidental/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            accidental accidental = db.accidental.Find(id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        // POST: /Accidental/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] accidental accidental)
        {
            if (ModelState.IsValid)
            {
                db.Entry(accidental).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(accidental);
        }

        // GET: /Accidental/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            accidental accidental = db.accidental.Find(id);
            if (accidental == null)
            {
                return HttpNotFound();
            }
            return View(accidental);
        }

        // POST: /Accidental/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            accidental accidental = db.accidental.Find(id);
            db.accidental.Remove(accidental);
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
