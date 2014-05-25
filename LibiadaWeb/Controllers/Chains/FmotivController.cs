// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FmotivController.cs" company="">
//   
// </copyright>
// <summary>
//   The fmotiv controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    /// <summary>
    /// The fmotiv controller.
    /// </summary>
    public class FmotivController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Fmotiv/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var fmotiv = this.db.fmotiv.Include(f => f.matter).Include(f => f.notation).Include(f => f.piece_type).Include(f => f.fmotiv_type).Include(f => f.remote_db);
            return View(fmotiv.ToList());
        }

        // GET: /Fmotiv/Details/5
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

            fmotiv fmotiv = this.db.fmotiv.Find(id);
            if (fmotiv == null)
            {
                return this.HttpNotFound();
            }

            return View(fmotiv);
        }

        // GET: /Fmotiv/Create
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name");
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name");
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name");
            this.ViewBag.fmotiv_type_id = new SelectList(this.db.fmotiv_type, "id", "name");
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name");
            return this.View();
        }

        // POST: /Fmotiv/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="fmotiv">
        /// The fmotiv.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,fmotiv_type_id,remote_db_id,remote_id,modified")] fmotiv fmotiv)
        {
            if (this.ModelState.IsValid)
            {
                this.db.fmotiv.Add(fmotiv);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", fmotiv.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", fmotiv.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", fmotiv.piece_type_id);
            this.ViewBag.fmotiv_type_id = new SelectList(this.db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", fmotiv.remote_db_id);
            return View(fmotiv);
        }

        // GET: /Fmotiv/Edit/5
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

            fmotiv fmotiv = this.db.fmotiv.Find(id);
            if (fmotiv == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", fmotiv.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", fmotiv.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", fmotiv.piece_type_id);
            this.ViewBag.fmotiv_type_id = new SelectList(this.db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", fmotiv.remote_db_id);
            return View(fmotiv);
        }

        // POST: /Fmotiv/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="fmotiv">
        /// The fmotiv.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,fmotiv_type_id,remote_db_id,remote_id,modified")] fmotiv fmotiv)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(fmotiv).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", fmotiv.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", fmotiv.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", fmotiv.piece_type_id);
            this.ViewBag.fmotiv_type_id = new SelectList(this.db.fmotiv_type, "id", "name", fmotiv.fmotiv_type_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", fmotiv.remote_db_id);
            return View(fmotiv);
        }

        // GET: /Fmotiv/Delete/5
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

            fmotiv fmotiv = this.db.fmotiv.Find(id);
            if (fmotiv == null)
            {
                return this.HttpNotFound();
            }

            return View(fmotiv);
        }

        // POST: /Fmotiv/Delete/5
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
            fmotiv fmotiv = this.db.fmotiv.Find(id);
            this.db.fmotiv.Remove(fmotiv);
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
