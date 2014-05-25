// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LiteratureChainController.cs" company="">
//   
// </copyright>
// <summary>
//   The literature chain controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    /// <summary>
    /// The literature chain controller.
    /// </summary>
    public class LiteratureChainController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /LiteratureChain/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var literature_chain = this.db.literature_chain.Include(l => l.language).Include(l => l.matter).Include(l => l.notation).Include(l => l.piece_type).Include(l => l.translator).Include(l => l.remote_db);
            return View(literature_chain.ToList());
        }

        // GET: /LiteratureChain/Details/5
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

            literature_chain literature_chain = this.db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return this.HttpNotFound();
            }

            return View(literature_chain);
        }

        // GET: /LiteratureChain/Create
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.language_id = new SelectList(this.db.language, "id", "name");
            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name");
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name");
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name");
            this.ViewBag.translator_id = new SelectList(this.db.translator, "id", "name");
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name");
            return this.View();
        }

        // POST: /LiteratureChain/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="literature_chain">
        /// The literature_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,translator_id,piece_position,original,language_id,remote_db_id,remote_id,modified,description")] literature_chain literature_chain)
        {
            if (this.ModelState.IsValid)
            {
                this.db.literature_chain.Add(literature_chain);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.language_id = new SelectList(this.db.language, "id", "name", literature_chain.language_id);
            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", literature_chain.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", literature_chain.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", literature_chain.piece_type_id);
            this.ViewBag.translator_id = new SelectList(this.db.translator, "id", "name", literature_chain.translator_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        // GET: /LiteratureChain/Edit/5
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

            literature_chain literature_chain = this.db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.language_id = new SelectList(this.db.language, "id", "name", literature_chain.language_id);
            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", literature_chain.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", literature_chain.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", literature_chain.piece_type_id);
            this.ViewBag.translator_id = new SelectList(this.db.translator, "id", "name", literature_chain.translator_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        // POST: /LiteratureChain/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="literature_chain">
        /// The literature_chain.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,translator_id,piece_position,original,language_id,remote_db_id,remote_id,modified,description")] literature_chain literature_chain)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(literature_chain).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.language_id = new SelectList(this.db.language, "id", "name", literature_chain.language_id);
            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", literature_chain.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", literature_chain.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", literature_chain.piece_type_id);
            this.ViewBag.translator_id = new SelectList(this.db.translator, "id", "name", literature_chain.translator_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", literature_chain.remote_db_id);
            return View(literature_chain);
        }

        // GET: /LiteratureChain/Delete/5
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

            literature_chain literature_chain = this.db.literature_chain.Find(id);
            if (literature_chain == null)
            {
                return this.HttpNotFound();
            }

            return View(literature_chain);
        }

        // POST: /LiteratureChain/Delete/5
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
            literature_chain literature_chain = this.db.literature_chain.Find(id);
            this.db.literature_chain.Remove(literature_chain);
            this.db.SaveChanges();
            return this.RedirectToAction("Index");
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
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
