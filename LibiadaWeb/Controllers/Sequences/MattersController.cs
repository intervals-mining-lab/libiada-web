namespace LibiadaWeb.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Models.Account;

    /// <summary>
    /// The matters controller.
    /// </summary>
    [Authorize]
    public class MattersController : SequencesMattersController
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            var matter = await Db.Matter.ToListAsync();

            if (!UserHelper.IsAdmin())
            {
                matter = matter.Where(m => m.Nature == Nature.Genetic).ToList();
            }

            return View(matter);
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

            Matter matter = await Db.Matter.FindAsync(id);
            if (matter == null)
            {
                return HttpNotFound();
            }

            return View(matter);
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

            Matter matter = await Db.Matter.FindAsync(id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.Nature = EnumHelper.GetSelectList(typeof(Nature), matter.Nature);
            return View(matter);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Nature,Description")] Matter matter)
        {
            if (ModelState.IsValid)
            {
                Db.Entry(matter).State = EntityState.Modified;
                await Db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Nature = EnumHelper.GetSelectList(typeof(Nature), matter.Nature);
            return View(matter);
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

            Matter matter = await Db.Matter.FindAsync(id);
            if (matter == null)
            {
                return HttpNotFound();
            }

            return View(matter);
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
            Matter matter = await Db.Matter.FindAsync(id);
            Db.Matter.Remove(matter);
            await Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
