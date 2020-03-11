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
                    //var importResults = new List<MatterImportResult>(multisequences.Count);
                    //foreach (var multisequence in multisequences)
                    //{
                    //    var importResult = new MatterImportResult();
                    //    try
                    //    {
                    //        importResult.MatterName = multisequence.Name;
                    //        db.Multisequence.Add(multisequence);
                    //        db.SaveChanges();
                    //        importResult.Status = "Success";
                    //        importResult.Result =
                    //            $"Successfully created multisequence with {multisequence.Matters.Count} matters";
                    //    }
                    //    catch (Exception exception)
                    //    {
                    //        importResult.Status = "Error";
                    //        importResult.Result = $"Error: {exception.Message}";
                    //    }
                    //    finally
                    //    {
                    //        importResults.Add(importResult);
                    //    }
                    //}

                    //var result = db.Multisequence.Where(ms => multisequences.Select(m => m.Id).Contains(ms.Id))
                    //    .Include(ms => ms.Matters).ToList();
                    var result = multisequences.Select(m => new {name = m.Key, matterIds = m.Value}).ToArray();
                    var groupingResult = new Dictionary<string, object>
                    {
                        {"result", result},
                        { "matters", matters.ToDictionary(m => m.Id, m => new StringedMatter(m, 0)) }
                    };

                    return new Dictionary<string, object>
                    {
                        { "data", JsonConvert.SerializeObject(groupingResult) }
                    };
                }
            }); 
        }

        private Dictionary<string, long[]> GeneticMattersGenerator(List<Matter> matters)
        {
            matters = matters.Where(m => m.Nature == Nature.Genetic).ToList();
            var matterNameSpliters = new[] {"|", "chromosome", "plasmid", "segment"};
            var mattersNames = matters.Select(m => (m.Id, m.Name.Split(matterNameSpliters, StringSplitOptions.RemoveEmptyEntries)[0].Trim())).ToArray();
            var tempArray = new List<string>();

            foreach (var matter in matters)
            {
                if (!matter.Name.Contains("|"))
                {
                    throw new Exception();
                }

                tempArray.Add(matter.Name.Split('|').Last().Trim());
            }

            var refArray = new List<(long, string)>();
            var notRefArray = new List<(long, string)>();

            for (int i = 0; i < mattersNames.Length; i++)
            {
                if (tempArray[i].Contains('_'))
                {
                    refArray.Add(mattersNames[i]);
                }
                else
                {
                    notRefArray.Add(mattersNames[i]);
                }
            }

          



            //Dictionary<string, long[]> refMultisequenceMatterNames = refArray.GroupBy(mn => mn.)

            Dictionary<string, long[]> multisequencesRefMatters = refArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key, mn => mn.Select(m => m.Item1).ToArray());

            Dictionary<string, long[]> multisequencesNotRefMatters = notRefArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key + " not ref", mn => mn.Select(m => m.Item1).ToArray());
            var result = multisequencesRefMatters;
            foreach (var multisequencesNotRefMatter in multisequencesNotRefMatters)
            {
                result.Add(multisequencesNotRefMatter.Key, multisequencesNotRefMatter.Value);
            }
            // List<Multisequence> result = new List<Multisequence>(multisequencesMatters.Count);
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