namespace Libiada.Web.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Bio;
    using Bio.Core.Extensions;
    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;

    using Libiada.Database;
    using Libiada.Database.Helpers;
    using Libiada.Database.Models;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Authorization;
    using Libiada.Web.Tasks;

    [Authorize(Roles = "Admin")]
    public class BatchGeneticImportFromGenBankSearchFileController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly INcbiHelper ncbiHelper;
        private readonly Cache cache;

        public BatchGeneticImportFromGenBankSearchFileController(LibiadaDatabaseEntities db,
                                                                 ITaskManager taskManager,
                                                                 INcbiHelper ncbiHelper,
                                                                 Cache cache)
            : base(TaskType.BatchGeneticImportFromGenBankSearchFile, taskManager)
        {
            this.db = db;
            this.ncbiHelper = ncbiHelper;
            this.cache = cache;
        }

        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(string.Empty);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            bool importGenes,
            bool importPartial,
            bool filterMinLength,
            int minLength,
            bool filterMaxLength,
            int maxLength,
            IFormFileCollection files)
        {
            return CreateTask(() =>
            {
                using var genBankSearchResultsStream = Helpers.FileHelper.GetFileStream(files[0]);
                string searchResults = FileHelper.ReadSequenceFromStream(genBankSearchResultsStream);
                string[] accessions;
                if (filterMinLength)
                {
                    accessions = filterMaxLength ?
                        NcbiHelper.GetIdsFromNcbiSearchResults(searchResults, importPartial, minLength, maxLength) :
                        NcbiHelper.GetIdsFromNcbiSearchResults(searchResults, importPartial, minLength);
                }
                else
                {
                    accessions = filterMaxLength ?
                        NcbiHelper.GetIdsFromNcbiSearchResults(searchResults, importPartial, 1, maxLength) :
                        NcbiHelper.GetIdsFromNcbiSearchResults(searchResults, importPartial);
                }
                accessions = accessions.Distinct().Select(a => a.Split('.')[0]).ToArray();
                var importResults = new List<MatterImportResult>(accessions.Length);

                var matterRepository = new MatterRepository(db, cache);
                var dnaSequenceRepository = new GeneticSequenceRepository(db, cache);

                var (existingAccessions, accessionsToImport) = dnaSequenceRepository.SplitAccessionsIntoExistingAndNotImported(accessions);

                importResults.AddRange(existingAccessions.ConvertAll(existingAccession => new MatterImportResult
                {
                    MatterName = existingAccession,
                    Result = "Sequence already exists",
                    Status = "Exists"
                }));

                foreach (string accession in accessionsToImport)
                {
                    var importResult = new MatterImportResult() { MatterName = accession };

                    try
                    {
                        ISequence bioSequence = ncbiHelper.DownloadGenBankSequence(accession);
                        GenBankMetadata metadata = NcbiHelper.GetMetadata(bioSequence);
                        importResult.MatterName = metadata.Version.CompoundAccession;

                        Matter matter = matterRepository.CreateMatterFromGenBankMetadata(metadata);

                        importResult.SequenceType = matter.SequenceType.GetDisplayValue();
                        importResult.Group = matter.Group.GetDisplayValue();
                        importResult.MatterName = matter.Name;
                        importResult.AllNames = $"Common name = {metadata.Source.CommonName}, "
                                        + $"Species = {metadata.Source.Organism.Species}, "
                                        + $"Definition = {metadata.Definition}, "
                                        + $"Saved matter name = {importResult.MatterName}";

                        var sequence = new CommonSequence
                        {
                            Matter = matter,
                            Notation = Notation.Nucleotides,
                            RemoteDb = RemoteDb.GenBank,
                            RemoteId = metadata.Version.CompoundAccession
                        };
                        bool partial = metadata.Definition.ToLower().Contains("partial");
                        dnaSequenceRepository.Create(sequence, bioSequence, partial);

                        (importResult.Result, importResult.Status) = importGenes ?
                                                         ImportFeatures(metadata, sequence) :
                                                         ("Successfully imported sequence", "Success");
                    }
                    catch (Exception exception)
                    {
                        importResult.Status = "Error";
                        importResult.Result = $"Error: {exception.Message}";
                        while (exception.InnerException != null)
                        {
                            exception = exception.InnerException;
                            importResult.Result += $" {exception.Message}";
                        }

                        foreach (var dbEntityEntry in db.ChangeTracker.Entries())
                        {
                            if (dbEntityEntry.Entity != null)
                            {
                                dbEntityEntry.State = EntityState.Detached;
                            }
                        }
                    }
                    finally
                    {
                        importResults.Add(importResult);
                    }
                }

                string[] names = importResults.Select(r => r.MatterName).ToArray();

                // removing matters for which adding of sequence failed
                Matter[] orphanMatters = db.Matters
                                           .Include(m => m.Sequence)
                                           .Where(m => names.Contains(m.Name) && m.Sequence.Count == 0)
                                           .ToArray();

                if (orphanMatters.Length > 0)
                {
                    db.Matters.RemoveRange(orphanMatters);
                    db.SaveChanges();
                }


                var result = new Dictionary<string, object> { { "result", importResults } };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
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
        [NonAction]
        private (string result, string status) ImportFeatures(GenBankMetadata metadata, CommonSequence sequence)
        {
            try
            {
                var subsequenceImporter = new SubsequenceImporter(db, metadata.Features.All, sequence.Id);
                var (featuresCount, nonCodingCount) = subsequenceImporter.CreateSubsequences();

                string result = $"Successfully imported sequence, {featuresCount} features "
                              + $"and {nonCodingCount} non-coding subsequences";

                return (result, "Success");

            }
            catch (Exception exception)
            {
                string result = $"Successfully imported sequence but failed to import genes: {exception.Message}";
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    result += $" {exception.Message}";
                }

                return (result, "Error");
            }
        }
    }
}