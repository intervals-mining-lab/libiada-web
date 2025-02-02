namespace Libiada.Web.Controllers.Sequences;

using Bio.Extensions;
using Libiada.Database.Models.Repositories.Sequences;
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
    private readonly ResearchObjectsCache cache;

    public MultisequenceController(LibiadaDatabaseEntities db, IViewDataHelper viewDataHelper, ResearchObjectsCache cache)
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
        List<Multisequence> multisequences = db.Multisequences.Include(ms => ms.ResearchObjects).ToList();
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
    public ActionResult Create(
        Multisequence multisequence,
        short[] multisequenceNumbers,
        long[] researchObjectIds)
    {
        if (!ModelState.IsValid)
        {
            throw new Exception("Model state is invalid");
        }

        db.Multisequences.Add(multisequence);
        db.SaveChanges();

        var researchObjects = db.ResearchObjects.Where(m => researchObjectIds.Contains(m.Id))
                               .ToDictionary(m => m.Id, m => m);
        for (int i = 0; i < researchObjectIds.Length; i++)
        {
            ResearchObject researchObject = researchObjects[researchObjectIds[i]];
            researchObject.MultisequenceId = multisequence.Id;
            researchObject.MultisequenceNumber = multisequenceNumbers[i];
            db.Entry(researchObject).State = EntityState.Modified;
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

        Multisequence? multisequence = db.Multisequences.Include(m => m.ResearchObjects)
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

        Multisequence? multisequence = db.Multisequences.Include(m => m.ResearchObjects)
                                                      .SingleOrDefault(m => m.Id == id);
        if (multisequence == null)
        {
            return NotFound();
        }

        var selectedResearchObjectIds = multisequence.ResearchObjects.Select(m => m.Id);
        var data = viewDataHelper.FillViewData(2,
                                               int.MaxValue,
                                               m => (SequenceTypesFilter.Contains(m.SequenceType)
                                                    && m.MultisequenceId == null)
                                                    || selectedResearchObjectIds.Contains(m.Id),
                                               m => selectedResearchObjectIds.Contains(m.Id),
                                               "Save");
        data.Add("multisequenceNumbers", multisequence.ResearchObjects.Select(m => new { m.Id, m.MultisequenceNumber }));
        ViewBag.data = JsonConvert.SerializeObject(data);

        return View(multisequence);

    }

    [HttpPost]
    public async Task<ActionResult> Edit(
                                         Multisequence multisequence,
                                         short[] multisequenceNumbers,
                                         long[] researchObjectIds)
    {
        if (ModelState.IsValid)
        {
            db.Entry(multisequence).State = EntityState.Modified;

            ResearchObject[] researchObjectsToRemove = db.ResearchObjects.Where(m => m.MultisequenceId == multisequence.Id && !researchObjectIds.Contains(m.Id)).ToArray();
            for (int i = 0; i < researchObjectsToRemove.Length; i++)
            {
                ResearchObject researchObject = researchObjectsToRemove[i];
                researchObject.MultisequenceId = null;
                researchObject.MultisequenceNumber = null;
                db.Entry(researchObject).State = EntityState.Modified;
            }

            var researchObjectsToAddOrUpdate = db.ResearchObjects.Where(m => researchObjectIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m);
            for (int i = 0; i < researchObjectIds.Length; i++)
            {
                ResearchObject researchObject = researchObjectsToAddOrUpdate[researchObjectIds[i]];
                researchObject.MultisequenceId = multisequence.Id;
                researchObject.MultisequenceNumber = multisequenceNumbers[i];
                db.Entry(researchObject).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        var sellectedResearchObjectIds = multisequence.ResearchObjects.Select(m => m.Id);
        var data = viewDataHelper.FillViewData(2,
                                               int.MaxValue,
                                               m => (SequenceTypesFilter.Contains(m.SequenceType)
                                                    && m.MultisequenceId == null)
                                                    || sellectedResearchObjectIds.Contains(m.Id),
                                               m => sellectedResearchObjectIds.Contains(m.Id),
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

        Multisequence? multisequence = await db.Multisequences.Include(m => m.ResearchObjects).SingleOrDefaultAsync(m => m.Id == id);
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
    public async Task<ActionResult> DeleteConfirmed(long id)
    {
        Multisequence multisequence = await db.Multisequences.Include(m => m.ResearchObjects).SingleAsync(m => m.Id == id);
        ResearchObject[] researchObjects = multisequence.ResearchObjects.ToArray();
        foreach (ResearchObject researchObject in researchObjects)
        {
            researchObject.MultisequenceId = null;
            researchObject.MultisequenceNumber = null;
            db.Entry(researchObject).State = EntityState.Modified;
        }

        db.Multisequences.Remove(multisequence);
        await db.SaveChangesAsync();
        cache.Clear();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Divides research objects into reference and not reference and groups them.
    /// </summary>
    /// <param name="researchObjects">
    /// List of research objects.
    /// </param>
    /// <returns>
    /// Returns grouped research objects.
    /// </returns>
    [NonAction]
    private Dictionary<string, long[]> SplitResearchObjectsIntoReferenceAnsNotReference(ResearchObject[] researchObjects)
    {
        researchObjects = researchObjects.Where(m => m.Nature == Nature.Genetic && SequenceTypesFilter.Contains(m.SequenceType)).ToArray();
        string[] researchObjectNameSpliters = ["|", "chromosome", "plasmid", "segment"];
        var researchObjectsNames = researchObjects.Select(m => (m.Id, m.Name.Split(researchObjectNameSpliters, StringSplitOptions.RemoveEmptyEntries)[0].Trim())).ToArray();
        string[] accessions = new string[researchObjects.Length];
        List<(long, string)> referenceArray = new(researchObjects.Length / 2);
        List<(long, string)> notReferenceArray = new(researchObjects.Length / 2);
        for (int i = 0; i < researchObjects.Length; i++)
        {
            ResearchObject researchObject = researchObjects[i];
            if (!researchObject.Name.Contains('|'))
            {
                throw new Exception();
            }

            accessions[i] = researchObject.Name.Split('|').Last().Trim();
            if (accessions[i].Contains('_'))
            {
                referenceArray.Add(researchObjectsNames[i]);
            }
            else
            {
                notReferenceArray.Add(researchObjectsNames[i]);
            }
        }

        Dictionary<string, long[]> multisequencesRefResearchObjects = referenceArray.GroupBy(mn => mn.Item2)
            .ToDictionary(mn => $"{mn.Key} ref", mn => mn.Select(m => m.Item1).ToArray());

        Dictionary<string, long[]> multisequencesNotRefResearchObjects = notReferenceArray.GroupBy(mn => mn.Item2)
            .ToDictionary(mn => mn.Key, mn => mn.Select(m => m.Item1).ToArray());

        return multisequencesRefResearchObjects.Concat(multisequencesNotRefResearchObjects)
                                               .ToDictionary(x => x.Key, y => y.Value);

    }

    public ActionResult Group()
    {
        return View();
    }

    /// <summary>
    /// Gets genetic research objects names and ids from database.
    /// </summary>
    /// <param name="excludeResearchObjectIds"></param>
    /// <returns>
    /// Returns multisequences with sequences included in them.
    /// </returns>
    public string GroupResearchObjectsIntoMultisequences(long[] excludeResearchObjectIds)
    {
        ResearchObject[] researchObjects = db.ResearchObjects.Where(m => SequenceTypesFilter.Contains(m.SequenceType)).ToArray();
        Dictionary<string, long[]> multisequences = SplitResearchObjectsIntoReferenceAnsNotReference(researchObjects);
        var result = multisequences.Select(m => new { name = m.Key, researchObjectIds = m.Value }).ToArray();
        var researchObjectIds = result.SelectMany(r => r.researchObjectIds);
        var researchObjectsDictionary = researchObjects.Where(m => researchObjectIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
        var groupingResult = new Dictionary<string, object>
            {
                {"result", result},
                { "researchObjects", researchObjectsDictionary},
                { "ungroupedResearchObjects", db.ResearchObjects
                                        .Where(m => m.Nature == Nature.Genetic && !researchObjectIds.Contains(m.Id))
                                        .Select(m => new { m.Id, m.Name })
                                        .ToArray() }
            };

        string data = JsonConvert.SerializeObject(groupingResult);

        return data;
    }

    /// <summary>
    /// Writes multisequences data into database.
    /// </summary>
    /// <param name="multisequenceResearchObjects">
    /// Dictionary of multisequences with research objects.
    /// </param>
    /// <param name="multisequencesNames">
    /// Multisequence names list.
    /// </param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult GroupResearchObjectsIntoMultisequences(Dictionary<string, long[]> multisequenceResearchObjects)
    {
        string[] multisequencesNames = multisequenceResearchObjects.Keys.ToArray();
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

        var researchObjects = db.ResearchObjects.Where(mt => mt.Nature == Nature.Genetic).ToDictionary(m => m.Id, m => m);
        foreach (Multisequence multisequence in multisequences)
        {
            try
            {
                long[] researchObjectIds = multisequenceResearchObjects[multisequence.Name];
                foreach (long researchObjectId in researchObjectIds)
                {
                    db.Entry(researchObjects[researchObjectId]).State = EntityState.Modified;
                    researchObjects[researchObjectId].MultisequenceId = multisequence.Id;
                }

                SetSequenceNumbers(researchObjectIds.Select(m => researchObjects[m]).ToArray());
                db.SaveChanges();
            }
            catch (Exception e)
            {
            }
        }

        return RedirectToAction("Index");
    }
}
