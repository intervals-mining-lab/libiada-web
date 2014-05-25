// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NatureController.cs" company="">
//   
// </copyright>
// <summary>
//   The nature controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The nature controller.
    /// </summary>
    public class NatureController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Nature/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return this.View(this.db.nature.ToList());
        }

        // GET: /Nature/Details/5
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

            nature nature = this.db.nature.Find(id);
            if (nature == null)
            {
                return this.HttpNotFound();
            }

            return View(nature);
        }

        // GET: /Nature/Create
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

        // POST: /Nature/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="nature">
        /// The nature.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] nature nature)
        {
            if (this.ModelState.IsValid)
            {
                this.db.nature.Add(nature);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(nature);
        }

        // GET: /Nature/Edit/5
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

            nature nature = this.db.nature.Find(id);
            if (nature == null)
            {
                return this.HttpNotFound();
            }

            return View(nature);
        }

        // POST: /Nature/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="nature">
        /// The nature.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] nature nature)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(nature).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(nature);
        }

        // GET: /Nature/Delete/5
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

            nature nature = this.db.nature.Find(id);
            if (nature == null)
            {
                return this.HttpNotFound();
            }

            return View(nature);
        }

        // POST: /Nature/Delete/5
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
            nature nature = this.db.nature.Find(id);
            this.db.nature.Remove(nature);
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
