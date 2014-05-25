// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstrumentController.cs" company="">
//   
// </copyright>
// <summary>
//   The instrument controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The instrument controller.
    /// </summary>
    public class InstrumentController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Instrument/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return this.View(this.db.instrument.ToList());
        }

        // GET: /Instrument/Details/5
        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            instrument instrument = this.db.instrument.Find(id);
            if (instrument == null)
            {
                return this.HttpNotFound();
            }

            return View(instrument);
        }

        // GET: /Instrument/Create
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            return this.View();
        }

        // POST: /Instrument/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="instrument">
        /// The instrument.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] instrument instrument)
        {
            if (this.ModelState.IsValid)
            {
                this.db.instrument.Add(instrument);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(instrument);
        }

        // GET: /Instrument/Edit/5
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            instrument instrument = this.db.instrument.Find(id);
            if (instrument == null)
            {
                return this.HttpNotFound();
            }

            return View(instrument);
        }

        // POST: /Instrument/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="instrument">
        /// The instrument.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] instrument instrument)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(instrument).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(instrument);
        }

        // GET: /Instrument/Delete/5
        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            instrument instrument = this.db.instrument.Find(id);
            if (instrument == null)
            {
                return this.HttpNotFound();
            }

            return View(instrument);
        }

        // POST: /Instrument/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            instrument instrument = this.db.instrument.Find(id);
            this.db.instrument.Remove(instrument);
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
