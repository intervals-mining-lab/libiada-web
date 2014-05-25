// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatterController.cs" company="">
//   
// </copyright>
// <summary>
//   The matter controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Chains
{
    using LibiadaWeb.Helpers;

    /// <summary>
    /// The matter controller.
    /// </summary>
    public class MatterController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Matter/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            this.ViewBag.dbName = DbHelper.GetDbName(this.db);
            var matter = this.db.matter.Include(m => m.nature);
            return View(matter.ToList());
        }

        // GET: /Matter/Details/5
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

            matter matter = this.db.matter.Find(id);
            if (matter == null)
            {
                return this.HttpNotFound();
            }

            return View(matter);
        }

        // GET: /Matter/Create
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name");
            return this.View();
        }

        // POST: /Matter/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,nature_id,description,created,modified")] matter matter)
        {
            if (this.ModelState.IsValid)
            {
                this.db.matter.Add(matter);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        // GET: /Matter/Edit/5
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

            matter matter = this.db.matter.Find(id);
            if (matter == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        // POST: /Matter/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,nature_id,description,created,modified")] matter matter)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(matter).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        // GET: /Matter/Delete/5
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

            matter matter = this.db.matter.Find(id);
            if (matter == null)
            {
                return this.HttpNotFound();
            }

            return View(matter);
        }

        // POST: /Matter/Delete/5
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
            matter matter = this.db.matter.Find(id);
            this.db.matter.Remove(matter);
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
