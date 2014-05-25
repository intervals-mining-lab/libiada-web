// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductController.cs" company="">
//   
// </copyright>
// <summary>
//   The product controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The product controller.
    /// </summary>
    public class ProductController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Product/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var product = this.db.product.Include(p => p.piece_type);
            return View(product.ToList());
        }

        // GET: /Product/Details/5
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

            product product = this.db.product.Find(id);
            if (product == null)
            {
                return this.HttpNotFound();
            }

            return View(product);
        }

        // GET: /Product/Create
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name");
            return this.View();
        }

        // POST: /Product/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="product">
        /// The product.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,piece_type_id")] product product)
        {
            if (this.ModelState.IsValid)
            {
                this.db.product.Add(product);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", product.piece_type_id);
            return View(product);
        }

        // GET: /Product/Edit/5
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

            product product = this.db.product.Find(id);
            if (product == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", product.piece_type_id);
            return View(product);
        }

        // POST: /Product/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="product">
        /// The product.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,piece_type_id")] product product)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(product).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.piece_type_id = new SelectList(this.db.piece_type, "id", "name", product.piece_type_id);
            return View(product);
        }

        // GET: /Product/Delete/5
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

            product product = this.db.product.Find(id);
            if (product == null)
            {
                return this.HttpNotFound();
            }

            return View(product);
        }

        // POST: /Product/Delete/5
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
            product product = this.db.product.Find(id);
            this.db.product.Remove(product);
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
