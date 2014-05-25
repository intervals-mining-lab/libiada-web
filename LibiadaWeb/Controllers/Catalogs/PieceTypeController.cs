// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PieceTypeController.cs" company="">
//   
// </copyright>
// <summary>
//   The piece type controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The piece type controller.
    /// </summary>
    public class PieceTypeController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /PieceType/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var piece_type = this.db.piece_type.Include(p => p.nature);
            return View(piece_type.ToList());
        }

        // GET: /PieceType/Details/5
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

            piece_type piece_type = this.db.piece_type.Find(id);
            if (piece_type == null)
            {
                return this.HttpNotFound();
            }

            return View(piece_type);
        }

        // GET: /PieceType/Create
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

        // POST: /PieceType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="piece_type">
        /// The piece_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description,nature_id")] piece_type piece_type)
        {
            if (this.ModelState.IsValid)
            {
                this.db.piece_type.Add(piece_type);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", piece_type.nature_id);
            return View(piece_type);
        }

        // GET: /PieceType/Edit/5
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

            piece_type piece_type = this.db.piece_type.Find(id);
            if (piece_type == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", piece_type.nature_id);
            return View(piece_type);
        }

        // POST: /PieceType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="piece_type">
        /// The piece_type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description,nature_id")] piece_type piece_type)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(piece_type).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.nature_id = new SelectList(this.db.nature, "id", "name", piece_type.nature_id);
            return View(piece_type);
        }

        // GET: /PieceType/Delete/5
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

            piece_type piece_type = this.db.piece_type.Find(id);
            if (piece_type == null)
            {
                return this.HttpNotFound();
            }

            return View(piece_type);
        }

        // POST: /PieceType/Delete/5
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
            piece_type piece_type = this.db.piece_type.Find(id);
            this.db.piece_type.Remove(piece_type);
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
