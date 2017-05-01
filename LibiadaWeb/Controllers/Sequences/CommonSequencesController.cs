namespace LibiadaWeb.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Tasks;

    /// <summary>
    /// The common sequences controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CommonSequencesController : SequencesMattersController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonSequencesController"/> class.
        /// </summary>
        public CommonSequencesController() : base(TaskType.CommonSequences)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                var commonSequence = db.CommonSequence.Include(c => c.Matter);
                return View(await commonSequence.ToListAsync());
            }
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
                if (commonSequence == null)
                {
                    return HttpNotFound();
                }

                return View(commonSequence);
            }
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
                if (commonSequence == null)
                {
                    return HttpNotFound();
                }

                ViewBag.MatterId = new SelectList(db.Matter, "Id", "Name", commonSequence.MatterId);
                ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), commonSequence.Notation);
                ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), commonSequence.RemoteDb);
                return View(commonSequence);
            }
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Notation,MatterId,RemoteDb,RemoteId,Description")] CommonSequence commonSequence)
        {
            using (var db = new LibiadaWebEntities())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(commonSequence).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                ViewBag.MatterId = new SelectList(db.Matter, "Id", "Name", commonSequence.MatterId);
                ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), commonSequence.Notation);
                ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), commonSequence.RemoteDb);
                return View(commonSequence);
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
                if (commonSequence == null)
                {
                    return HttpNotFound();
                }

                return View(commonSequence);
            }
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            using (var db = new LibiadaWebEntities())
            {
                CommonSequence commonSequence = await db.CommonSequence.FindAsync(id);
                db.CommonSequence.Remove(commonSequence);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }
    }
}
