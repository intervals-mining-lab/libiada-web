// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FmotivTypeController.cs" company="">
//   
// </copyright>
// <summary>
//   The fmotiv type controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The fmotiv type controller.
    /// </summary>
    public class FmotivTypeController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /FmotivType/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return this.View(this.db.fmotiv_type.ToList());
        }

        // GET: /FmotivType/Details/5
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

            fmotiv_type fmotiv_type = this.db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return this.HttpNotFound();
            }

            return View(fmotiv_type);
        }

        // GET: /FmotivType/Create
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

        // POST: /FmotivType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="fmotiv_type">
        /// The fmotiv_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] fmotiv_type fmotiv_type)
        {
            if (this.ModelState.IsValid)
            {
                this.db.fmotiv_type.Add(fmotiv_type);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(fmotiv_type);
        }

        // GET: /FmotivType/Edit/5
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

            fmotiv_type fmotiv_type = this.db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return this.HttpNotFound();
            }

            return View(fmotiv_type);
        }

        // POST: /FmotivType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="fmotiv_type">
        /// The fmotiv_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] fmotiv_type fmotiv_type)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(fmotiv_type).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(fmotiv_type);
        }

        // GET: /FmotivType/Delete/5
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

            fmotiv_type fmotiv_type = this.db.fmotiv_type.Find(id);
            if (fmotiv_type == null)
            {
                return this.HttpNotFound();
            }

            return View(fmotiv_type);
        }

        // POST: /FmotivType/Delete/5
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
            fmotiv_type fmotiv_type = this.db.fmotiv_type.Find(id);
            this.db.fmotiv_type.Remove(fmotiv_type);
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
