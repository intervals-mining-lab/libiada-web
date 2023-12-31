namespace Libiada.Web.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Extensions;
    using LibiadaCore.Music;

    using Libiada.Database.Tasks;

    using Newtonsoft.Json;
    using Libiada.Database.Models.Repositories.Sequences;
    using Microsoft.AspNetCore.Authorization;
    using Libiada.Database;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Libiada.Database.Models.Calculators;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The congeneric calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CongenericCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;
        private readonly ICongenericCharacteristicRepository congenericCharacteristicRepository;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly ICommonSequenceRepository commonSequenceRepository;
        private readonly Cache cache;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CongenericCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CongenericCalculationController"/> class.
        /// </summary>
        public CongenericCalculationController(LibiadaDatabaseEntities db, 
                                               IViewDataHelper viewDataHelper, 
                                               ITaskManager taskManager,
                                               ICongenericCharacteristicRepository congenericCharacteristicRepository,
                                               ICommonSequenceRepository commonSequenceRepository,
                                               Cache cache) 
            : base(TaskType.CongenericCalculation, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
            this.congenericCharacteristicRepository = congenericCharacteristicRepository;
            this.commonSequenceRepository = commonSequenceRepository;
            this.cache = cache;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewData = viewDataHelper.FillViewData(CharacteristicCategory.Congeneric, 1, int.MaxValue, "Calculate");
            ViewBag.data = JsonConvert.SerializeObject(viewData);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <param name="languages">
        /// The language ids.
        /// </param>
        /// <param name="translators">
        /// The translator ids.
        /// </param>
        /// <param name="pauseTreatments">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="imageOrderExtractor">
        /// Image order extractor of image sequences.
        /// </param>
        /// <param name="sequentialTransfers">
        /// Sequential transfer flag used in music sequences.
        /// </param>
        /// <param name="sort">
        /// The is sort.
        /// </param>
        /// <param name="theoretical">
        /// The theoretical.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            short[] characteristicLinkIds,
            Notation[] notations,
            Language[] languages,
            Translator[] translators,
            PauseTreatment[] pauseTreatments,
            bool[] sequentialTransfers,
            ImageOrderExtractor[] trajectories,
            bool sort,
            bool theoretical)
        {
            return CreateTask(() =>
            {
                var sequencesCharacteristics = new CongenericSequencesCharacteristics[matterIds.Length];
                var characteristicNames = new string[characteristicLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                Dictionary<long, string> mattersNames;
                long[][] sequenceIds;

                mattersNames = cache.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

                sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds, notations, languages, translators, pauseTreatments, sequentialTransfers, trajectories);

                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = congenericCharacteristicRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                    characteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = characteristicNames[k],
                        Selected = false
                    };

                }

                double[][][] characteristics = CongenericSequencesCharacteristicsCalculator.Calculate(sequenceIds, characteristicLinkIds);

                for (int i = 0; i < matterIds.Length; i++)
                {
                    sequencesCharacteristics[i] = new CongenericSequencesCharacteristics
                    {
                        MatterName = mattersNames[matterIds[i]],
                        Elements = null,
                        Characteristics = characteristics[i]
                    };
                }

                var result = new Dictionary<string, object>
                {
                        { "characteristics", sequencesCharacteristics },
                        { "characteristicNames", characteristicNames },
                        { "characteristicsList", characteristicsList }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };








                //var theoreticalRanks = new List<List<List<double>>>();
                //var elementNames = new List<List<string>>();
                //var newCharacteristics = new List<CongenericCharacteristicValue>();

                //var isLiteratureSequence = false;

                //// cycle through matters; first level of characteristics array
                //for (int w = 0; w < matterIds.Length; w++)
                //{
                //    long matterId = matterIds[w];
                //    elementNames.Add(new List<string>());
                //    characteristics.Add(new List<List<KeyValuePair<int, double>>>());
                //    theoreticalRanks.Add(new List<List<double>>());

                //    // cycle through characteristics and notations; second level of characteristics array
                //    for (int i = 0; i < characteristicLinkIds.Length; i++)
                //    {
                //        Notation notation = notations[i];

                //        long sequenceId;

                //        if (db.Matter.Single(m => m.Id == matterId).Nature == Nature.Literature)
                //        {
                //            Language language = languages[i];
                //            Translator? translator = translators[i];

                //            isLiteratureSequence = true;
                //            sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId
                //                                                      && l.Notation == notation
                //                                                      && l.Language == language
                //                                                      && translator == l.Translator).Id;
                //        }
                //        else
                //        {
                //            sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                //        }

                //        Chain chain = commonSequenceRepository.GetLibiadaChain(sequenceId);
                //        chain.FillIntervalManagers();
                //        characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                //        short characteristicLinkId = characteristicLinkIds[i];

                //        CongenericCharacteristic congenericCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                //        ICongenericCalculator calculator = CongenericCalculatorsFactory.CreateCalculator(congenericCharacteristic);
                //        Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                //        List<long> sequenceElements = db.GetElementIds(sequenceId);
                //        int calculated = db.CongenericCharacteristicValue.Count(c => c.SequenceId == sequenceId && c.CharacteristicLinkId == characteristicLinkId);
                //        if (calculated < chain.Alphabet.Cardinality)
                //        {
                //            for (int j = 0; j < chain.Alphabet.Cardinality; j++)
                //            {
                //                long elementId = sequenceElements[j];

                //                CongenericChain tempChain = chain.CongenericChain(j);

                //                if (!db.CongenericCharacteristicValue.Any(b => b.SequenceId == sequenceId
                //                                                       && b.CharacteristicLinkId == characteristicLinkId
                //                                                       && b.ElementId == elementId))
                //                {
                //                    double value = calculator.Calculate(tempChain, link);
                //                    var currentCharacteristic = new CongenericCharacteristicValue
                //                    {
                //                        SequenceId = sequenceId,
                //                        CharacteristicLinkId = characteristicLinkId,
                //                        ElementId = elementId,
                //                        Value = value
                //                    };

                //                    newCharacteristics.Add(currentCharacteristic);
                //                }
                //            }
                //        }

                //        // cycle through all alphabet elements; third level of characteristics array
                //        for (int d = 0; d < chain.Alphabet.Cardinality; d++)
                //        {
                //            long elementId = sequenceElements[d];

                //            double characteristic = db.CongenericCharacteristicValue.Single(c => c.SequenceId == sequenceId
                //                                                                         && c.CharacteristicLinkId == characteristicLinkId
                //                                                                         && c.ElementId == elementId).Value;

                //            characteristics.Last().Last().Add(new KeyValuePair<int, double>(d, characteristic));

                //            if (i == 0)
                //            {
                //                elementNames.Last().Add(chain.Alphabet[d].ToString());
                //            }
                //        }

                //        // theoretical frequencies of orlov criterion
                //        if (theoretical)
                //        {
                //            theoreticalRanks[w].Add(new List<double>());
                //            ICongenericCalculator countCalculator = CongenericCalculatorsFactory.CreateCalculator(CongenericCharacteristic.ElementsCount);
                //            var counts = new List<int>();
                //            for (int f = 0; f < chain.Alphabet.Cardinality; f++)
                //            {
                //                counts.Add((int)countCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied));
                //            }

                //            ICongenericCalculator frequencyCalculator = CongenericCalculatorsFactory.CreateCalculator(CongenericCharacteristic.Probability);
                //            var frequency = new List<double>();
                //            for (int f = 0; f < chain.Alphabet.Cardinality; f++)
                //            {
                //                frequency.Add(frequencyCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied));
                //            }

                //            double maxFrequency = frequency.Max();
                //            double k = 1 / Math.Log(counts.Max());
                //            double b = (k / maxFrequency) - 1;
                //            int n = 1;
                //            double plow = chain.Length;
                //            double p = k / (b + n);
                //            while (p >= (1 / plow))
                //            {
                //                theoreticalRanks.Last().Last().Add(p);
                //                n++;
                //                p = k / (b + n);
                //            }
                //        }
                //    }
                //}

                //db.CongenericCharacteristicValue.AddRange(newCharacteristics);
                //db.SaveChanges();

                //// characteristics names
                //for (int k = 0; k < characteristicLinkIds.Length; k++)
                //{
                //    string characteristicType = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                //    if (isLiteratureSequence)
                //    {
                //        Language language = languages[k];
                //        characteristicNames.Add(characteristicType + " " + language.GetDisplayValue());
                //    }
                //    else
                //    {
                //        characteristicNames.Add(characteristicType);
                //    }
                //}

                //// rank sorting
                //if (sort)
                //{
                //    for (int f = 0; f < matterIds.Length; f++)
                //    {
                //        for (int p = 0; p < characteristics[f].Count; p++)
                //        {
                //            SortKeyValuePairList(characteristics[f][p]);
                //        }
                //    }
                //}


                //return new Dictionary<string, object>
                //{
                //    { "characteristics", characteristics },
                //    { "elementNames", elementNames },
                //    { "characteristicNames", characteristicNames },
                //    { "matterIds", matterIds },
                //    { "theoreticalRanks", theoreticalRanks },
                //    { "characteristicsList", characteristicsList }
                //};
            });
        }


        /// <summary>
        /// The sort key value pair list.
        /// </summary>
        /// <param name="arrayForSort">
        /// The array for sort.
        /// </param>
        [NonAction]
        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }
    }
}
