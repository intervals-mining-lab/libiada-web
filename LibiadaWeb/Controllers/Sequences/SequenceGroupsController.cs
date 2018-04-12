namespace LibiadaWeb.Controllers.Sequences
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The sequence groups controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
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
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                var viewData = viewDataHelper.FillViewData(1, int.MaxValue);
                ViewBag.data = JsonConvert.SerializeObject(viewData);
                return View();
            }
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="sequenceGroup">
        /// The sequence group.
        /// </param>
        /// <param name="matterIds">
        /// The matters Ids.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Nature")] SequenceGroup sequenceGroup, long[] matterIds)
        {
            if (ModelState.IsValid)
            {
                sequenceGroup.CreatorId = AccountHelper.GetUserId();
                sequenceGroup.ModifierId = AccountHelper.GetUserId();
                var matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToArray();
                foreach (var matter in matters)
                {
                    sequenceGroup.Matters.Add(matter);
                }

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

            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                var viewData = viewDataHelper.FillViewData(1, int.MaxValue);
                var matterRepository = new MatterRepository(db);
                bool Selected(Matter m) => sequenceGroup.Matters.Contains(m);
                viewData["matters"] = matterRepository.GetMatterSelectList(db.Matter, Selected);
                ViewBag.data = JsonConvert.SerializeObject(viewData);
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Nature,CreatorId,Created")] SequenceGroup sequenceGroup)
        {
            if (ModelState.IsValid)
            {
                sequenceGroup.ModifierId = AccountHelper.GetUserId();
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
