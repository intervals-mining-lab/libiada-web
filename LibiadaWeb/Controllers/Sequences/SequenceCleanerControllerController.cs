namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The sequence cleaner controller controller.
    /// </summary>
    public class SequenceCleanerControllerController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                var dnaSequenceRepository = new DnaSequenceRepository(db);
                var commonSequenceRepository = new CommonSequenceRepository(db);
                var elementRepository = new ElementRepository(db);
                var matterIds = new long[] { 1332, 1333, 1339, 1330, 1337, 1342, 1331, 1338, 1340, 1943, 1945, 1334 };
                var matters = db.Matter.Where(m => matterIds.Contains(m.Id)).OrderBy(m => m.Id).ToArray();
                var sequences = db.DnaSequence.Include(d => d.Matter).Where(d => matterIds.Contains(d.MatterId)).OrderBy(m => m.MatterId).ToArray();

                for (int i = 0; i < matters.Length; i++)
                {
                    var newMatter = new Matter
                                        {
                                            Name = matters[i].Name + " Cleaned of IS110",
                                            Description = matters[i].Description,
                                            Nature = matters[i].Nature,
                                            Group = matters[i].Group,
                                            SequenceType = matters[i].SequenceType
                                        };

                    var newSequence = new CommonSequence
                                          {
                                              Notation = sequences[i].Notation,
                                              Matter = newMatter,
                                              Description = sequences[i].Description,
                                              RemoteDb = sequences[i].RemoteDb,
                                              RemoteId = sequences[i].RemoteId
                                          };
                    var chain = commonSequenceRepository.ToLibiadaChain(sequences[i].Id);
                    dnaSequenceRepository.Create(newSequence, false, elementRepository.ToDbElements(chain.Alphabet, Notation.Nucleotides, false), chain.Building);
                    var sequenceId = sequences[i].Id;
                    var subsequences = db.Subsequence.Where(s => s.SequenceId == sequenceId).ToList();
                    var subsequenceIds = subsequences.Select(s => s.Id);
                    var subsequencesIdsToRemove = db.SequenceAttribute.Include(sa => sa.Subsequence).Where(sa => subsequenceIds.Contains(sa.SequenceId) && sa.Value.Contains("IS110"))
                        .Select(sa => sa.Subsequence.Id).ToArray();

                    subsequences.RemoveAll(s => subsequencesIdsToRemove.Contains(s.Id));

                    var newSubsequences = new Subsequence[subsequences.Count];
                    var newSequenceAttributes = new List<SequenceAttribute>();
                    for (int j = 0; j < subsequences.Count; j++)
                    {
                        newSubsequences[j] = new Subsequence
                                                 {
                                                     Id = DbHelper.GetNewElementId(db),
                                                     Feature = subsequences[j].Feature,
                                                     SequenceId = subsequences[j].SequenceId,
                                                     Start = subsequences[j].Start,
                                                     Length = subsequences[j].Length,
                                                     RemoteId = subsequences[j].RemoteId,
                                                     Partial = subsequences[j].Partial
                                                 };
                        var subsequenceAttributes = subsequences[j].SequenceAttribute.ToArray();
                        foreach (SequenceAttribute subsequenceAttribute in subsequenceAttributes)
                        {
                            newSequenceAttributes.Add(new SequenceAttribute
                                                          {
                                                              SequenceId = newSubsequences[j].Id,
                                                              Attribute = subsequenceAttribute.Attribute,
                                                              Value = subsequenceAttribute.Value
                                                          });
                        }
                    }

                    db.Subsequence.AddRange(newSubsequences);
                    db.SequenceAttribute.AddRange(newSequenceAttributes);
                    db.SaveChanges();
                }
            }

            return View();
        }
    }
}
