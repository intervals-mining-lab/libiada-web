﻿namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Extensions;
    using LibiadaCore.Music;

    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The relation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class RelationCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly BinaryCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationCalculationController"/> class.
        /// </summary>
        public RelationCalculationController() : base(TaskType.RelationCalculation)
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = BinaryCharacteristicRepository.Instance;
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
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(CharacteristicCategory.Binary, 1, 1, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <param name="language">
        /// The language id.
        /// </param>
        /// <param name="translator">
        /// The translator id.
        /// </param>
        /// <param name="pauseTreatment">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfer">
        /// Sequential transfer flag used in music sequences.
        /// </param>
        /// <param name="trajectory">
        /// Reading trajectory for images.
        /// </param>
        /// <param name="filterSize">
        /// The filter size.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="frequencyFilter">
        /// The frequency filter.
        /// </param>
        /// <param name="frequencyCount">
        /// The frequency count.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long matterId,
            short characteristicLinkId,
            Notation notation,
            Language? language,
            Translator? translator,
            PauseTreatment? pauseTreatment,
            bool? sequentialTransfer,
            ImageOrderExtractor? trajectory,
            int filterSize,
            bool filter,
            bool frequencyFilter,
            int frequencyCount)
        {
            return CreateTask(() =>
            {
                string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);
                Matter matter = Cache.GetInstance().Matters.Single(m => m.Id == matterId);
                long sequenceId = commonSequenceRepository.GetSequenceIds(new[] { matterId }, 
                                                                          notation, 
                                                                          language, 
                                                                          translator, 
                                                                          pauseTreatment, 
                                                                          sequentialTransfer,
                                                                          trajectory).Single();

                Chain currentChain = commonSequenceRepository.GetLibiadaChain(sequenceId);
                var sequence = db.CommonSequence.Single(m => m.Id == sequenceId);

                var result = new Dictionary<string, object>
                {
                    { "isFilter", filter },
                    { "matterName", sequence.Matter.Name },
                    { "notationName", sequence.Notation.GetDisplayValue() },
                    { "characteristicName", characteristicName }
                };

                BinaryCharacteristic binaryCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                IBinaryCalculator calculator = BinaryCalculatorsFactory.CreateCalculator(binaryCharacteristic);
                Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);

                if (frequencyFilter)
                {
                    FrequencyCharacteristic(characteristicLinkId, frequencyCount, currentChain, sequenceId, calculator, link);
                }
                else
                {
                    NotFrequencyCharacteristic(characteristicLinkId, sequenceId, currentChain, calculator, link);
                }

                if (filter)
                {
                    var filteredResult = db.BinaryCharacteristicValue
                                       .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
                                       .OrderByDescending(b => b.Value)
                                       .Take(filterSize)
                                       .Select(rc => new
                                       {
                                           FirstElementId = rc.FirstElementId,
                                           SecondElementId = rc.SecondElementId,
                                           Value = rc.Value
                                       })
                                       .ToArray();

                    var firstElements = new List<string>();
                    var secondElements = new List<string>();
                    for (int i = 0; i < filterSize; i++)
                    {
                        long firstElementId = filteredResult[i].FirstElementId;
                        var firstElement = db.Element.Single(e => e.Id == firstElementId);
                        firstElements.Add(firstElement.Name ?? firstElement.Value);

                        long secondElementId = filteredResult[i].SecondElementId;
                        var secondElement = db.Element.Single(e => e.Id == secondElementId);
                        secondElements.Add(secondElement.Name ?? secondElement.Value);
                    }

                    result.Add("filteredResult", filteredResult);
                    result.Add("filterSize", filterSize);
                    result.Add("firstElements", firstElements);
                    result.Add("secondElements", secondElements);
                }
                else
                {
                    var characteristics = db.BinaryCharacteristicValue
                                        .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
                                        .GroupBy(b => b.FirstElementId)
                                        .ToDictionary(b => b.Key, b => b.ToDictionary(bb => bb.SecondElementId, bb => bb.Value));
                    var elementsIds = db.GetAlphabetElementIds(sequenceId);
                    var elements = db.Element
                        .Where(e => elementsIds.Contains(e.Id))
                        .OrderBy(e => e.Id)
                        .Select(e => new { Name = e.Name ?? e.Value, e.Id })
                        .ToArray();
                    
                    result.Add("characteristics", characteristics);
                    result.Add("elements", elements);
                }

                string json = JsonConvert.SerializeObject(result);

                return new Dictionary<string, string> { { "data", json } };
            });
        }

        /// <summary>
        /// The not frequency characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <param name="calculator">
        /// The calculator.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        private void NotFrequencyCharacteristic(short characteristicLinkId, long sequenceId, Chain chain, IBinaryCalculator calculator, Link link)
        {
            var newCharacteristics = new List<BinaryCharacteristicValue>();
            BinaryCharacteristicValue[] databaseCharacteristics = db.BinaryCharacteristicValue
                .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
                .ToArray();
            int calculatedCount = databaseCharacteristics.Length;
            int alphabetCardinality = chain.Alphabet.Cardinality;

            if (calculatedCount < alphabetCardinality * alphabetCardinality)
            {
                long[] sequenceElements = db.GetAlphabetElementIds(sequenceId);
                for (int i = 0; i < alphabetCardinality; i++)
                {
                    for (int j = 0; j < alphabetCardinality; j++)
                    {
                        long firstElementId = sequenceElements[i];
                        long secondElementId = sequenceElements[j];
                        if (i != j && !databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                        {
                            double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                            newCharacteristics.Add(characteristicTypeLinkRepository.CreateCharacteristic(sequenceId, characteristicLinkId, firstElementId, secondElementId, result));
                        }
                    }
                }
            }

            db.BinaryCharacteristicValue.AddRange(newCharacteristics);
            db.SaveChanges();
        }

        /// <summary>
        /// The frequency characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="frequencyCount">
        /// The frequency count.
        /// </param>
        /// <param name="chain">
        /// The chain.
        /// </param>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="calculator">
        /// The calculator.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        private void FrequencyCharacteristic(short characteristicLinkId, int frequencyCount, Chain chain, long sequenceId, IBinaryCalculator calculator, Link link)
        {
            long[] sequenceElements = db.GetAlphabetElementIds(sequenceId);
            var newCharacteristics = new List<BinaryCharacteristicValue>();
            BinaryCharacteristicValue[] databaseCharacteristics = db.BinaryCharacteristicValue
                .Where(b => b.SequenceId == sequenceId && b.CharacteristicLinkId == characteristicLinkId)
                .ToArray();

            // calculating frequencies of elements in alphabet
            Alphabet alphabet = chain.Alphabet;
            var frequencies = new (IBaseObject element, double frequency)[alphabet.Cardinality];
            for (int f = 0; f < alphabet.Cardinality; f++)
            {
                var probabilityCalculator = new Probability();
                double result = probabilityCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied);
                frequencies[f] = (alphabet[f], result);
            }

            // ordering alphabet by frequencies
            frequencies = frequencies.OrderBy(pair => pair.frequency).ToArray();

            // calculating relation characteristic only for elements with maximum frequency
            for (int i = 0; i < frequencyCount; i++)
            {
                for (int j = 0; j < frequencyCount; j++)
                {
                    int firstElementNumber = alphabet.IndexOf(frequencies[i].element) + 1;
                    int secondElementNumber = alphabet.IndexOf(frequencies[j].element) + 1;
                    long firstElementId = sequenceElements[firstElementNumber];
                    long secondElementId = sequenceElements[secondElementNumber];

                    // searching characteristic in database
                    if (!databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                    {
                        double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                        newCharacteristics.Add(characteristicTypeLinkRepository.CreateCharacteristic(sequenceId, characteristicLinkId, firstElementId, secondElementId, result));
                    }
                }
            }

            db.BinaryCharacteristicValue.AddRange(newCharacteristics);
            db.SaveChanges();
        }
    }
}
