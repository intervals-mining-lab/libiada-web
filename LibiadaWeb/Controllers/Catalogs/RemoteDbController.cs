// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteDbController.cs" company="">
//   
// </copyright>
// <summary>
//   The remote db controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The remote db controller.
    /// </summary>
    public class RemoteDbController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /RemoteDb/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var remote_db = this.db.remote_db.Include(r => r.nature);
            return View(remote_db.ToList());
        }

        // GET: /RemoteDb/Details/5
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

            remote_db remote_db = this.db.remote_db.Find(id);
            if (remote_db == null)
            {
                return this.HttpNotFound();
            }

            return View(remote_db);
        }

        // GET: /RemoteDb/Create
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

        // POST: /RemoteDb/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="remote_db">
        /// The remote_db.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,url,nature_id")] remote_db remote_db)
        {
            if (this.ModelState.IsValid)
            {
                this.db.remote_db.Add(remote_db);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        // GET: /RemoteDb/Edit/5
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

            remote_db remote_db = this.db.remote_db.Find(id);
            if (remote_db == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        // POST: /RemoteDb/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="remote_db">
        /// The remote_db.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,url,nature_id")] remote_db remote_db)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(remote_db).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", remote_db.nature_id);
            return View(remote_db);
        }

        // GET: /RemoteDb/Delete/5
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

            remote_db remote_db = this.db.remote_db.Find(id);
            if (remote_db == null)
            {
                return this.HttpNotFound();
            }

            return View(remote_db);
        }

        // POST: /RemoteDb/Delete/5
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
            remote_db remote_db = this.db.remote_db.Find(id);
            this.db.remote_db.Remove(remote_db);
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
