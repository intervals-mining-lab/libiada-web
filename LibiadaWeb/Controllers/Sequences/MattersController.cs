﻿namespace LibiadaWeb.Controllers.Sequences
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
                List<Matter> matter = await db.Matter.Include(m => m.Multisequence).ToListAsync();

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
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                Matter matter = db.Matter.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
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
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new LibiadaWebEntities())
            {
                Matter matter = db.Matter.Include(m => m.Multisequence).SingleOrDefault(m => m.Id == id);
                if (matter == null)
                {
                    return HttpNotFound();
                }
                var data = new Dictionary<string, object>
                {
                    { "natures", EnumHelper.GetSelectList(typeof(Nature), matter.Nature) },
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
            [Bind(Include = "Id,Name,Nature,Description,Group,SequenceType,MultisequenceId,MultisequenceNumber,CollectionCountry,CollectionDate")] Matter matter)
        {
            using (var db = new LibiadaWebEntities())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(matter).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                var data = new Dictionary<string, object>
                {
                    { "natures", EnumHelper.GetSelectList(typeof(Nature), matter.Nature) },
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
                Cache.Clear();
                return RedirectToAction("Index");
            }
        }
    }
}
