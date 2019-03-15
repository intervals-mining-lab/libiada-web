namespace LibiadaWeb.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Tasks;

    /// <summary>
    /// The common sequences controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class FmotifsDictionaryController: SequencesMattersController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FmotifsDictionaryController"/> class.
        /// </summary>
        public FmotifsDictionaryController() : base(TaskType.CommonSequences)
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
                var musicSequence = db.MusicSequence.Include(m => m.Matter);
                return View(await musicSequence.ToListAsync());
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
                MusicSequence musicSequence = db.MusicSequence.Include(m => m.Matter).Single(m => m.Id == id);
                if (musicSequence == null)
                {
                    return HttpNotFound();
                }

                return View(musicSequence);
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
                MusicSequence musicSequence = await db.MusicSequence.FindAsync(id);
                if (musicSequence == null)
                {
                    return HttpNotFound();
                }

                ViewBag.MatterId = new SelectList(db.Matter.ToArray(), "Id", "Name", musicSequence.MatterId);
                ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), musicSequence.Notation);
                ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), musicSequence.RemoteDb);
                return View(musicSequence);
            }
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="musicSequence">
        /// The common sequence.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Notation,MatterId,RemoteDb,RemoteId,Description")] MusicSequence musicSequence)
        {
            using (var db = new LibiadaWebEntities())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(musicSequence).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                ViewBag.MatterId = new SelectList(db.Matter.ToArray(), "Id", "Name", musicSequence.MatterId);
                ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), musicSequence.Notation);
                ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), musicSequence.RemoteDb);
                return View(musicSequence);
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
                MusicSequence musicSequence = db.MusicSequence.Include(m => m.Matter).Single(m => m.Id == id);
                if (musicSequence == null)
                {
                    return HttpNotFound();
                }

                return View(musicSequence);
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
                MusicSequence musicSequence = await db.MusicSequence.FindAsync(id);
                db.MusicSequence.Remove(musicSequence);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }
    }
}
