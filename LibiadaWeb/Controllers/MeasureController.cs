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
    public class MeasureController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Measure/
        public ActionResult Index()
        {
            var measure = db.measure.Include(m => m.matter).Include(m => m.notation).Include(m => m.piece_type).Include(m => m.remote_db);
            return View(measure.ToList());
        }

        // GET: /Measure/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            measure measure = db.measure.Find(id);
            if (measure == null)
            {
                return HttpNotFound();
            }
            return View(measure);
        }

        // GET: /Measure/Create
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        // POST: /Measure/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,beats,beatbase,ticks_per_beat,fifths,remote_db_id,remote_id,modified")] measure measure)
        {
            if (ModelState.IsValid)
            {
                db.measure.Add(measure);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        // GET: /Measure/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            measure measure = db.measure.Find(id);
            if (measure == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        // POST: /Measure/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,beats,beatbase,ticks_per_beat,fifths,remote_db_id,remote_id,modified")] measure measure)
        {
            if (ModelState.IsValid)
            {
                db.Entry(measure).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", measure.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", measure.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", measure.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        // GET: /Measure/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            measure measure = db.measure.Find(id);
            if (measure == null)
            {
                return HttpNotFound();
            }
            return View(measure);
        }

        // POST: /Measure/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            measure measure = db.measure.Find(id);
            db.measure.Remove(measure);
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
