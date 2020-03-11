namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The sequence cleaner controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequenceCleanerController : Controller
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
                var matterRepository = new MatterRepository(db);
                var dnaSequenceRepository = new GeneticSequenceRepository(db);
                var commonSequenceRepository = new CommonSequenceRepository(db);
                var elementRepository = new ElementRepository(db);
                var matterIds = new long[] { 1332, 1333, 1339, 1330, 1337, 1342, 1331, 1338, 1340, 1943, 1945, 1334 };
                DnaSequence[] sequences = db.DnaSequence.Include(d => d.Matter).Where(d => matterIds.Contains(d.MatterId)).ToArray();

                for (int i = 0; i < sequences.Length; i++)
                {
                    var newMatter = new Matter
                                        {
                                            Name = $"{sequences[i].Matter.Name} Cleaned of IS110",
                                            Description = sequences[i].Matter.Description,
                                            Nature = sequences[i].Matter.Nature,
                                            Group = sequences[i].Matter.Group,
                                            SequenceType = sequences[i].Matter.SequenceType
                                        };

                    var newSequence = new CommonSequence
                                          {
                                              Notation = sequences[i].Notation,
                                              Matter = newMatter,
                                              Description = sequences[i].Description,
                                              RemoteDb = sequences[i].RemoteDb,
                                              RemoteId = sequences[i].RemoteId
                                          };
                    var chain = commonSequenceRepository.GetLibiadaChain(sequences[i].Id);

                    matterRepository.CreateOrExtractExistingMatterForSequence(newSequence);
                    dnaSequenceRepository.Create(newSequence, false, elementRepository.ToDbElements(chain.Alphabet, Notation.Nucleotides, false), chain.Building);
                    var sequenceId = sequences[i].Id;
                    var subsequences = db.Subsequence.Include(s => s.Position).Include(s => s.SequenceAttribute).Where(s => s.SequenceId == sequenceId).ToList();
                    var subsequenceIds = subsequences.Select(s => s.Id);
                    var subsequencesIdsToRemove = db.SequenceAttribute
                        .Where(sa => subsequenceIds.Contains(sa.SequenceId) && sa.Value.Contains("IS110"))
                        .Select(sa => sa.SequenceId)
                        .Distinct()
                        .ToArray();

                    subsequences.RemoveAll(s => subsequencesIdsToRemove.Contains(s.Id));

                    var newSubsequences = new Subsequence[subsequences.Count];
                    var newSequenceAttributes = new List<SequenceAttribute>();
                    var newPositions = new List<Position>();
                    for (int j = 0; j < subsequences.Count; j++)
                    {
                        newSubsequences[j] = new Subsequence
                                                 {
                                                     Id = DbHelper.GetNewElementId(db),
                                                     Feature = subsequences[j].Feature,
                                                     SequenceId = newSequence.Id,
                                                     Start = subsequences[j].Start,
                                                     Length = subsequences[j].Length,
                                                     RemoteId = subsequences[j].RemoteId,
                                                     Partial = subsequences[j].Partial
                                                 };

                        foreach (SequenceAttribute subsequenceAttribute in subsequences[j].SequenceAttribute.ToArray())
                        {
                            newSequenceAttributes.Add(new SequenceAttribute
                                                          {
                                                              SequenceId = newSubsequences[j].Id,
                                                              Attribute = subsequenceAttribute.Attribute,
                                                              Value = subsequenceAttribute.Value
                                                          });
                        }

                        foreach (Position position in subsequences[j].Position.ToArray())
                        {
                            newPositions.Add(new Position
                            {
                                SubsequenceId = newSubsequences[j].Id,
                                Length = position.Length,
                                Start = position.Start
                            });
                        }
                    }

                    db.Subsequence.AddRange(newSubsequences);
                    db.SequenceAttribute.AddRange(newSequenceAttributes);
                    db.Position.AddRange(newPositions);
                    db.SaveChanges();
                }
            }

            return View();
        }
    }
}
