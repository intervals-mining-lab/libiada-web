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

        /// <summary>
        /// Gets genetic matters names and ids from database.
        /// </summary>
        /// <param name="excludeMatterIds"></param>
        /// <returns>
        /// Returns multisequences with sequences included in them.
        /// </returns>
        [HttpPost]
        public ActionResult Index(long[] excludeMatterIds)
        {
            return CreateTask(() =>
            {
                using (var db = new LibiadaWebEntities())
                {
                    List<Matter> matters = db.Matter.ToList();
                    var multisequences = GeneticMattersGenerator(matters);
                    var result = multisequences.Select(m => new {name = m.Key, matterIds = m.Value}).ToArray();
                    var matterIds = result.SelectMany(r => r.matterIds);
                    matters = matters.Where(m => matterIds.Contains(m.Id)).ToList();
                    var groupingResult = new Dictionary<string, object>
                    {
                        {"result", result},
                        { "matters", matters.ToDictionary(m => m.Id, m => new StringedMatter(m, 0)) },
                        { "ungroupedMatters", db.Matter.Where(m => m.Nature == Nature.Genetic && !matterIds.Contains(m.Id)).ToArray() }
                    };

                    return new Dictionary<string, object>
                    {
                        { "data", JsonConvert.SerializeObject(groupingResult) }
                    };
                }
            }); 
        }

        /// <summary>
        /// Divides matters into reference and not reference and groups them.
        /// </summary>
        /// <param name="matters">
        /// List of matters.
        /// </param>
        /// <returns>
        /// Returns grouped matters.
        /// </returns>
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

            Dictionary<string, long[]> multisequencesRefMatters = refArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key, mn => mn.Select(m => m.Item1).ToArray());

            Dictionary<string, long[]> multisequencesNotRefMatters = notRefArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key + " not ref", mn => mn.Select(m => m.Item1).ToArray());
            var result = multisequencesRefMatters;
            foreach (var multisequencesNotRefMatter in multisequencesNotRefMatters)
            {
                result.Add(multisequencesNotRefMatter.Key, multisequencesNotRefMatter.Value);
            }
            return result;
            
        }

        public SequenceConcatenatorController() : base(TaskType.SequenceConcatenator)
        {

        }
    }
}