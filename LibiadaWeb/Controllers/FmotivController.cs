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
    public class FmotivController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Fmotiv/
        public ActionResult Index()
        {
            var fmotiv = db.fmotiv.Include(f => f.matter).Include(f => f.notation).Include(f => f.piece_type).Include(f => f.fmotiv_type).Include(f => f.remote_db);
            return View(fmotiv.ToList());
        }

        // GET: /Fmotiv/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            fmotiv fmotiv = db.fmotiv.Find(id);
            if (fmotiv == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv);
        }

        // GET: /Fmotiv/Create
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        // POST: /Fmotiv/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,fmotiv_type_id,remote_db_id,remote_id,modified")] fmotiv fmotiv)
        {
            if (ModelState.IsValid)
            {
                db.fmotiv.Add(fmotiv);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", fmotiv.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", fmotiv.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", fmotiv.piece_type_id);
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", fmotiv.remote_db_id);
            return View(fmotiv);
        }

        // GET: /Fmotiv/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            fmotiv fmotiv = db.fmotiv.Find(id);
            if (fmotiv == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", fmotiv.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", fmotiv.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", fmotiv.piece_type_id);
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", fmotiv.remote_db_id);
            return View(fmotiv);
        }

        // POST: /Fmotiv/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,fmotiv_type_id,remote_db_id,remote_id,modified")] fmotiv fmotiv)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fmotiv).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", fmotiv.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", fmotiv.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", fmotiv.piece_type_id);
            ViewBag.fmotiv_type_id = new SelectList(db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", fmotiv.remote_db_id);
            return View(fmotiv);
        }

        // GET: /Fmotiv/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            fmotiv fmotiv = db.fmotiv.Find(id);
            if (fmotiv == null)
            {
                return HttpNotFound();
            }
            return View(fmotiv);
        }

        // POST: /Fmotiv/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            fmotiv fmotiv = db.fmotiv.Find(id);
            db.fmotiv.Remove(fmotiv);
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
