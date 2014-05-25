// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorController.cs" company="">
//   
// </copyright>
// <summary>
//   The translator controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Catalogs
{
    /// <summary>
    /// The translator controller.
    /// </summary>
    public class TranslatorController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        // GET: /Translator/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            return this.View(this.db.translator.ToList());
        }

        // GET: /Translator/Details/5
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

            translator translator = this.db.translator.Find(id);
            if (translator == null)
            {
                return this.HttpNotFound();
            }

            return View(translator);
        }

        // GET: /Translator/Create
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

        // POST: /Translator/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="translator">
        /// The translator.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,name,description")] translator translator)
        {
            if (this.ModelState.IsValid)
            {
                this.db.translator.Add(translator);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(translator);
        }

        // GET: /Translator/Edit/5
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

            translator translator = this.db.translator.Find(id);
            if (translator == null)
            {
                return this.HttpNotFound();
            }

            return View(translator);
        }

        // POST: /Translator/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="translator">
        /// The translator.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,name,description")] translator translator)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(translator).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return View(translator);
        }

        // GET: /Translator/Delete/5
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

            translator translator = this.db.translator.Find(id);
            if (translator == null)
            {
                return this.HttpNotFound();
            }

            return View(translator);
        }

        // POST: /Translator/Delete/5
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
            translator translator = this.db.translator.Find(id);
            this.db.translator.Remove(translator);
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
