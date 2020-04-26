﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using Accord.Math;
using Bio.Extensions;
using LibiadaWeb.Models.CalculatorsData;
using LibiadaWeb.Models.Repositories.Sequences;
using LibiadaWeb.Tasks;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Sequences
{
    public class SequenceConcatenatorController : AbstractResultController
    {
        public SequenceConcatenatorController() : base(TaskType.SequenceConcatenator)
        {

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
            var sequenceTypeFilter = new SequenceType[]
            {
                SequenceType.ChloroplastGenome,
                SequenceType.CompleteGenome,
                SequenceType.MitochondrialPlasmid,
                SequenceType.MitochondrionGenome,
                SequenceType.Plasmid,
                SequenceType.Plastid
            };
            matters = matters.Where(m => m.Nature == Nature.Genetic && sequenceTypeFilter.Contains(m.SequenceType)).ToList();
            var matterNameSpliters = new[] { "|", "chromosome", "plasmid", "segment" };
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
                if (Enumerable.Contains(tempArray[i], '_'))
                {
                    refArray.Add(mattersNames[i]);
                }
                else
                {
                    notRefArray.Add(mattersNames[i]);
                }
            }

            Dictionary<string, long[]> multisequencesRefMatters = refArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key + " ref", mn => mn.Select(m => m.Item1).ToArray());

            Dictionary<string, long[]> multisequencesNotRefMatters = notRefArray.GroupBy(mn => mn.Item2)
                .ToDictionary(mn => mn.Key, mn => mn.Select(m => m.Item1).ToArray());
            var result = multisequencesRefMatters;
            foreach (var multisequencesNotRefMatter in multisequencesNotRefMatters)
            {
                result.Add(multisequencesNotRefMatter.Key, multisequencesNotRefMatter.Value);
            }
            return result;

        }

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
                    var result = multisequences.Select(m => new { name = m.Key, matterIds = m.Value }).ToArray();
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

        [HttpPost]
        public ActionResult Result(Dictionary<string, long[]> multisequenceMatters, string[] multisequencesNames)
        {
            using (var db = new LibiadaWebEntities())
            {
                db.Database.ExecuteSqlCommand("UPDATE matter SET multisequence_id = NULL, multisequence_number = NULL");
                db.Database.ExecuteSqlCommand("DELETE FROM multisequence");
                Multisequence[] multisequences = new Multisequence[multisequencesNames.Length];

                for (int i = 0; i < multisequencesNames.Length; i++)
                {
                    multisequences[i] = new Multisequence
                    {
                        Name = multisequencesNames[i],
                        Nature = Nature.Genetic
                    };
                }

                db.Multisequence.AddRange(multisequences);
                db.SaveChanges();

                var exceptionCases = new Dictionary<string, object>();
                long mId = 0;
                var matters = db.Matter.Where(mt => mt.Nature == Nature.Genetic).ToDictionary(m => m.Id, m => m);
                for (int i = 0; i < multisequences.Length; i++)
                {
                    try
                    {
                        var matterIds = multisequenceMatters[multisequences[i].Name];
                        foreach (var matterId in matterIds)
                        {
                            db.Entry(matters[matterId]).State = EntityState.Modified;
                            matters[matterId].MultisequenceId = multisequences[i].Id;
                            mId = matterId;
                        }
                        MultisequenceRepository.SetSequenceNumbers(matterIds.Select(m => matters[m]).ToArray());
                        var check = matterIds.Select(m => matters[m]);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        exceptionCases.Add(multisequences[i].Name, matters[mId]);
                    }

                }

            }

            return RedirectToAction("Index", "Multisequence");
        }
    }
}