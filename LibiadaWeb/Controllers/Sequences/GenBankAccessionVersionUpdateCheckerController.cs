namespace LibiadaWeb.Controllers.Sequences
{
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.NcbiSequencesData;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using LibiadaCore.Extensions;

    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using LibiadaWeb.Models;

    public class GenBankAccessionVersionUpdateCheckerController : AbstractResultController
    {
        public GenBankAccessionVersionUpdateCheckerController() : base(TaskType.GenBankAccessionVersionUpdateChecker)
        {
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

                Dictionary<string, AccessionUpdateSearchResult> sequencesData = new Dictionary<string, AccessionUpdateSearchResult>();
                using (var db = new LibiadaWebEntities())
                {
                    var dnaSequenceRepository = new GeneticSequenceRepository(db);

                    var sequencesWithAccessions = db.DnaSequence
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
                }

                List<NuccoreObject> searchResults = new List<NuccoreObject>();

                // slicing accessions into chunks to prevent "too long request" error
                string[] accessions = sequencesData.Keys.ToArray();
                const int maxChunkSize = 10000;

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
                    searchResult.Title = searchResult.Title.TrimEnd(".")
                                                        .TrimEnd(", complete genome")
                                                        .TrimEnd(", complete sequence")
                                                        .TrimEnd(", complete CDS")
                                                        .TrimEnd(", complete cds")
                                                        .TrimEnd(", genome");

                    var newAccession = searchResult.AccessionVersion.Split('.');
                    var sequenceData = sequencesData[newAccession[0]];
                    sequenceData.RemoteVersion = Convert.ToByte(newAccession[1]);
                    sequenceData.RemoteName = searchResult.Title;
                    sequenceData.RemoteOrganism = searchResult.Organism;
                    sequenceData.RemoteUpdateDate = searchResult.UpdateDate.ToString(OutputFormats.DateFormat);
                    sequenceData.Updated = sequenceData.LocalUpdateDateTime <= searchResult.UpdateDate || sequenceData.RemoteVersion > sequenceData.LocalVersion;
                    sequenceData.NameUpdated = !(sequenceData.Name.Contains(searchResult.Title) && sequenceData.Name.Contains(searchResult.Organism));
                }


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