namespace Libiada.Web.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    using Bio.Extensions;

    using Libiada.Web.Helpers;
    using Libiada.Database.Models.Repositories.Sequences;

    using Newtonsoft.Json;


    /// <summary>
    /// 
    /// </summary>
    public class MultisequenceController : Controller
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;

        public MultisequenceController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
        }

        /// <summary>
        /// Gets page with list of all multisequences.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<Multisequence> multisequences = db.Multisequence.Include(ms => ms.Matters).ToList();
            return View(multisequences);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            var data = viewDataHelper.FillViewData(2, int.MaxValue, m => MultisequenceRepository.SequenceTypesFilter.Contains(m.SequenceType) && m.MultisequenceId == null, "Create");
            ViewBag.data = JsonConvert.SerializeObject(data);

            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(//[Bind(Include = "Name,Nature")] 
            Multisequence multisequence,
            short[] multisequenceNumbers,
            long[] matterIds)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Model state is invalid");
            }

            db.Multisequence.Add(multisequence);
            db.SaveChanges();

            var matters = db.Matter.Where(m => matterIds.Contains(m.Id))
                                   .ToDictionary(m => m.Id, m => m);
            for (int i = 0; i < matterIds.Length; i++)
            {
                var matter = matters[matterIds[i]];
                matter.MultisequenceId = multisequence.Id;
                matter.MultisequenceNumber = multisequenceNumbers[i];
                db.Entry(matter).State = EntityState.Modified;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
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

            Multisequence? multisequence = db.Multisequence.Include(m => m.Matters)
                                                          .SingleOrDefault(m => m.Id == id);
            if (multisequence == null)
            {
                return NotFound();
            }

            return View(multisequence);

        }


        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Multisequence? multisequence = db.Multisequence.Include(m => m.Matters)
                                                          .SingleOrDefault(m => m.Id == id);
            if (multisequence == null)
            {
                return NotFound();
            }

            var selectedMatterIds = multisequence.Matters.Select(m => m.Id);
            var data = viewDataHelper.FillViewData(2,
                                                   int.MaxValue,
                                                   m => (MultisequenceRepository.SequenceTypesFilter.Contains(m.SequenceType)
                                                        && m.MultisequenceId == null)
                                                        || selectedMatterIds.Contains(m.Id),
                                                   m => selectedMatterIds.Contains(m.Id),
                                                   "Save");
            data.Add("multisequenceNumbers", multisequence.Matters.Select(m => new { m.Id, m.MultisequenceNumber }));
            ViewBag.data = JsonConvert.SerializeObject(data);

            return View(multisequence);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(//[Bind(Include = "Id,Name,Nature")]
                                             Multisequence multisequence,
                                             short[] multisequenceNumbers,
                                             long[] matterIds)
        {
            if (ModelState.IsValid)
            {
                db.Entry(multisequence).State = EntityState.Modified;

                var mattersToRemove = db.Matter.Where(m => m.MultisequenceId == multisequence.Id && !matterIds.Contains(m.Id)).ToArray();
                for (int i = 0; i < mattersToRemove.Length; i++)
                {
                    var matter = mattersToRemove[i];
                    matter.MultisequenceId = null;
                    matter.MultisequenceNumber = null;
                    db.Entry(matter).State = EntityState.Modified;
                }

                var mattersToAddOrUpdate = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m);
                for (int i = 0; i < matterIds.Length; i++)
                {
                    var matter = mattersToAddOrUpdate[matterIds[i]];
                    matter.MultisequenceId = multisequence.Id;
                    matter.MultisequenceNumber = multisequenceNumbers[i];
                    db.Entry(matter).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var sellectedMatterIds = multisequence.Matters.Select(m => m.Id);
            var data = viewDataHelper.FillViewData(2,
                                                   int.MaxValue,
                                                   m => (MultisequenceRepository.SequenceTypesFilter.Contains(m.SequenceType)
                                                        && m.MultisequenceId == null)
                                                        || sellectedMatterIds.Contains(m.Id),
                                                   m => sellectedMatterIds.Contains(m.Id),
                                                   "Create");

            ViewBag.data = JsonConvert.SerializeObject(data);

            return View(multisequence);

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

            Multisequence multisequence = await db.Multisequence.Include(m => m.Matters).SingleOrDefaultAsync(m => m.Id == id);
            if (multisequence == null)
            {
                return NotFound();
            }

            return View(multisequence);

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
            Multisequence multisequence = await db.Multisequence.Include(m => m.Matters).SingleAsync(m => m.Id == id);
            var matters = multisequence.Matters.ToArray();
            foreach (var matter in matters)
            {
                matter.MultisequenceId = null;
                matter.MultisequenceNumber = null;
                db.Entry(matter).State = EntityState.Modified;
            }

            db.Multisequence.Remove(multisequence);
            await db.SaveChangesAsync();
            Cache.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Divides matters into reference and not reference and groups them.
        /// </summary>
        /// <param name="matters">
        /// List of matters.
        /// </param>
        /// <returns>
        /// Returns grouped matters.
        /// </returns>
        [NonAction]
        private Dictionary<string, long[]> SplitMattersIntoReferenceAnsNotReference(Matter[] matters)
        {
            matters = matters.Where(m => m.Nature == Nature.Genetic && MultisequenceRepository.SequenceTypesFilter.Contains(m.SequenceType)).ToArray();
            var matterNameSpliters = new[] { "|", "chromosome", "plasmid", "segment" };
            var mattersNames = matters.Select(m => (m.Id, m.Name.Split(matterNameSpliters, StringSplitOptions.RemoveEmptyEntries)[0].Trim())).ToArray();
            var accessions = new string[matters.Length];
            var referenceArray = new List<(long, string)>(matters.Length / 2);
            var notReferenceArray = new List<(long, string)>(matters.Length / 2);
            for (int i = 0; i < matters.Length; i++)
            {
                Matter matter = matters[i];
                if (matter.Name.IndexOf('|') == -1)
                {
                    throw new Exception();
                }

                accessions[i] = matter.Name.Split('|').Last().Trim();
                if (accessions[i].IndexOf('_') != -1)
                {
                    referenceArray.Add(mattersNames[i]);
                }
                else
                {
                    notReferenceArray.Add(mattersNames[i]);
                }
            }

            Dictionary<string, long[]> multisequencesRefMatters = referenceArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key + " ref", mn => mn.Select(m => m.Item1).ToArray());

            Dictionary<string, long[]> multisequencesNotRefMatters = notReferenceArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key, mn => mn.Select(m => m.Item1).ToArray());

            return multisequencesRefMatters.Concat(multisequencesNotRefMatters)
                                           .ToDictionary(x => x.Key, y => y.Value);

        }

        public ActionResult Group()
        {
            return View();
        }

        /// <summary>
        /// Gets genetic matters names and ids from database.
        /// </summary>
        /// <param name="excludeMatterIds"></param>
        /// <returns>
        /// Returns multisequences with sequences included in them.
        /// </returns>
        public string GroupMattersIntoMultisequences(long[] excludeMatterIds)
        {
            Matter[] matters = db.Matter.Where(m => MultisequenceRepository.SequenceTypesFilter.Contains(m.SequenceType)).ToArray();
            Dictionary<string, long[]> multisequences = SplitMattersIntoReferenceAnsNotReference(matters);
            var result = multisequences.Select(m => new { name = m.Key, matterIds = m.Value }).ToArray();
            var matterIds = result.SelectMany(r => r.matterIds);
            var mattersDictionary = matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
            var groupingResult = new Dictionary<string, object>
                {
                    {"result", result},
                    { "matters", mattersDictionary},
                    { "ungroupedMatters", db.Matter
                                            .Where(m => m.Nature == Nature.Genetic && !matterIds.Contains(m.Id))
                                            .Select(m => new { m.Id, m.Name })
                                            .ToArray() }
                };

            var data = JsonConvert.SerializeObject(groupingResult);

            return data;
        }

        /// <summary>
        /// Writes multisequences data into database.
        /// </summary>
        /// <param name="multisequenceMatters">
        /// Dictionary of multisequences with matters.
        /// </param>
        /// <param name="multisequencesNames">
        /// Multisequence names list.
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GroupMattersIntoMultisequences(Dictionary<string, long[]> multisequenceMatters)
        {
            var multisequencesNames = multisequenceMatters.Keys.ToArray();
            Multisequence[] multisequences = new Multisequence[multisequencesNames.Length];

            for (int i = 0; i < multisequencesNames.Length; i++)
            {
                multisequences[i] = new Multisequence
                {
                    Name = multisequencesNames[i],
                    Nature = Nature.Genetic
                };
            }

            db.Multisequence.AddRange(multisequences);
            db.SaveChanges();

            var matters = db.Matter.Where(mt => mt.Nature == Nature.Genetic).ToDictionary(m => m.Id, m => m);
            foreach (Multisequence multisequence in multisequences)
            {
                try
                {
                    var matterIds = multisequenceMatters[multisequence.Name];
                    foreach (var matterId in matterIds)
                    {
                        db.Entry(matters[matterId]).State = EntityState.Modified;
                        matters[matterId].MultisequenceId = multisequence.Id;
                    }

                    MultisequenceRepository.SetSequenceNumbers(matterIds.Select(m => matters[m]).ToArray());
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                }
            }

            return RedirectToAction("Index");
        }
    }
}
