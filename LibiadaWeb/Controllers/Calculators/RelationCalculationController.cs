namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;

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
        /// The binary characteristic repository.
        /// </summary>
        private readonly BinaryCharacteristicRepository binaryCharacteristicRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationCalculationController"/> class.
        /// </summary>
        public RelationCalculationController() : base("Relation calculation")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            binaryCharacteristicRepository = new BinaryCharacteristicRepository();
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
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
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(c => c.BinarySequenceApplicable, 1, 1, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="languageId">
        /// The language id.
        /// </param>
        /// <param name="translatorId">
        /// The translator id.
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
            int characteristicTypeLinkId,
            int notationId,
            int? languageId,
            int? translatorId,
            int filterSize,
            bool filter,
            bool frequencyFilter,
            int frequencyCount)
        {
            return Action(() =>
            {
                var characteristics = new List<BinaryCharacteristic>();
                var elements = new List<Element>();
                List<BinaryCharacteristic> filteredResult = null;
                var firstElements = new List<Element>();
                var secondElements = new List<Element>();

                long sequenceId;
                if (db.Matter.Single(m => m.Id == matterId).Nature == Nature.Literature)
                {
                    sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId &&
                                l.NotationId == notationId
                                && l.LanguageId == languageId
                                && ((translatorId == null && l.TranslatorId == null)
                                                || (translatorId == l.TranslatorId))).Id;
                }
                else
                {
                    sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
                }

                Chain currentChain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;

                IBinaryCalculator calculator = CalculatorsFactory.CreateBinaryCalculator(className);
                var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);

                if (frequencyFilter)
                {
                    FrequencyCharacteristic(characteristicTypeLinkId, frequencyCount, currentChain, sequenceId, calculator, link);
                }
                else
                {
                    NotFrequencyCharacteristic(characteristicTypeLinkId, sequenceId, currentChain, calculator, link);
                }

                if (filter)
                {
                    filteredResult = db.BinaryCharacteristic.Where(b => b.SequenceId == sequenceId && b.CharacteristicTypeLinkId == characteristicTypeLinkId)
                        .OrderByDescending(b => b.Value)
                        .Take(filterSize).ToList();

                    for (int l = 0; l < filterSize; l++)
                    {
                        long firstElementId = filteredResult[l].FirstElementId;
                        firstElements.Add(db.Element.Single(e => e.Id == firstElementId));
                    }

                    for (int m = 0; m < filterSize; m++)
                    {
                        long secondElementId = filteredResult[m].SecondElementId;
                        secondElements.Add(db.Element.Single(e => e.Id == secondElementId));
                    }
                }
                else
                {
                    characteristics = db.BinaryCharacteristic.Where(b => b.SequenceId == sequenceId && b.CharacteristicTypeLinkId == characteristicTypeLinkId)
                        .OrderBy(b => b.SecondElementId)
                        .ThenBy(b => b.FirstElementId)
                        .ToList();
                    for (int m = 0; m < Math.Sqrt(characteristics.Count()); m++)
                    {
                        long firstElementId = characteristics[m].FirstElementId;
                        elements.Add(db.Element.Single(e => e.Id == firstElementId));
                    }
                }

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notationId);

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "isFilter", filter },
                    { "filteredResult", filteredResult },
                    { "firstElements", firstElements },
                    { "secondElements", secondElements },
                    { "filterSize", filterSize },
                    { "elements", elements },
                    { "characteristicName", characteristicName },
                    { "matterName", db.CommonSequence.Single(m => m.Id == sequenceId).Matter.Name },
                    { "notationName", db.CommonSequence.Single(c => c.Id == sequenceId).Notation.Name }
                };
            });
        }

        /// <summary>
        /// The not frequency characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
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
        private void NotFrequencyCharacteristic(
            int characteristicTypeLinkId,
            long sequenceId,
            Chain chain,
            IBinaryCalculator calculator,
            Link link)
        {
            var newCharacteristics = new List<BinaryCharacteristic>();
            var databaseCharacteristics = db.BinaryCharacteristic.Where(b => b.SequenceId == sequenceId 
                                                                    && b.CharacteristicTypeLinkId == characteristicTypeLinkId)
                                                           .ToArray();
            int calculatedCount = databaseCharacteristics.Length;
            if (calculatedCount < chain.Alphabet.Cardinality * chain.Alphabet.Cardinality)
            {
                List<long> sequenceElements = DbHelper.GetElementIds(db, sequenceId);
                for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                {
                    for (int j = 0; j < chain.Alphabet.Cardinality; j++)
                    {
                        long firstElementId = sequenceElements[i];
                        long secondElementId = sequenceElements[i];
                        if (!databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                        {
                            double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                            newCharacteristics.Add(binaryCharacteristicRepository.CreateBinaryCharacteristic(sequenceId, characteristicTypeLinkId, firstElementId, secondElementId, result));
                        }
                    }
                }
            }

            db.BinaryCharacteristic.AddRange(newCharacteristics);
            db.SaveChanges();
        }

        /// <summary>
        /// The frequency characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
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
        private void FrequencyCharacteristic(
            int characteristicTypeLinkId,
            int frequencyCount,
            Chain chain,
            long sequenceId,
            IBinaryCalculator calculator,
            Link link)
        {
            List<long> sequenceElements = DbHelper.GetElementIds(db, sequenceId);
            var newCharacteristics = new List<BinaryCharacteristic>();
            var databaseCharacteristics = db.BinaryCharacteristic.Where(b => b.SequenceId == sequenceId 
                                                                    && b.CharacteristicTypeLinkId == characteristicTypeLinkId)
                                                           .ToArray(); 

            // calculating frequencies of elements in alphabet
            var frequencies = new List<KeyValuePair<IBaseObject, double>>();
            for (int f = 0; f < chain.Alphabet.Cardinality; f++)
            {
                var probabilityCalculator = new Probability();
                var result = probabilityCalculator.Calculate(chain.CongenericChain(f), Link.NotApplied);
                frequencies.Add(new KeyValuePair<IBaseObject, double>(chain.Alphabet[f], result));
            }

            // ordering alphabet by frequencies
            SortKeyValuePairList(frequencies);

            // calculating relation characteristic only for elements with maximum frequency
            for (int i = 0; i < frequencyCount; i++)
            {
                for (int j = 0; j < frequencyCount; j++)
                {
                    int firstElementNumber = chain.Alphabet.IndexOf(frequencies[i].Key) + 1;
                    int secondElementNumber = chain.Alphabet.IndexOf(frequencies[j].Key) + 1;
                    long firstElementId = sequenceElements[firstElementNumber];
                    long secondElementId = sequenceElements[secondElementNumber];

                    // searching characteristic in database
                    if (!databaseCharacteristics.Any(b => b.FirstElementId == firstElementId && b.SecondElementId == secondElementId))
                    {
                        double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                        newCharacteristics.Add(binaryCharacteristicRepository.CreateBinaryCharacteristic(sequenceId, characteristicTypeLinkId, firstElementId, secondElementId, result));
                    }
                }
            }

            db.BinaryCharacteristic.AddRange(newCharacteristics);
            db.SaveChanges();
        }

        /// <summary>
        /// The sort key value pair list.
        /// </summary>
        /// <param name="arrayForSort">
        /// The array for sort.
        /// </param>
        private void SortKeyValuePairList(List<KeyValuePair<IBaseObject, double>> arrayForSort)
        {
            arrayForSort.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }
    }
}
