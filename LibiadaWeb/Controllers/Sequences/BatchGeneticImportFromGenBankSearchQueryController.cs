﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Bio;
using Bio.Core.Extensions;
using Bio.IO.GenBank;
using LibiadaCore.Extensions;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.CalculatorsData;
using LibiadaWeb.Models.NcbiSequencesData;
using LibiadaWeb.Models.Repositories.Sequences;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Sequences
{
    [Authorize(Roles = "Admin")]
    public class BatchGeneticImportFromGenBankSearchQueryController : AbstractResultController
    {
        public BatchGeneticImportFromGenBankSearchQueryController() : base(TaskType.BatchGeneticImportFromGenBankSearchQuery)
        {
        }
        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(string.Empty);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            string searchQuery,
            bool importGenes,
            bool importPartial,
            bool filterMinLength,
            int minLength,
            bool filterMaxLength,
            int maxLength)
        {
            return CreateTask(() =>
            {
                string searchResults;
                string[] accessions;
                List<NuccoreObject> nuccoreObjects;

                if (filterMinLength)
                {
                    searchResults = filterMaxLength ?
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength, maxLength: maxLength) :
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength);
                }
                else
                {
                    searchResults = filterMaxLength ?
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength: 1, maxLength: maxLength) :
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery);
                }
                nuccoreObjects = NcbiHelper.ExecuteESummaryRequest(searchResults, importPartial);
                accessions = nuccoreObjects.Select(no => no.AccessionVersion.Split('.')[0]).Distinct().ToArray();
                var importResults = new List<MatterImportResult>(accessions.Length);

                using (var db = new LibiadaWebEntities())
                {
                    var matterRepository = new MatterRepository(db);
                    var dnaSequenceRepository = new GeneticSequenceRepository(db);

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
                            ISequence bioSequence = NcbiHelper.DownloadGenBankSequence(accession);
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
        private (string, string) ImportFeatures(GenBankMetadata metadata, CommonSequence sequence)
        {
            try
            {
                using (var subsequenceImporter = new SubsequenceImporter(metadata.Features.All, sequence.Id))
                {
                    var (featuresCount, nonCodingCount) = subsequenceImporter.CreateSubsequences();

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
                    result += $" {exception.Message}";
                }

                string status = "Error";
                return (result, status);
            }
        }

    }
}