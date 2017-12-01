namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Bio;
    using Bio.Extensions;
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
        /// Imports sequences from GenBank by their ids.
        /// Optionally imports all subsequences.
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
                accessions = accessions.Distinct().ToArray();
                var importResults = new List<MatterImportResult>(accessions.Length);

                using (var db = new LibiadaWebEntities())
                {
                    var matterRepository = new MatterRepository(db);
                    var dnaSequenceRepository = new GeneticSequenceRepository(db);

                    var(existingAccessions, accessionsToImport) = dnaSequenceRepository.SplitAccessionsIntoExistingAndNotImported(accessions);

                    importResults.AddRange(existingAccessions.ConvertAll(existingAccession => new MatterImportResult
                                                                                                  {
                                                                                                      MatterName = existingAccession,
                                                                                                      Result = "Sequence already exists",
                                                                                                      Status = "Exist"
                                                                                                  }));

                    ISequence[] bioSequences = NcbiHelper.GetGenBankSequences(accessionsToImport);
                    foreach (ISequence bioSequence in bioSequences)
                    {
                        var result = new MatterImportResult();

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

                            (result.Result, result.Status) = importGenes ?
                                                             ImportFeatures(metadata, sequence) :
                                                             ("Successfully imported sequence", "Success");
                        }
                        catch (Exception exception)
                        {
                            result.Status = "Error";
                            result.Result = $"Error: {exception.Message}";
                            while (exception.InnerException != null)
                            {
                                exception = exception.InnerException;
                                result.Result += $"<br/> {exception.Message}";
                            }
                        }
                        finally
                        {
                            importResults.Add(result);
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

                var data = new Dictionary<string, object> { { "result", importResults } };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(data) }
                           };
            });
        }

        /// <summary>
        /// Imports sequence features.
        /// </summary>
        /// <param name="metadata">
        /// The metadata.
        /// </param>
        /// <param name="sequence">
        /// The sequence.
        /// </param>
        /// <returns>
        /// Returns tuple where first element is import result text
        /// and second element is import status as  string.
        /// </returns>
        private (string, string) ImportFeatures(GenBankMetadata metadata, CommonSequence sequence)
        {
            try
            {
                using (var subsequenceImporter = new SubsequenceImporter(metadata.Features.All, sequence.Id))
                {
                    var(featuresCount, nonCodingCount) = subsequenceImporter.CreateSubsequences();

                    string result = $"Successfully imported sequence, {featuresCount} features "
                                  + $"and {nonCodingCount} non-coding subsequences";
                    string status = "Success";

                    return (result, status);
                }
            }
            catch (Exception exception)
            {
                string result = $"successfully imported sequence but failed to import genes: {exception.Message}";
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    result += $"<br/> {exception.Message}";
                }

                string status = "Error";
                return (result, status);
            }
        }
    }
}
