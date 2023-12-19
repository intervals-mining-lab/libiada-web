namespace Libiada.Web.Controllers.Sequences
{
    using Bio.Core.Extensions;

    using Libiada.Database.Helpers;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models.NcbiSequencesData;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Libiada.Web.Tasks;

    [Authorize(Roles = "Admin")]
    public class NcbiNuccoreSearchController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly INcbiHelper ncbiHelper;
        private readonly Cache cache;

        public NcbiNuccoreSearchController(LibiadaDatabaseEntities db,
                                           ITaskManager taskManager,
                                           INcbiHelper ncbiHelper,
                                           Cache cache)
            : base(TaskType.NcbiNuccoreSearch, taskManager)
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
            string searchQuery,
            bool importPartial,
            bool filterMinLength,
            int minLength,
            bool filterMaxLength,
            int maxLength)
        {
            return CreateTask(() =>
            {
                if (filterMinLength)
                {
                    searchQuery = filterMaxLength ?
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength, maxLength: maxLength) :
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength);
                }
                else
                {
                    searchQuery = filterMaxLength ?
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, maxLength: maxLength) :
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery);
                }

                List<NuccoreObject> searchResults = ncbiHelper.ExecuteESummaryRequest(searchQuery, importPartial);

                List<NuccoreObject> unfilteredSearchResults;
                List<NuccoreObject> filteresOutSearchResults = searchResults;
                string[] accessions;
                if (!importPartial)
                {
                    unfilteredSearchResults = ncbiHelper.ExecuteESummaryRequest(searchQuery, true);
                    filteresOutSearchResults = unfilteredSearchResults.Except(searchResults).ToList();
                    accessions = unfilteredSearchResults.Select(no => no.AccessionVersion.Split('.')[0]).Distinct().ToArray();
                }
                else
                {
                    accessions = searchResults.Select(no => no.AccessionVersion.Split('.')[0]).Distinct().ToArray();
                }

                var results = new List<MatterImportResult>(accessions.Length);

                string[] existingAccessions;

                var dnaSequenceRepository = new GeneticSequenceRepository(db, cache);

                (existingAccessions, _) = dnaSequenceRepository.SplitAccessionsIntoExistingAndNotImported(accessions);

                searchResults = searchResults
                                    .Where(sr => !existingAccessions.Contains(sr.AccessionVersion.Split('.')[0]))
                                    .ToList();
                foreach (var searchResult in searchResults)
                {
                    results.Add(new MatterImportResult()
                    {
                        MatterName = $"{searchResult.Title} | {searchResult.AccessionVersion}",
                        Result = "Found new sequence",
                        Status = "Success"
                    });
                }

                results.AddRange(existingAccessions.ConvertAll(existingAccession => new MatterImportResult
                {
                    MatterName = existingAccession,
                    Result = "Sequence already exists",
                    Status = "Exists"
                }));

                if (!importPartial)
                {
                    filteresOutSearchResults = filteresOutSearchResults
                                            .Where(sr => !existingAccessions.Contains(sr.AccessionVersion.Split('.')[0]))
                                            .ToList();
                    foreach (var filteresOutSearchResult in filteresOutSearchResults)
                    {
                        results.Add(new MatterImportResult()
                        {
                            MatterName = $"{filteresOutSearchResult.Title} | {filteresOutSearchResult.AccessionVersion}",
                            Result = "Filtered out",
                            Status = "Error"
                        });
                    }
                }

                accessions = searchResults.Select(sr => sr.AccessionVersion).ToArray();

                var result = new Dictionary<string, object> { { "result", results }, { "accessions", accessions } };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
