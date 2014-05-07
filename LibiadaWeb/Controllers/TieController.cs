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
    public class TieController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Tie/
        public ActionResult Index()
        {
            return View(db.tie.ToList());
        }

        // GET: /Tie/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tie tie = db.tie.Find(id);
            if (tie == null)
            {
                return HttpNotFound();
            }
            return View(tie);
        }

        // GET: /Tie/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Tie/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] tie tie)
        {
            if (ModelState.IsValid)
            {
                db.tie.Add(tie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tie);
        }

        // GET: /Tie/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tie tie = db.tie.Find(id);
            if (tie == null)
            {
                return HttpNotFound();
            }
            return View(tie);
        }

        // POST: /Tie/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] tie tie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tie);
        }

        // GET: /Tie/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tie tie = db.tie.Find(id);
            if (tie == null)
            {
                return HttpNotFound();
            }
            return View(tie);
        }

        // POST: /Tie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tie tie = db.tie.Find(id);
            db.tie.Remove(tie);
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
