
namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Accord.Math;

    using Bio.Extensions;

    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    public class SequenceConcatenatorController : AbstractResultController
    {
        /// <summary>
        /// 
        /// </summary>
        public SequenceConcatenatorController() : base(TaskType.SequenceConcatenator)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private readonly SequenceType[] sequenceTypeFilter = new SequenceType[]
        {
            SequenceType.ChloroplastGenome,
            SequenceType.CompleteGenome,
            SequenceType.MitochondrialPlasmid,
            SequenceType.MitochondrialGenome,
            SequenceType.Plasmid,
            SequenceType.Plastid
        };

        /// <summary>
        /// Divides matters into reference and not reference and groups them.
        /// </summary>
        /// <param name="matters">
        /// List of matters.
        /// </param>
        /// <returns>
        /// Returns grouped matters.
        /// </returns>
        private Dictionary<string, long[]> SplitMattersIntoReferenceAnsNotReference(List<Matter> matters)
        {

            matters = matters.Where(m => m.Nature == Nature.Genetic && sequenceTypeFilter.Contains(m.SequenceType)).ToList();
            var matterNameSpliters = new[] { "|", "chromosome", "plasmid", "segment" };
            var mattersNames = matters.Select(m => (m.Id, m.Name.Split(matterNameSpliters, StringSplitOptions.RemoveEmptyEntries)[0].Trim())).ToArray();
            var accessions = new List<string>();

            foreach (var matter in matters)
            {
                if (matter.Name.IndexOf('|') == -1)
                {
                    throw new Exception();
                }

                accessions.Add(matter.Name.Split('|').Last().Trim());
            }

            var referenceArray = new List<(long, string)>();
            var notReferenceArray = new List<(long, string)>();

            for (int i = 0; i < mattersNames.Length; i++)
            {
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

            var result = multisequencesRefMatters;
            foreach (var multisequencesNotRefMatter in multisequencesNotRefMatters)
            {
                result.Add(multisequencesNotRefMatter.Key, multisequencesNotRefMatter.Value);
            }

            return result;

        }

        // GET: SequenceConcatenator
        public ActionResult Index()
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
        [HttpPost]
        public ActionResult Index(long[] excludeMatterIds)
        {
            return CreateTask(() =>
            {
                using (var db = new LibiadaWebEntities())
                {
                    List<Matter> matters = db.Matter.Where(m => sequenceTypeFilter.Contains(m.SequenceType)).ToList();
                    var multisequences = SplitMattersIntoReferenceAnsNotReference(matters);
                    var result = multisequences.Select(m => new { name = m.Key, matterIds = m.Value }).ToArray();
                    var matterIds = result.SelectMany(r => r.matterIds);
                    matters = matters.Where(m => matterIds.Contains(m.Id)).ToList();
                    var groupingResult = new Dictionary<string, object>
                    {
                        {"result", result},
                        { "matters", matters.ToDictionary(m => m.Id, m => m.Name )},
                        { "ungroupedMatters", db.Matter
                                                .Where(m => m.Nature == Nature.Genetic && !matterIds.Contains(m.Id))
                                                .Select(m => new { m.Id, m.Name })
                                                .ToArray() }
                    };

                    var data = JsonConvert.SerializeObject(groupingResult);

                    return new Dictionary<string, string>
                    {
                        { "data",  data }
                    };
                }
            });
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
        public ActionResult Result(Dictionary<string, long[]> multisequenceMatters)
        {
            using (var db = new LibiadaWebEntities())
            {
                db.Database.ExecuteSqlCommand("UPDATE matter SET multisequence_id = NULL, multisequence_number = NULL");
                db.Database.ExecuteSqlCommand("DELETE FROM multisequence");
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

                var exceptionCases = new Dictionary<string, object>();
                long mId = 0;
                var matters = db.Matter.Where(mt => mt.Nature == Nature.Genetic).ToDictionary(m => m.Id, m => m);
                foreach (Multisequence multisquence in multisequences)
                {
                    try
                    {
                        var matterIds = multisequenceMatters[multisquence.Name];
                        foreach (var matterId in matterIds)
                        {
                            db.Entry(matters[matterId]).State = EntityState.Modified;
                            matters[matterId].MultisequenceId = multisquence.Id;
                            mId = matterId;
                        }
                        MultisequenceRepository.SetSequenceNumbers(matterIds.Select(m => matters[m]).ToArray());
                        var check = matterIds.Select(m => matters[m]);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        exceptionCases.Add(multisquence.Name, matters[mId]);
                    }
                }
            }

            return RedirectToAction("Index", "Multisequence");
        }
    }
}
