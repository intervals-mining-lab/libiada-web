﻿namespace Libiada.Web.Controllers.Sequences;

using Libiada.Database.Helpers;
using Libiada.Database.Models.NcbiSequencesData;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;

public class GenBankAccessionVersionUpdateCheckerController : AbstractResultController
{
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly INcbiHelper ncbiHelper;
    private readonly IResearchObjectsCache cache;

    public GenBankAccessionVersionUpdateCheckerController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                                                          ITaskManager taskManager,
                                                          INcbiHelper ncbiHelper,
                                                          IResearchObjectsCache cache)
        : base(TaskType.GenBankAccessionVersionUpdateChecker, taskManager)
    {
        this.dbFactory = dbFactory;
        this.ncbiHelper = ncbiHelper;
        this.cache = cache;
    }

    public ActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Index(bool reinmportSequences)
    {
        return CreateTask(() =>
        {
            if (reinmportSequences)
            {
                throw new NotImplementedException();
            }

            Dictionary<string, AccessionUpdateSearchResult> sequencesData;

            using var geneticSequenceRepository = new GeneticSequenceRepository(dbFactory, cache);
            using var db = dbFactory.CreateDbContext();
            var sequencesWithAccessions = db.CombinedSequenceEntities
                                            .Include(ds => ds.ResearchObject)
                                            .Where(ds => ds.Notation == Notation.Nucleotides && !string.IsNullOrEmpty(ds.RemoteId))
                                            .ToArray();

            sequencesData = sequencesWithAccessions
                                    .ToDictionary(s => s.RemoteId!.Split('.')[0], s => new AccessionUpdateSearchResult()
                                    {
                                        LocalAccession = s.RemoteId!,
                                        LocalVersion = Convert.ToByte(s.RemoteId!.Split('?')[0].Split('.')[1]),
                                        Name = s.ResearchObject.Name.Split('|')[0].Trim(),
                                        LocalUpdateDate = s.ResearchObject.Modified.ToString(OutputFormats.DateFormat),
                                        LocalUpdateDateTime = s.ResearchObject.Modified
                                    });


            List<NuccoreObject> searchResults = [];

            // slicing accessions into chunks to prevent "too long request" error
            string[] accessions = sequencesData.Keys.ToArray();
            const int maxChunkSize = 1000;

            for (int i = 0; i < accessions.Length; i += maxChunkSize)
            {
                int actualChunkSize = System.Math.Min(maxChunkSize, accessions.Length - i);
                string[] accessionsChunk = new string[actualChunkSize];
                Array.Copy(accessions, i, accessionsChunk, 0, actualChunkSize);
                (string ncbiWebEnvironment, string queryKey) = ncbiHelper.ExecuteEPostRequest(string.Join(",", accessionsChunk));
                searchResults.AddRange(ncbiHelper.ExecuteESummaryRequest(ncbiWebEnvironment, queryKey, true));
            }

            for (int i = 0; i < searchResults.Count; i++)
            {
                NuccoreObject searchResult = searchResults[i];
                searchResult.Title = ResearchObjectRepository.TrimGenBankNameEnding(searchResult.Title);

                string[] newAccession = searchResult.AccessionVersion.Split('.');
                AccessionUpdateSearchResult sequenceData = sequencesData[newAccession[0]];
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
