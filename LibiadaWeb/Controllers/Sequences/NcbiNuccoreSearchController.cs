using Bio.Core.Extensions;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models.CalculatorsData;
using LibiadaWeb.Models.NcbiSequencesData;
using LibiadaWeb.Models.Repositories.Sequences;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Sequences
{
    public class NcbiNuccoreSearchController : AbstractResultController
    {
        public NcbiNuccoreSearchController() : base(TaskType.NcbiNuccoreSearch)
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
            bool importPartial,
            bool filterMinLength,
            int minLength,
            bool filterMaxLength,
            int maxLength)
        {
            return CreateTask(() =>
            {
                string[] accessions;
                List<NuccoreObject> searchResults;

                if (filterMinLength)
                {
                    searchQuery = filterMaxLength ?
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength, maxLength) :
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength);
                }
                else
                {
                    searchQuery = filterMaxLength ?
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery, minLength: 1, maxLength) :
                        NcbiHelper.FormatNcbiSearchTerm(searchQuery);
                }


                searchResults = NcbiHelper.SearchInNuccoreDb(searchQuery, importPartial);

                string unfilteredSearch = NcbiHelper.FormatNcbiSearchTerm(searchQuery);
                List<NuccoreObject> unfilteredSearchResults = NcbiHelper.SearchInNuccoreDb(unfilteredSearch, true);
                List<NuccoreObject> filteresOutSearchResults = unfilteredSearchResults.Except(searchResults).ToList();
                accessions = searchResults.Select(no => no.Accession.Split('.')[0]).Distinct().ToArray();
                var importResults = new List<MatterImportResult>(accessions.Length);

                using (var db = new LibiadaWebEntities())
                {
                    var dnaSequenceRepository = new GeneticSequenceRepository(db);

                    var (existingAccessions, accessionsToImport) = dnaSequenceRepository.SplitAccessionsIntoExistingAndNotImported(accessions);

                    importResults.AddRange(existingAccessions.ConvertAll(existingAccession => new MatterImportResult
                    {

                        MatterName = existingAccession,
                        Result = "Sequence already exists",
                        Status = "Exist"
                    }));

                    foreach (var searchResult in searchResults)
                    {
                        importResults.Add(new MatterImportResult()
                        {
                            MatterName = searchResult.Name + searchResult.Accession,
                            Status = "Success"
                        });
                    }

                    foreach (var filteresOutSearchResult in filteresOutSearchResults)
                    {
                        importResults.Add(new MatterImportResult()
                        {
                            MatterName = filteresOutSearchResult.Name + filteresOutSearchResult.Accession,
                            Result = "Filtered out",
                            Status = "Error"
                        });
                    }
                }

                var data = new Dictionary<string, object> { { "result", importResults } };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(data) }
                           };
            });
        }
    }
}
