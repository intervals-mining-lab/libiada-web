namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The batch sequence import controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class BatchSequenceImportController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchSequenceImportController"/> class.
        /// </summary>
        public BatchSequenceImportController() : base("Batch sequences import")
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(string.Empty);
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="accessions">
        /// Ids of imported sequences in ncbi (remote ids).
        /// </param>
        /// <param name="importGenes">
        /// Flag indicating if genes import is needed.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string[] accessions, bool importGenes)
        {
            return Action(() =>
            {
                var matterNames = new string[accessions.Length];
                var savedMattersNames = new string[accessions.Length];
                var results = new string[accessions.Length];
                var statuses = new string[accessions.Length];

                using (var db = new LibiadaWebEntities())
                {
                    var matterRepository = new MatterRepository(db);
                    var existingAccessions = db.DnaSequence.Select(d => d.RemoteId).Distinct().ToArray();
                    var dnaSequenceRepository = new DnaSequenceRepository(db);
                    var bioSequences = NcbiHelper.GetGenBankSequences(accessions);

                    for (int i = 0; i < accessions.Length; i++)
                    {
                        string accession = accessions[i];
                        matterNames[i] = accession;
                        if (existingAccessions.Contains(accession) || existingAccessions.Contains(accession + ".1"))
                        {
                            results[i] = "Sequence already exists";
                            statuses[i] = "Exist";
                            continue;
                        }

                        try
                        {
                            var metadata = NcbiHelper.GetMetadata(bioSequences[i]);
                            if (existingAccessions.Contains(metadata.Version.CompoundAccession))
                            {
                                results[i] = "Sequence already exists";
                                statuses[i] = "Exist";
                                continue;
                            }

                            var matter = matterRepository.CreateMatterFromGenBankMetadata(metadata);

                            savedMattersNames[i] = matter.Name;
                            matterNames[i] = "Common name=" + metadata.Source.CommonName +
                                             ", Species=" + metadata.Source.Organism.Species +
                                             ", Definition=" + metadata.Definition +
                                             ", Saved matter name=" + savedMattersNames[i];

                            var sequence = new CommonSequence
                                               {
                                                   Matter = matter,
                                                   Notation = Notation.Nucleotides,
                                                   RemoteDb = RemoteDb.GenBank,
                                                   RemoteId = metadata.Version.CompoundAccession
                                               };

                            dnaSequenceRepository.Create(sequence, bioSequences[i], metadata.Definition.ToLower().Contains("partial"));
                            if (importGenes)
                            {
                                try
                                {
                                    using (var subsequenceImporter = new SubsequenceImporter(metadata.Features.All, sequence.Id))
                                    {
                                        subsequenceImporter.CreateSubsequences();
                                    }

                                    var nonCodingCount = db.Subsequence.Count(s => s.SequenceId == sequence.Id && s.FeatureId == Aliases.Feature.NonCodingSequence);
                                    var featuresCount = db.Subsequence.Count(s => s.SequenceId == sequence.Id && s.FeatureId != Aliases.Feature.NonCodingSequence);

                                    statuses[i] = "Success";
                                    results[i] = "Successfully imported  sequence and " + featuresCount + " features and " + nonCodingCount + " non coding subsequences";
                                }
                                catch (Exception exception)
                                {
                                    results[i] = "successfully imported sequence but failed to import genes: " + exception.Message;
                                    statuses[i] = "Error";

                                    if (exception.InnerException != null)
                                    {
                                        results[i] += " " + exception.InnerException.Message;
                                    }
                                }
                            }
                            else
                            {
                                results[i] = "successfully imported sequence";
                                statuses[i] = "Success";
                            }
                        }
                        catch (Exception exception)
                        {
                            results[i] = "Error:" + exception.Message + (exception.InnerException == null ? string.Empty : exception.InnerException.Message);
                            statuses[i] = "Error";
                        }
                    }

                    // removing matters for whitch adding of sequence failed
                    var orphanMatters = db.Matter.Include(m => m.Sequence).Where(m => savedMattersNames.Contains(m.Name) && m.Sequence.Count == 0).ToArray();

                    if (orphanMatters.Length > 0)
                    {
                        db.Matter.RemoveRange(orphanMatters);
                        db.SaveChanges();
                    }
                }

                return new Dictionary<string, object>
                           {
                               { "matterNames", matterNames },
                               { "results", results },
                               { "status", statuses }
                           };
            });
        }
    }
}
