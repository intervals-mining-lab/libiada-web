namespace LibiadaWeb.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Extensions;

    /// <summary>
    /// The common sequences controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CommonSequencesController : SequencesMattersController
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            var commonSequence = Db.CommonSequence.Include(c => c.Matter)
                                .Include(c => c.Notation)
                                .Include(c => c.RemoteDb);
            return View(await commonSequence.ToListAsync());
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
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommonSequence commonSequence = await Db.CommonSequence.FindAsync(id);
            if (commonSequence == null)
            {
                return HttpNotFound();
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
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommonSequence commonSequence = await Db.CommonSequence.FindAsync(id);
            if (commonSequence == null)
            {
                return HttpNotFound();
            }


            ViewBag.MatterId = new SelectList(Db.Matter, "Id", "Name", commonSequence.MatterId);
            ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), commonSequence.Notation);
            ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), commonSequence.RemoteDb);
            return View(commonSequence);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Notation,MatterId,RemoteDb,RemoteId,Description")] CommonSequence commonSequence)
        {
            if (ModelState.IsValid)
            {
                Db.Entry(commonSequence).State = EntityState.Modified;
                await Db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MatterId = new SelectList(Db.Matter, "Id", "Name", commonSequence.MatterId);
            ViewBag.Notation = EnumHelper.GetSelectList(typeof(Notation), commonSequence.Notation);
            ViewBag.RemoteDb = EnumHelper.GetSelectList(typeof(RemoteDb), commonSequence.RemoteDb);
            return View(commonSequence);
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
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CommonSequence commonSequence = await Db.CommonSequence.FindAsync(id);
            if (commonSequence == null)
            {
                return HttpNotFound();
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
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            CommonSequence commonSequence = await Db.CommonSequence.FindAsync(id);
            Db.CommonSequence.Remove(commonSequence);
            await Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
