namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// The matters controller.
    /// </summary>
    [Authorize]
    public class MattersController : SequencesMattersController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MattersController"/> class.
        /// </summary>
        public MattersController() : base(TaskType.Matters)
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
                List<Matter> matter = await db.Matter.ToListAsync();

                if (!AccountHelper.IsAdmin())
                {
                    matter = matter.Where(m => m.Nature == Nature.Genetic).ToList();
                }

                return View(matter);
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
                Matter matter = await db.Matter.FindAsync(id);
                if (matter == null)
                {
                    return HttpNotFound();
                }

                ViewBag.SequencesCount = db.CommonSequence.Count(c => c.MatterId == matter.Id);

                return View(matter);
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
                Matter matter = await db.Matter.FindAsync(id);
                if (matter == null)
                {
                    return HttpNotFound();
                }

                var groups = EnumExtensions.ToArray<Group>().ToSelectListWithNature();
                var sequenceTypes = EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature();
                var natures = EnumHelper.GetSelectList(typeof(Nature), matter.Nature);
                var data = new Dictionary<string, object>
                               {
                                   { "natures", natures },
                                   { "groups", groups },
                                   { "sequenceTypes", sequenceTypes },
                                   { "matter", new StringedMatter(matter, db.CommonSequence.Count(c => c.MatterId == matter.Id)) }
                               };

                ViewBag.data = JsonConvert.SerializeObject(data);

                return View(matter);
            }
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Nature,Description,Group,SequenceType")] Matter matter)
        {
            using (var db = new LibiadaWebEntities())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(matter).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                var groups = EnumExtensions.ToArray<Group>().ToSelectListWithNature();
                var sequenceTypes = EnumExtensions.ToArray<SequenceType>().ToSelectListWithNature();
                var natures = EnumHelper.GetSelectList(typeof(Nature), matter.Nature);
                var data = new Dictionary<string, object>
                               {
                                   { "natures", natures },
                                   { "groups", groups },
                                   { "sequenceTypes", sequenceTypes },
                                   { "matter", new StringedMatter(matter, db.CommonSequence.Count(c => c.MatterId == matter.Id)) }
                               };

                ViewBag.data = JsonConvert.SerializeObject(data);
                return View(matter);
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
                Matter matter = await db.Matter.FindAsync(id);
                if (matter == null)
                {
                    return HttpNotFound();
                }

                ViewBag.SequencesCount = db.CommonSequence.Count(c => c.MatterId == matter.Id);
                return View(matter);
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
                Matter matter = await db.Matter.FindAsync(id);
                db.Matter.Remove(matter);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Stringed matter for drop down lists ids.
        /// </summary>
        private struct StringedMatter
        {
            /// <summary>
            /// The id.
            /// </summary>
            public readonly long Id;

            /// <summary>
            /// The name.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The description.
            /// </summary>
            public readonly string Description;

            /// <summary>
            /// The nature.
            /// </summary>
            public readonly string Nature;

            /// <summary>
            /// The group.
            /// </summary>
            public readonly string Group;

            /// <summary>
            /// The sequence type.
            /// </summary>
            public readonly string SequenceType;

            public readonly int SequencesCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="StringedMatter"/> struct
            /// from given matter instance.
            /// </summary>
            /// <param name="matter">
            /// The matter.
            /// </param>
            /// <param name="sequencesCount">
            /// Matter's sequences Count.
            /// </param>
            public StringedMatter(Matter matter, int sequencesCount)
            {
                Id = matter.Id;
                Name = matter.Name;
                Description = matter.Description;
                Nature = ((byte)matter.Nature).ToString();
                Group = ((byte)matter.Group).ToString();
                SequenceType = ((byte)matter.SequenceType).ToString();
                SequencesCount = sequencesCount;
            }
        }
    }
}
