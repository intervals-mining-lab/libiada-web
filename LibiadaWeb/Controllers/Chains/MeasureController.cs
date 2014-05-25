// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasureController.cs" company="">
//   
// </copyright>
// <summary>
//   The measure controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    /// <summary>
    /// The measure controller.
    /// </summary>
    public class MeasureController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Measure/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var measure = this.db.measure.Include(m => m.matter).Include(m => m.notation).Include(m => m.piece_type).Include(m => m.remote_db);
            return View(measure.ToList());
        }

        // GET: /Measure/Details/5
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

            measure measure = this.db.measure.Find(id);
            if (measure == null)
            {
                return this.HttpNotFound();
            }

            return View(measure);
        }

        // GET: /Measure/Create
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
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name");
            return this.View();
        }

        // POST: /Measure/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="measure">
        /// The measure.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,beats,beatbase,ticks_per_beat,fifths,remote_db_id,remote_id,modified")] measure measure)
        {
            if (this.ModelState.IsValid)
            {
                this.db.measure.Add(measure);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", measure.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", measure.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", measure.piece_type_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        // GET: /Measure/Edit/5
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

            measure measure = this.db.measure.Find(id);
            if (measure == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", measure.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", measure.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", measure.piece_type_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        // POST: /Measure/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="measure">
        /// The measure.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,notation_id,created,matter_id,dissimilar,piece_type_id,piece_position,value,description,name,beats,beatbase,ticks_per_beat,fifths,remote_db_id,remote_id,modified")] measure measure)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(measure).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.matter_id = new SelectList(this.db.matter, "id", "name", measure.matter_id);
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", measure.notation_id);
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", measure.piece_type_id);
            this.ViewBag.remote_db_id = new SelectList(this.db.remote_db, "id", "name", measure.remote_db_id);
            return View(measure);
        }

        // GET: /Measure/Delete/5
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

            measure measure = this.db.measure.Find(id);
            if (measure == null)
            {
                return this.HttpNotFound();
            }

            return View(measure);
        }

        // POST: /Measure/Delete/5
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
            measure measure = this.db.measure.Find(id);
            this.db.measure.Remove(measure);
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
