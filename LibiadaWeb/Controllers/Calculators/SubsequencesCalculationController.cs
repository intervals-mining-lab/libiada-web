namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The subsequences calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The subsequence extractor.
        /// </summary>
        private readonly SubsequenceExtractor subsequenceExtractor;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The sequence attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
        /// </summary>
        public SubsequencesCalculationController() : base("Subsequences characteristics calculation")
        {
            db = new LibiadaWebEntities();
            subsequenceExtractor = new SubsequenceExtractor(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.GetSubsequencesViewData(1, int.MaxValue, true, "Calculate"));
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
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <param name="sort">
        /// The is sort.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
            int[] notationIds,
            int[] featureIds,
            bool sort)
        {
            return Action(() =>
            {
                var characteristics = new List<List<List<KeyValuePair<int, double>>>>();
                var matterNames = new List<string>();
                var sequenceAttributes = new List<List<List<string>>>();
                var sequencesPositions = new List<List<long>>();
                var sequenceFeatures = new List<List<string>>();
                var characteristicNames = new List<string>();
                var newCharacteristics = new List<Characteristic>();

                var parentSequences = db.DnaSequence.Where(s => matterIds.Contains(s.MatterId) && notationIds.Contains(s.NotationId)).ToArray();
                var sequenceIds = parentSequences.Select(s => s.Id).ToArray();
                var subsequenceIds = db.Subsequence.Where(s => sequenceIds.Contains(s.SequenceId) && featureIds.Contains(s.FeatureId)).Select(s => s.Id).ToArray();
                var dbCharacteristics = db.Characteristic.Where(c => characteristicTypeLinkIds.Contains(c.CharacteristicTypeLinkId) && subsequenceIds.Contains(c.SequenceId)).ToList();


                // cycle through matters; first level of characteristics array
                foreach (long matterId in matterIds)
                {
                    matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    sequenceAttributes.Add(new List<List<string>>());
                    sequencesPositions.Add(new List<long>());
                    sequenceFeatures.Add(new List<string>());
                    characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                    // cycle through characteristics and notations; second level of characteristics array
                    for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                    {
                        var notationId = notationIds[i];
                        var parentSequenceId = parentSequences.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                        List<Subsequence> subsequences = subsequenceExtractor.GetSubsequences(parentSequenceId, featureIds);
                        var sequences = subsequenceExtractor.ExtractChains(subsequences, parentSequenceId);

                        characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                        int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                        for (int j = 0; j < sequences.Count; j++)
                        {
                            long subsequenceId = subsequences[j].Id;

                            if (!dbCharacteristics.Any(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId))
                            {
                                double value = calculator.Calculate(sequences[j], link);
                                var currentCharacteristic = new Characteristic
                                                                {
                                                                    SequenceId = subsequenceId,
                                                                    CharacteristicTypeLinkId = characteristicTypeLinkId,
                                                                    Value = value,
                                                                    ValueString = value.ToString()
                                                                };

                                newCharacteristics.Add(currentCharacteristic);
                                dbCharacteristics.Add(currentCharacteristic);
                            }
                        }

                        for (int d = 0; d < sequences.Count; d++)
                        {
                            long subsequenceId = subsequences[d].Id;
                            double characteristic = dbCharacteristics.Single(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value;

                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(d, characteristic));

                            if (i == 0)
                            {
                                sequenceAttributes.Last().Add(sequenceAttributeRepository.GetAttributes(subsequences[d].Id));
                                sequencesPositions.Last().Add(subsequences[d].Start);

                                var featureId = subsequences[d].FeatureId;

                                sequenceFeatures.Last().Add(db.Feature.Single(p => featureId == p.Id).Name);
                            }
                        }
                    }
                }

                // trying to save calculated characteristics to database
                if (newCharacteristics.Count > 0)
                {
                    try
                    {
                        db.Characteristic.AddRange(newCharacteristics);
                        db.SaveChanges();
                    }
                    catch (Exception exception)
                    {
                        var characteristicsSequences = newCharacteristics.Select(c => c.SequenceId).Distinct().ToArray();
                        var characteristicsTypes = newCharacteristics.Select(c => c.CharacteristicTypeLinkId).Distinct().ToArray();
                        var characteristicsFilter = newCharacteristics.Select(c => new { c.SequenceId, c.CharacteristicTypeLinkId }).ToArray();
                        var wasteCharacteristics = db.Characteristic.Where(c => characteristicsSequences.Contains(c.SequenceId) && characteristicsTypes.Contains(c.CharacteristicTypeLinkId))
                                                                    .ToArray()
                                                                    .Where(c => characteristicsFilter.Contains(new { c.SequenceId, c.CharacteristicTypeLinkId }))
                                                                    .Select(c => new { c.SequenceId, c.CharacteristicTypeLinkId });
                        var wasteNewCharacteristics = newCharacteristics.Where(c => wasteCharacteristics.Contains(new { c.SequenceId, c.CharacteristicTypeLinkId }));

                        db.Characteristic.RemoveRange(wasteNewCharacteristics);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception anotherException)
                        {
                        }
                    }
                }



                // characteristics names
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notationIds[k]));
                }

                // rank sorting
                if (sort)
                {
                    for (int f = 0; f < matterIds.Length; f++)
                    {
                        for (int p = 0; p < characteristics[f].Count; p++)
                        {
                            SortKeyValuePairList(characteristics[f][p]);
                        }
                    }
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    });
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "matterNames", matterNames },
                    { "sequenceAttributes", sequenceAttributes },
                    { "sequencesPositions", sequencesPositions },
                    { "sequenceFeatures", sequenceFeatures },
                    { "characteristicNames", characteristicNames },
                    { "matterIds", matterIds },
                    { "characteristicsList", characteristicsList }
                };
            });
        }

        /// <summary>
        /// The sort key value pair list.
        /// </summary>
        /// <param name="arrayForSort">
        /// The array for sort.
        /// </param>
        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }
    }
}
