// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacteristicGroupController.cs" company="">
//   
// </copyright>
// <summary>
//   The characteristic group controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The characteristic group controller.
    /// </summary>
    public class CharacteristicGroupController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /CharacteristicGroup/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return this.View(this.db.characteristic_group.ToList());
        }

        // GET: /CharacteristicGroup/Details/5
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

            characteristic_group characteristic_group = this.db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_group);
        }

        // GET: /CharacteristicGroup/Create
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

        // POST: /CharacteristicGroup/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="characteristic_group">
        /// The characteristic_group.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] characteristic_group characteristic_group)
        {
            if (this.ModelState.IsValid)
            {
                this.db.characteristic_group.Add(characteristic_group);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(characteristic_group);
        }

        // GET: /CharacteristicGroup/Edit/5
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

            characteristic_group characteristic_group = this.db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_group);
        }

        // POST: /CharacteristicGroup/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="characteristic_group">
        /// The characteristic_group.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] characteristic_group characteristic_group)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(characteristic_group).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(characteristic_group);
        }

        // GET: /CharacteristicGroup/Delete/5
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

            characteristic_group characteristic_group = this.db.characteristic_group.Find(id);
            if (characteristic_group == null)
            {
                return this.HttpNotFound();
            }

            return View(characteristic_group);
        }

        // POST: /CharacteristicGroup/Delete/5
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
            characteristic_group characteristic_group = this.db.characteristic_group.Find(id);
            this.db.characteristic_group.Remove(characteristic_group);
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
