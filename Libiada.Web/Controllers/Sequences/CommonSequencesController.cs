namespace Libiada.Web.Controllers.Sequences
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using Libiada.Database.Tasks;
    using Libiada.Web.Extensions;
    using Libiada.Web.Helpers;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The common sequences controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CommonSequencesController : SequencesMattersController
    {
        private readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonSequencesController"/> class.
        /// </summary>
        public CommonSequencesController(LibiadaDatabaseEntities db, 
                                         IViewDataHelper viewDataHelper, 
                                         ITaskManager taskManager,
                                         Cache cache)
            : base(TaskType.CommonSequences, db, viewDataHelper, taskManager, cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            var commonSequence = db.CommonSequences.Include(c => c.Matter);
            return View(await commonSequence.ToListAsync());

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
                return BadRequest();
            }

            CommonSequence commonSequence = db.CommonSequences.Include(c => c.Matter).Single(c => c.Id == id);
            if (commonSequence == null)
            {
                return NotFound();
            }

            return View(commonSequence);

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
                return BadRequest();
            }


            CommonSequence commonSequence = await db.CommonSequences.FindAsync(id);
            if (commonSequence == null)
            {
                return NotFound();
            }
            var remoteDb = commonSequence.RemoteDb == null ? Array.Empty<RemoteDb>() : new[] { (RemoteDb)commonSequence.RemoteDb };
            ViewBag.MatterId = new SelectList(cache.Matters.ToArray(), "Id", "Name", commonSequence.MatterId);
            ViewBag.Notation = EnumExtensions.GetSelectList(new[] { commonSequence.Notation });
            ViewBag.RemoteDb = EnumExtensions.GetSelectList(remoteDb);
            return View(commonSequence);
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
        public async Task<ActionResult> Edit(//[Bind(Include = "Id,Notation,MatterId,RemoteDb,RemoteId,Description")] 
        CommonSequence commonSequence)
        {
            if (ModelState.IsValid)
            {
                db.Entry(commonSequence).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var remoteDb = commonSequence.RemoteDb == null ? Array.Empty<RemoteDb>() : new[] { (RemoteDb)commonSequence.RemoteDb };

            ViewBag.MatterId = new SelectList(cache.Matters.ToArray(), "Id", "Name", commonSequence.MatterId);
            ViewBag.Notation = EnumExtensions.GetSelectList(new[] { commonSequence.Notation });
            ViewBag.RemoteDb = EnumExtensions.GetSelectList(remoteDb);
            return View(commonSequence);
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
                return BadRequest();
            }

            CommonSequence commonSequence = db.CommonSequences.Include(c => c.Matter).Single(c => c.Id == id);
            if (commonSequence == null)
            {
                return NotFound();
            }

            return View(commonSequence);
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
            CommonSequence commonSequence = await db.CommonSequences.FindAsync(id);
            db.CommonSequences.Remove(commonSequence);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
