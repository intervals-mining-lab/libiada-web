﻿namespace LibiadaWeb.Controllers
{
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    /// <summary>
    /// The sequence groups controller.
    /// </summary>
    public class SequenceGroupsController : Controller
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            var sequenceGroups = db.SequenceGroup.Include(s => s.Creator).Include(s => s.Modifier);
            return View(await sequenceGroups.ToListAsync());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SequenceGroup sequenceGroup = await db.SequenceGroup.FindAsync(id);
            if (sequenceGroup == null)
            {
                return HttpNotFound();
            }

            return View(sequenceGroup);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="sequenceGroup">
        /// The sequence group.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] SequenceGroup sequenceGroup)
        {
            if (ModelState.IsValid)
            {
                db.SequenceGroup.Add(sequenceGroup);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(sequenceGroup);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SequenceGroup sequenceGroup = await db.SequenceGroup.FindAsync(id);
            if (sequenceGroup == null)
            {
                return HttpNotFound();
            }
            
            return View(sequenceGroup);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="sequenceGroup">
        /// The sequence group.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] SequenceGroup sequenceGroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sequenceGroup).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return View(sequenceGroup);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SequenceGroup sequenceGroup = await db.SequenceGroup.FindAsync(id);
            if (sequenceGroup == null)
            {
                return HttpNotFound();
            }

            return View(sequenceGroup);
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SequenceGroup sequenceGroup = await db.SequenceGroup.FindAsync(id);
            db.SequenceGroup.Remove(sequenceGroup);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
