namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The dna chain controller.
    /// </summary>
    public class DnaChainController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var dna_chain = db.dna_chain.Include(d => d.matter)
                                        .Include(d => d.notation)
                                        .Include(d => d.product)
                                        .Include(d => d.piece_type)
                                        .Include(d => d.remote_db);
            return View(dna_chain.ToList());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            ViewBag.matter_id = new SelectList(db.matter, "id", "name");
            ViewBag.notation_id = new SelectList(db.notation, "id", "name");
            ViewBag.product_id = new SelectList(db.product, "id", "name");
            ViewBag.piece_type_id = new SelectList(db.piece_type, "id", "name");
            ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="dna_chain">
        /// The dna_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,notation_id,product_id,created,matter_id,piece_type_id,piece_position,remote_db_id,web_api_id,remote_id,fasta_header,modified,complement,partial,description")] dna_chain dna_chain)
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

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="dna_chain">
        /// The dna_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,notation_id,product_id,created,matter_id,piece_type_id,piece_position,remote_db_id,web_api_id,remote_id,fasta_header,modified,complement,partial,description")] dna_chain dna_chain)
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

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            dna_chain dna_chain = db.dna_chain.Find(id);
            db.dna_chain.Remove(dna_chain);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
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
