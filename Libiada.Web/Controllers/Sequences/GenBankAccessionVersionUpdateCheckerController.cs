namespace Libiada.Web.Controllers.Sequences
{
    using Libiada.Database.Helpers;
    using Libiada.Database.Models.NcbiSequencesData;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;
    using Libiada.Database.Models;

    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Libiada.Web.Tasks;

    public class GenBankAccessionVersionUpdateCheckerController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly Cache cache;

        public GenBankAccessionVersionUpdateCheckerController(LibiadaDatabaseEntities db,
                                                              ITaskManager taskManager,
                                                              Cache cache)
            : base(TaskType.GenBankAccessionVersionUpdateChecker, taskManager)
        {
            this.db = db;
            this.cache = cache;
        }

        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(string.Empty);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(bool reinmportSequences)
        {
            return CreateTask(() =>
            {
                if (reinmportSequences)
                {
                    throw new NotImplementedException();
                }

                Dictionary<string, AccessionUpdateSearchResult> sequencesData;

                var dnaSequenceRepository = new GeneticSequenceRepository(db, cache);

                var sequencesWithAccessions = db.DnaSequences
                                                .Include(ds => ds.Matter)
                                                .Where(ds => ds.Notation == Notation.Nucleotides && !string.IsNullOrEmpty(ds.RemoteId))
                                                .ToArray();

                sequencesData = sequencesWithAccessions
                                        .ToDictionary(s => s.RemoteId.Split('.')[0], s => new AccessionUpdateSearchResult()
                                        {
                                            LocalAccession = s.RemoteId,
                                            LocalVersion = Convert.ToByte(s.RemoteId.Split('?')[0].Split('.')[1]),
                                            Name = s.Matter.Name.Split('|')[0].Trim(),
                                            LocalUpdateDate = s.Matter.Modified.ToString(OutputFormats.DateFormat),
                                            LocalUpdateDateTime = s.Matter.Modified
                                        });


                List<NuccoreObject> searchResults = new List<NuccoreObject>();

                // slicing accessions into chunks to prevent "too long request" error
                string[] accessions = sequencesData.Keys.ToArray();
                const int maxChunkSize = 1000;

                for (int i = 0; i < accessions.Length; i += maxChunkSize)
                {
                    int actualChunkSize = Math.Min(maxChunkSize, accessions.Length - i);
                    var accessionsChunk = new string[actualChunkSize];
                    Array.Copy(accessions, i, accessionsChunk, 0, actualChunkSize);
                    (string ncbiWebEnvironment, string queryKey) = NcbiHelper.ExecuteEPostRequest(string.Join(",", accessionsChunk));
                    searchResults.AddRange(NcbiHelper.ExecuteESummaryRequest(ncbiWebEnvironment, queryKey, true));
                }

                for (int i = 0; i < searchResults.Count; i++)
                {
                    var searchResult = searchResults[i];
                    searchResult.Title = MatterRepository.TrimGenBankNameEnding(searchResult.Title);

                    var newAccession = searchResult.AccessionVersion.Split('.');
                    var sequenceData = sequencesData[newAccession[0]];
                    sequenceData.RemoteVersion = Convert.ToByte(newAccession[1]);
                    sequenceData.RemoteName = searchResult.Title;
                    sequenceData.RemoteOrganism = searchResult.Organism;
                    sequenceData.RemoteUpdateDate = searchResult.UpdateDate.ToString(OutputFormats.DateFormat);
                    sequenceData.Updated = sequenceData.LocalUpdateDateTime <= searchResult.UpdateDate || sequenceData.RemoteVersion > sequenceData.LocalVersion;
                    sequenceData.NameUpdated = !(sequenceData.Name.Contains(searchResult.Title) && sequenceData.Name.Contains(searchResult.Organism));
                }

                // TODO: add status for results table coloring
                var result = new Dictionary<string, object>
                {
                    {
                        "results", sequencesData.Values
                                                .OrderByDescending(r => r.RemoteVersion - r.LocalVersion)
                                                .ThenBy(r => r.Updated)
                                                .ThenBy(r => r.NameUpdated)
                    }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
