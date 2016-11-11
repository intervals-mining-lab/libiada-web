using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibiadaWeb.Controllers.Calculators
{
    using System.Data.Entity;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    public class FilteredSubsequenceCalculationController : AbstractResultController
    {
       /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
        /// </summary>
        public FilteredSubsequenceCalculationController() : base("Subsequences characteristics calculation")
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(1, int.MaxValue, "Calculate"));
            }

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, int[] characteristicTypeLinkIds, int[] featureIds, string[] filters)
        {
            return Action(() =>
            {
                Dictionary<int, string> features;

                var characteristics = new Dictionary<string, SubsequenceData[]>(matterIds.Length);
                var matterNames = new string[matterIds.Length];
                var characteristicNames = new string[characteristicTypeLinkIds.Length];

                long[] parentSequenceIds;
                var calculators = new IFullCalculator[characteristicTypeLinkIds.Length];
                var links = new LibiadaCore.Core.Link[characteristicTypeLinkIds.Length];

                using (var db = new LibiadaWebEntities())
                {
                    var featureRepository = new FeatureRepository(db);
                    features = featureRepository.Features.ToDictionary(f => f.Id, f => f.Name);

                    var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                            .Where(s => s.NotationId == Aliases.Notation.Nucleotide && matterIds.Contains(s.MatterId))
                                            .Select(s => new { s.Id, MatterName = s.Matter.Name })
                                            .ToDictionary(s => s.Id);
                    parentSequenceIds = parentSequences.Keys.ToArray();

                    for (int n = 0; n < parentSequenceIds.Length; n++)
                    {
                        matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    }

                    var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                    for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                    {
                        var characteristicTypeLinkId = characteristicTypeLinkIds[k];
                        characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId);
                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        calculators[k] = CalculatorsFactory.CreateFullCalculator(className);
                        links[k] = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                    }
                }


                // cycle through matters; first level of characteristics array
                for (int m = 0; m < parentSequenceIds.Length; m++)
                {
                    // creating local context to avoid memory overflow due to possibly big cache of characteristics
                    using (var context = new LibiadaWebEntities())
                    {
                        var subsequenceExtractor = new SubsequenceExtractor(context);
                        var sequenceAttributeRepository = new SequenceAttributeRepository(context);
                        var newCharacteristics = new List<Characteristic>();

                        // extracting data from database
                        var dbSubsequences = subsequenceExtractor.GetSubsequences(parentSequenceIds[m], featureIds, filters);
                        var subsequenceIds = dbSubsequences.Select(s => s.Id).ToArray();
                        var dbSubsequencesAttributes = sequenceAttributeRepository.GetAttributes(subsequenceIds);

                        var dbCharacteristics = context.Characteristic
                                              .Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId))
                                              .ToArray()
                                              .GroupBy(c => c.SequenceId)
                                              .ToDictionary(c => c.Key, c => c.ToDictionary(ct => ct.CharacteristicTypeLinkId, ct => ct.Value));

                        // converting to libiada sequences
                        var sequences = subsequenceExtractor.ExtractChains(dbSubsequences, parentSequenceIds[m]);
                        characteristics.Add(matterNames[m], new SubsequenceData[sequences.Length]);
                        for (int j = 0; j < sequences.Length; j++)
                        {
                            var values = new double[characteristicTypeLinkIds.Length];
                            Dictionary<int, double> sequenceDbCharacteristics;
                            if (!dbCharacteristics.TryGetValue(dbSubsequences[j].Id, out sequenceDbCharacteristics))
                            {
                                sequenceDbCharacteristics = new Dictionary<int, double>();
                            }

                            // cycle through characteristics and notations; second level of characteristics array
                            for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                            {
                                int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                                if (!sequenceDbCharacteristics.TryGetValue(characteristicTypeLinkId, out values[i]))
                                {
                                    values[i] = calculators[i].Calculate(sequences[j], links[i]);
                                    var currentCharacteristic = new Characteristic
                                    {
                                        SequenceId = dbSubsequences[j].Id,
                                        CharacteristicTypeLinkId = characteristicTypeLinkId,
                                        Value = values[i]
                                    };

                                    newCharacteristics.Add(currentCharacteristic);
                                }
                            }

                            Dictionary<string, string> attributes;
                            if (!dbSubsequencesAttributes.TryGetValue(dbSubsequences[j].Id, out attributes))
                            {
                                attributes = new Dictionary<string, string>();
                            }

                            characteristics[matterNames[m]][j] = new SubsequenceData(dbSubsequences[j], values, attributes);
                        }

                        // trying to save calculated characteristics to database
                        var characteristicRepository = new CharacteristicRepository(context);
                        characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);
                    }
                }

                return new Dictionary<string, object>
                            {
                                { "characteristics", characteristics },
                                { "matterNames", matterNames },
                                { "characteristicNames", characteristicNames },
                                { "features", features }
                            };
            });
        }
    }
}