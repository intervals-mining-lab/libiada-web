using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LibiadaWeb.Models.CalculatorsData;
using LibiadaWeb.Models.Repositories.Sequences;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Sequences
{
    public class SequenceConcatenatorController : AbstractResultController
    {
        // GET: SequenceConcatenator
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] excludeMatterIds)
        {
            return CreateTask(() =>
            {
                using (var db = new LibiadaWebEntities())
                {
                    List<Matter> matters = db.Matter.ToList();
                    var multisequences = GeneticMattersGenerator(matters);
                    var importResults = new List<MatterImportResult>(multisequences.Count);
                    foreach (var multisequence in multisequences)
                    {
                        var importResult = new MatterImportResult();
                        try
                        {
                            importResult.MatterName = multisequence.Name;
                            db.Multisequence.Add(multisequence);
                            db.SaveChanges();
                            importResult.Status = "Success";
                            importResult.Result =
                                $"Successfully created multisequence with {multisequence.Matters.Count} matters";
                        }
                        catch (Exception exception)
                        {
                            importResult.Status = "Error";
                            importResult.Result = $"Error: {exception.Message}";
                        }
                        finally
                        {
                            importResults.Add(importResult);
                        }
                    }

                    var result = db.Multisequence.Where(ms => multisequences.Select(m => m.Id).Contains(ms.Id))
                        .Include(ms => ms.Matters).ToList();

                    var groupingResult = new Dictionary<string, object> { {"result", importResults} };

                    return new Dictionary<string, object>
                    {
                        { "data", JsonConvert.SerializeObject(groupingResult) }
                    };
                }
            }); 
        }

        private List<Multisequence> GeneticMattersGenerator(List<Matter> matters)
        {
            matters = matters.Where(m => m.Nature == Nature.Genetic).ToList();
            var matterNameSpliters = new[] {"|", "chromosome", "plasmid", "segment"};
            var mattersNames = matters.Select(m => (m.Id, m.Name.Split(matterNameSpliters, StringSplitOptions.RemoveEmptyEntries)[0].Trim())).ToArray();
            var tempArray = new List<string>();

            foreach (var matter in matters)
            {
                if (matter.Name.Contains("|"))
                {
                    tempArray.Add(matter.Name.Split('|').Last().Trim());
                }
            }

            var tempRefArray = new List<string>();
            var tempNotRefArray = new List<string>();

            for (int i = 1; i <= mattersNames.Length; i++)
            {
                if (tempArray[i - 1].Contains('_'))
                {
                    tempRefArray.Add(mattersNames.GetValue(i).ToString());
                }
                else
                {
                    tempNotRefArray.Add(mattersNames.GetValue(i).ToString());
                }
            }

            var refArray = tempRefArray.Select(MultisequenceRepository.GetMatterNameSplit).ToList();
            var notRefArray = tempNotRefArray.Select(MultisequenceRepository.GetMatterNameSplit).ToList();



            //Dictionary<string, long[]> refMultisequenceMatterNames = refArray.GroupBy(mn => mn.)

            Dictionary<string, long[]> multisequencesMatters = mattersNames.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key, mn => mn.Select(m => m.Item1).ToArray());
            List<Multisequence> result = new List<Multisequence>(multisequencesMatters.Count);
           /* foreach (var (matterName, matterIds) in multisequencesMatters)
            {
                if (matterIds.Length > 1)
                {
                    var concatenatedMatters = matters.Where(m => matterIds.Contains(m.Id)).ToArray();
                    result.Add(new Multisequence()
                    {
                        Name = matterName,
                        Matters = concatenatedMatters,
                        Nature = Nature.Genetic
                    });
                }
            }
            */
            return result;
            
        }

        public SequenceConcatenatorController() : base(TaskType.SequenceConcatenator)
        {

        }
    }
}