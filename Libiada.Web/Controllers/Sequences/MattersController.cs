namespace Libiada.Web.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    using Libiada.Web.Extensions;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;
    using Libiada.Web.Helpers;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The matters controller.
    /// </summary>
    [Authorize]
    public class MattersController : SequencesMattersController
    {
        private readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MattersController"/> class.
        /// </summary>
        public MattersController(LibiadaDatabaseEntities db, 
                                 IViewDataHelper viewDataHelper, 
                                 ITaskManager taskManager,
                                 Cache cache)
            : base(TaskType.Matters, db, viewDataHelper, taskManager, cache)
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
            List<Matter> matter = await db.Matter.Include(m => m.Multisequence).ToListAsync();

            if (!User.IsInRole("admin"))
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
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Matter matter = db.Matter.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
            if (matter == null)
            {
                return NotFound();
            }

            ViewBag.SequencesCount = db.CommonSequence.Count(c => c.MatterId == matter.Id);

            return View(matter);

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
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Matter matter = db.Matter.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
            if (matter == null)
            {
                return NotFound();
            }
            var data = new Dictionary<string, object>
                {
                    { "natures", Libiada.Web.Extensions.EnumExtensions.GetSelectList(new[] { matter.Nature }) },
                    { "groups", EnumExtensions.ToArray<Group>().ToSelectListWithNature() },
                    { "sequenceTypes", EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature() },
                    { "sequencesCount", db.CommonSequence.Count(c => c.MatterId == matter.Id) },
                    { "matter", matter },
                    { "multisequences", db.Multisequence.ToList() },
                    { "nature", ((byte)matter.Nature).ToString() },
                    { "group", ((byte)matter.Group).ToString() },
                    { "sequenceType", ((byte)matter.SequenceType).ToString() }
                };

            ViewBag.data = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return View(matter);

        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
        // [Bind(Include = "Id,Name,Nature,Description,Group,SequenceType,MultisequenceId,MultisequenceNumber,CollectionCountry,CollectionDate")] 
        Matter matter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(matter).State = EntityState.Modified;
                await db.SaveChangesAsync();
                cache.Clear();
                return RedirectToAction("Index");
            }

            var data = new Dictionary<string, object>
                {
                    { "natures", Libiada.Web.Extensions.EnumExtensions.GetSelectList(new[] { matter.Nature }) },
                    { "groups", EnumExtensions.ToArray<Group>().ToSelectListWithNature() },
                    { "sequenceTypes", EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature() },
                    { "sequencesCount", db.CommonSequence.Count(c => c.MatterId == matter.Id) },
                    { "matter", matter },
                    { "multisequences", db.Multisequence.ToList() },
                    { "nature", ((byte)matter.Nature).ToString() },
                    { "group", ((byte)matter.Group).ToString() },
                    { "sequenceType", ((byte)matter.SequenceType).ToString() }
                };

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View(matter);

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

            Matter matter = await db.Matter.FindAsync(id);
            if (matter == null)
            {
                return NotFound();
            }

            ViewBag.SequencesCount = db.CommonSequence.Count(c => c.MatterId == matter.Id);
            return View(matter);

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
            Matter matter = await db.Matter.FindAsync(id);
            db.Matter.Remove(matter);
            await db.SaveChangesAsync();
            cache.Clear();
            return RedirectToAction("Index");
        }
    }
}
