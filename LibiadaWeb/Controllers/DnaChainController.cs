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
    public class DnaChainController : Controller
    {
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /DnaChain/
        public ActionResult Index()
        {
            var dna_chain = db.dna_chain.Include(d => d.matter).Include(d => d.notation).Include(d => d.product).Include(d => d.piece_type).Include(d => d.remote_db);
            return View(dna_chain.ToList());
        }

        // GET: /DnaChain/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            dna_chain dna_chain = db.dna_chain.Find(id);
            if (dna_chain == null)
            {
                return HttpNotFound();
            }
            return View(dna_chain);
        }

        // GET: /DnaChain/Create
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.product_id = new SelectList(db.product, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        // POST: /DnaChain/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,product_id,created,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,web_api_id,remote_id,fasta_header,modified,complement,partial,description")] dna_chain dna_chain)
        {
            if (ModelState.IsValid)
            {
                db.dna_chain.Add(dna_chain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.matter_id = new SelectList(db.matter, "id", "name", dna_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", dna_chain.notation_id);
            ViewBag.product_id = new SelectList(db.product, "id", "name", dna_chain.product_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", dna_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", dna_chain.remote_db_id);
            return View(dna_chain);
        }

        // GET: /DnaChain/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            dna_chain dna_chain = db.dna_chain.Find(id);
            if (dna_chain == null)
            {
                return HttpNotFound();
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", dna_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", dna_chain.notation_id);
            ViewBag.product_id = new SelectList(db.product, "id", "name", dna_chain.product_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", dna_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", dna_chain.remote_db_id);
            return View(dna_chain);
        }

        // POST: /DnaChain/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,product_id,created,matter_id,dissimilar,piece_type_id,piece_position,remote_db_id,web_api_id,remote_id,fasta_header,modified,complement,partial,description")] dna_chain dna_chain)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dna_chain).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.matter_id = new SelectList(db.matter, "id", "name", dna_chain.matter_id);
            ViewBag.notation_id = new SelectList(db.notation, "id", "name", dna_chain.notation_id);
            ViewBag.product_id = new SelectList(db.product, "id", "name", dna_chain.product_id);
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name", dna_chain.piece_type_id);
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name", dna_chain.remote_db_id);
            return View(dna_chain);
        }

        // GET: /DnaChain/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            dna_chain dna_chain = db.dna_chain.Find(id);
            if (dna_chain == null)
            {
                return HttpNotFound();
            }
            return View(dna_chain);
        }

        // POST: /DnaChain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            dna_chain dna_chain = db.dna_chain.Find(id);
            db.dna_chain.Remove(dna_chain);
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
