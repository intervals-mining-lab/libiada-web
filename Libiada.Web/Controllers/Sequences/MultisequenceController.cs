namespace Libiada.Web.Controllers.Sequences;

using Bio.Extensions;

using Libiada.Web.Helpers;

using Newtonsoft.Json;

using static Database.Models.Repositories.Sequences.MultisequenceRepository;

/// <summary>
/// 
/// </summary>
public class MultisequenceController : Controller
{
    private readonly LibiadaDatabaseEntities db;
    private readonly IViewDataHelper viewDataHelper;
    private readonly Cache cache;

    public MultisequenceController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper, Cache cache)
    {
        this.db = db;
        this.viewDataHelper = viewDataHelper;
        this.cache = cache;
    }

    /// <summary>
    /// Gets page with list of all multisequences.
    /// </summary>
    /// <returns></returns>
    public ActionResult Index()
    {
        List<Multisequence> multisequences = db.Multisequences.Include(ms => ms.Matters).ToList();
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
        var data = viewDataHelper.FillViewData(2, int.MaxValue, m => SequenceTypesFilter.Contains(m.SequenceType) && m.MultisequenceId == null, "Create");
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
    public ActionResult Create(
        Multisequence multisequence,
        short[] multisequenceNumbers,
        long[] matterIds)
    {
        if (!ModelState.IsValid)
        {
            throw new Exception("Model state is invalid");
        }

        db.Multisequences.Add(multisequence);
        db.SaveChanges();

        var matters = db.Matters.Where(m => matterIds.Contains(m.Id))
                               .ToDictionary(m => m.Id, m => m);
        for (int i = 0; i < matterIds.Length; i++)
        {
            Matter matter = matters[matterIds[i]];
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

        Multisequence? multisequence = db.Multisequences.Include(m => m.Matters)
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

        Multisequence? multisequence = db.Multisequences.Include(m => m.Matters)
                                                      .SingleOrDefault(m => m.Id == id);
        if (multisequence == null)
        {
            return NotFound();
        }

        var selectedMatterIds = multisequence.Matters.Select(m => m.Id);
        var data = viewDataHelper.FillViewData(2,
                                               int.MaxValue,
                                               m => (SequenceTypesFilter.Contains(m.SequenceType)
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
    public async Task<ActionResult> Edit(
                                         Multisequence multisequence,
                                         short[] multisequenceNumbers,
                                         long[] matterIds)
    {
        if (ModelState.IsValid)
        {
            db.Entry(multisequence).State = EntityState.Modified;

            Matter[] mattersToRemove = db.Matters.Where(m => m.MultisequenceId == multisequence.Id && !matterIds.Contains(m.Id)).ToArray();
            for (int i = 0; i < mattersToRemove.Length; i++)
            {
                Matter matter = mattersToRemove[i];
                matter.MultisequenceId = null;
                matter.MultisequenceNumber = null;
                db.Entry(matter).State = EntityState.Modified;
            }

            var mattersToAddOrUpdate = db.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m);
            for (int i = 0; i < matterIds.Length; i++)
            {
                Matter matter = mattersToAddOrUpdate[matterIds[i]];
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
                                               m => (SequenceTypesFilter.Contains(m.SequenceType)
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

        Multisequence? multisequence = await db.Multisequences.Include(m => m.Matters).SingleOrDefaultAsync(m => m.Id == id);
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
        Multisequence multisequence = await db.Multisequences.Include(m => m.Matters).SingleAsync(m => m.Id == id);
        Matter[] matters = multisequence.Matters.ToArray();
        foreach (Matter matter in matters)
        {
            matter.MultisequenceId = null;
            matter.MultisequenceNumber = null;
            db.Entry(matter).State = EntityState.Modified;
        }

        db.Multisequences.Remove(multisequence);
        await db.SaveChangesAsync();
        cache.Clear();
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
        matters = matters.Where(m => m.Nature == Nature.Genetic && SequenceTypesFilter.Contains(m.SequenceType)).ToArray();
        string[] matterNameSpliters = ["|", "chromosome", "plasmid", "segment"];
        var mattersNames = matters.Select(m => (m.Id, m.Name.Split(matterNameSpliters, StringSplitOptions.RemoveEmptyEntries)[0].Trim())).ToArray();
        string[] accessions = new string[matters.Length];
        List<(long, string)> referenceArray = new(matters.Length / 2);
        List<(long, string)> notReferenceArray = new(matters.Length / 2);
        for (int i = 0; i < matters.Length; i++)
        {
            Matter matter = matters[i];
            if (!matter.Name.Contains('|'))
            {
                throw new Exception();
            }

            accessions[i] = matter.Name.Split('|').Last().Trim();
            if (accessions[i].Contains('_'))
            {
                referenceArray.Add(mattersNames[i]);
            }
            else
            {
                notReferenceArray.Add(mattersNames[i]);
            }
        }

        Dictionary<string, long[]> multisequencesRefMatters = referenceArray.GroupBy(mn => mn.Item2)
            .ToDictionary(mn => $"{mn.Key} ref", mn => mn.Select(m => m.Item1).ToArray());

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
        Matter[] matters = db.Matters.Where(m => SequenceTypesFilter.Contains(m.SequenceType)).ToArray();
        Dictionary<string, long[]> multisequences = SplitMattersIntoReferenceAnsNotReference(matters);
        var result = multisequences.Select(m => new { name = m.Key, matterIds = m.Value }).ToArray();
        var matterIds = result.SelectMany(r => r.matterIds);
        var mattersDictionary = matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
        var groupingResult = new Dictionary<string, object>
            {
                {"result", result},
                { "matters", mattersDictionary},
                { "ungroupedMatters", db.Matters
                                        .Where(m => m.Nature == Nature.Genetic && !matterIds.Contains(m.Id))
                                        .Select(m => new { m.Id, m.Name })
                                        .ToArray() }
            };

        string data = JsonConvert.SerializeObject(groupingResult);

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
        string[] multisequencesNames = multisequenceMatters.Keys.ToArray();
        Multisequence[] multisequences = new Multisequence[multisequencesNames.Length];

        for (int i = 0; i < multisequencesNames.Length; i++)
        {
            multisequences[i] = new Multisequence
            {
                Name = multisequencesNames[i],
                Nature = Nature.Genetic
            };
        }

        db.Multisequences.AddRange(multisequences);
        db.SaveChanges();

        var matters = db.Matters.Where(mt => mt.Nature == Nature.Genetic).ToDictionary(m => m.Id, m => m);
        foreach (Multisequence multisequence in multisequences)
        {
            try
            {
                long[] matterIds = multisequenceMatters[multisequence.Name];
                foreach (long matterId in matterIds)
                {
                    db.Entry(matters[matterId]).State = EntityState.Modified;
                    matters[matterId].MultisequenceId = multisequence.Id;
                }

                SetSequenceNumbers(matterIds.Select(m => matters[m]).ToArray());
                db.SaveChanges();
            }
            catch (Exception e)
            {
            }
        }

        return RedirectToAction("Index");
    }
}
