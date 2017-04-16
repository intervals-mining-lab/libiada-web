namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Bio;
    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.CalculatorsData;
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
                var result = new MatterImportResult[accessions.Length];

                using (var db = new LibiadaWebEntities())
                {
                    var matterRepository = new MatterRepository(db);
                    var dnaSequenceRepository = new DnaSequenceRepository(db);
                    string[] existingAccessions = db.DnaSequence.Select(d => d.RemoteId).Distinct().ToArray();
                    ISequence[] bioSequences = NcbiHelper.GetGenBankSequences(accessions);

                    for (int i = 0; i < accessions.Length; i++)
                    {
                        result[i] = new MatterImportResult();
                        string accession = accessions[i];
                        result[i].MatterName = accession;
                        if (existingAccessions.Contains(accession) || existingAccessions.Contains(accession + ".1"))
                        {
                            result[i].Result = "Sequence already exists";
                            result[i].Status = "Exist";
                            continue;
                        }

                        try
                        {
                            GenBankMetadata metadata = NcbiHelper.GetMetadata(bioSequences[i]);
                            if (existingAccessions.Contains(metadata.Version.CompoundAccession))
                            {
                                result[i].Result = "Sequence already exists";
                                result[i].Status = "Exist";
                                continue;
                            }

                            Matter matter = matterRepository.CreateMatterFromGenBankMetadata(metadata);

                            result[i].SequenceType = matter.SequenceType.GetDisplayValue();
                            result[i].Group = matter.Group.GetDisplayValue();
                            result[i].MatterName = matter.Name;
                            result[i].AllNames = "Common name=" + metadata.Source.CommonName +
                                             ", Species=" + metadata.Source.Organism.Species +
                                             ", Definition=" + metadata.Definition +
                                             ", Saved matter name=" + result[i].MatterName;

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

                                    int nonCodingCount = db.Subsequence.Count(s => s.SequenceId == sequence.Id && s.Feature == Feature.NonCodingSequence);
                                    int featuresCount = db.Subsequence.Count(s => s.SequenceId == sequence.Id && s.Feature != Feature.NonCodingSequence);

                                    result[i].Result = "Successfully imported sequence and " + featuresCount + " features and " + nonCodingCount + " noncoding subsequences";
                                    result[i].Status = "Success";
                                }
                                catch (Exception exception)
                                {
                                    result[i].Result = "successfully imported sequence but failed to import genes: " + exception.Message;
                                    result[i].Status = "Error";

                                    if (exception.InnerException != null)
                                    {
                                        result[i].Result += " " + exception.InnerException.Message;
                                    }
                                }
                            }
                            else
                            {
                                result[i].Result = "successfully imported sequence";
                                result[i].Status = "Success";
                            }
                        }
                        catch (Exception exception)
                        {
                            result[i].Result = "Error:" + exception.Message + (exception.InnerException == null ? string.Empty : exception.InnerException.Message);
                            result[i].Status = "Error";
                        }
                    }

                    string[] names = result.Select(r => r.MatterName).ToArray();

                    // removing matters for which adding of sequence failed
                    Matter[] orphanMatters = db.Matter
                                               .Include(m => m.Sequence)
                                               .Where(m => names.Contains(m.Name) && m.Sequence.Count == 0)
                                               .ToArray();

                    if (orphanMatters.Length > 0)
                    {
                        db.Matter.RemoveRange(orphanMatters);
                        db.SaveChanges();
                    }
                }

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(new Dictionary<string, object>
                                                                         {
                                                                             { "result", result }
                                                                         })
                               }
                           };
            });
        }
    }
}
