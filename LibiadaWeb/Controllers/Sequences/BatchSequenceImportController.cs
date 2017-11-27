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
    using LibiadaWeb.Tasks;

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
        public BatchSequenceImportController() : base(TaskType.BatchSequenceImport)
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
            return CreateTask(() =>
            {
                var importResults = new List<MatterImportResult>(accessions.Length);

                using (var db = new LibiadaWebEntities())
                {
                    var matterRepository = new MatterRepository(db);
                    var dnaSequenceRepository = new GeneticSequenceRepository(db);

                    var (existingAccesssions, accessionsToImport) = dnaSequenceRepository.SplitAccessionsIntoImportedAndNotImported(accessions);


                    foreach (string existingAccesssion in existingAccesssions)
                    {
                        var result = new MatterImportResult();
                        importResults.Add(result);
                        result.MatterName = existingAccesssion;
                        result.Result = "Sequence already exists";
                        result.Status = "Exist";
                    }

                    ISequence[] bioSequences = NcbiHelper.GetGenBankSequences(accessionsToImport);

                    foreach (ISequence bioSequence in bioSequences)
                    {
                        var result = new MatterImportResult();
                        importResults.Add(result);
                        try
                        {
                            GenBankMetadata metadata = NcbiHelper.GetMetadata(bioSequence);
                            result.MatterName = metadata.Version.CompoundAccession;

                            Matter matter = matterRepository.CreateMatterFromGenBankMetadata(metadata);

                            result.SequenceType = matter.SequenceType.GetDisplayValue();
                            result.Group = matter.Group.GetDisplayValue();
                            result.MatterName = matter.Name;
                            result.AllNames = $"Common name = {metadata.Source.CommonName}, "
                                            + $"Species = {metadata.Source.Organism.Species}, "
                                            + $"Definition = {metadata.Definition}, "
                                            + $"Saved matter name = {result.MatterName}";

                            var sequence = new CommonSequence
                                               {
                                                   Matter = matter,
                                                   Notation = Notation.Nucleotides,
                                                   RemoteDb = RemoteDb.GenBank,
                                                   RemoteId = metadata.Version.CompoundAccession
                                               };
                            bool partial = metadata.Definition.ToLower().Contains("partial");
                            dnaSequenceRepository.Create(sequence, bioSequence, partial);

                            if (importGenes)
                            {
                                ImportFeatures(metadata, sequence, result);
                            }
                            else
                            {
                                result.Result = "successfully imported sequence";
                                result.Status = "Success";
                            }
                        }
                        catch (Exception exception)
                        {
                            result.Result = "Error:" + exception.Message + (exception.InnerException?.Message ?? string.Empty);
                            result.Status = "Error";
                        }
                    }

                    string[] names = importResults.Select(r => r.MatterName).ToArray();

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
                                                                             { "result", importResults }
                                                                         })
                               }
                           };
            });
        }

        private void ImportFeatures(GenBankMetadata metadata, CommonSequence sequence, MatterImportResult result)
        {
            try
            {
                using (var subsequenceImporter = new SubsequenceImporter(metadata.Features.All, sequence.Id))
                {
                    (int featuresCount, int nonCodingCount) = subsequenceImporter.CreateSubsequences();

                    result.Result = $"Successfully imported sequence, "
                                    + $"{featuresCount} features "
                                    + $"and {nonCodingCount} non-coding subsequences";
                    result.Status = "Success";
                }

                //int featuresCount = db.Subsequence.Count(s => s.SequenceId == sequence.Id && s.Feature != Feature.NonCodingSequence);
                //int nonCodingCount = db.Subsequence.Count(s => s.SequenceId == sequence.Id && s.Feature == Feature.NonCodingSequence);
            }
            catch (Exception exception)
            {
                result.Result = $"successfully imported sequence but failed to import genes: {exception.Message}";
                result.Status = "Error";

                if (exception.InnerException != null)
                {
                    result.Result += " " + exception.InnerException.Message;
                }
            }
        }
    }
}
