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
    public class LiteratureChainController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /LiteratureChain/
        public ActionResult Index()
        {
            var literature_chain = db.literature_chain.Include(l => l.language).Include(l => l.matter).Include(l => l.notation).Include(l => l.piece_type).Include(l => l.translator).Include(l => l.remote_db);
            return View(literature_chain.ToList());
        }

        // GET: /LiteratureChain/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            literature_chain literature_chain = db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return HttpNotFound();
            }
            return View(literature_chain);
        }

        // GET: /LiteratureChain/Create
        public ActionResult Create()
        {
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.translator_id = new SelectList(db.translator, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        // POST: /LiteratureChain/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,translator_id,piece_position,original,language_id,remote_db_id,remote_id,modified,description")] literature_chain literature_chain)
        {
            if (ModelState.IsValid)
            {
                db.literature_chain.Add(literature_chain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.language_id = new SelectList(db.language, "id", "name", literature_chain.language_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", literature_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", literature_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", literature_chain.piece_type_id);
            ViewBag.translator_id = new SelectList(db.translator, "id", "name", literature_chain.translator_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        // GET: /LiteratureChain/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            literature_chain literature_chain = db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return HttpNotFound();
            }
            ViewBag.language_id = new SelectList(db.language, "id", "name", literature_chain.language_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", literature_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", literature_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", literature_chain.piece_type_id);
            ViewBag.translator_id = new SelectList(db.translator, "id", "name", literature_chain.translator_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        // POST: /LiteratureChain/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,translator_id,piece_position,original,language_id,remote_db_id,remote_id,modified,description")] literature_chain literature_chain)
        {
            if (ModelState.IsValid)
            {
                db.Entry(literature_chain).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.language_id = new SelectList(db.language, "id", "name", literature_chain.language_id);
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", literature_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", literature_chain.notation_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", literature_chain.piece_type_id);
            ViewBag.translator_id = new SelectList(db.translator, "id", "name", literature_chain.translator_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        // GET: /LiteratureChain/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            literature_chain literature_chain = db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return HttpNotFound();
            }
            return View(literature_chain);
        }

        // POST: /LiteratureChain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            literature_chain literature_chain = db.literature_chain.Find(id);
            db.literature_chain.Remove(literature_chain);
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
