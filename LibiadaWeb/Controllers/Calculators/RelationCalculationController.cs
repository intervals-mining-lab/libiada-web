namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Models.Repositories.Sequences;

    using Models;
    using Models.Repositories;
    using Models.Repositories.Catalogs;

    /// <summary>
    /// The relation calculation controller.
    /// </summary>
    public class RelationCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The link repository.
        /// </summary>
        private readonly LinkRepository linkRepository;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The binary characteristic repository.
        /// </summary>
        private readonly BinaryCharacteristicRepository binaryCharacteristicRepository;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        private readonly NotationRepository notationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationCalculationController"/> class.
        /// </summary>
        public RelationCalculationController() : base("RelationCalculation", "Relation calculation")
        {
            db = new LibiadaWebEntities();
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkRepository = new LinkRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
            binaryCharacteristicRepository = new BinaryCharacteristicRepository(db);
            matterRepository = new MatterRepository(db);
            notationRepository = new NotationRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);

            var characteristicsList = db.CharacteristicType.Where(c => c.BinarySequenceApplicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "links", links }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
                    { "translators", translators }
                };
            return View();
        }


        [HttpPost]
        public ActionResult Index(
            long matterId,
            int characteristicId,
            int? linkId,
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
                if (db.Matter.Single(m => m.Id == matterId).NatureId == Aliases.Nature.Literature)
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
                string className = db.CharacteristicType.Single(c => c.Id == characteristicId).ClassName;

                IBinaryCalculator calculator = CalculatorsFactory.CreateBinaryCalculator(className);
                var link = (Link)(linkId ?? 0);

                if (frequencyFilter)
                {
                    FrequencyCharacteristic(characteristicId, linkId, frequencyCount, currentChain, sequenceId, calculator, link);
                }
                else
                {
                    NotFrequencyCharacteristic(characteristicId, linkId, sequenceId, currentChain, calculator, link);
                }

                if (filter)
                {
                    filteredResult = db.BinaryCharacteristic.Where(b => b.SequenceId == sequenceId &&
                                                                         b.CharacteristicTypeId == characteristicId &&
                                                                         b.LinkId == linkId)
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
                    characteristics = db.BinaryCharacteristic.Where(b => b.SequenceId == sequenceId &&
                                                                          b.CharacteristicTypeId == characteristicId &&
                                                                          b.LinkId == linkId)
                        .OrderBy(b => b.SecondElementId)
                        .ThenBy(b => b.FirstElementId)
                        .ToList();
                    for (int m = 0; m < Math.Sqrt(characteristics.Count()); m++)
                    {
                        long firstElementId = characteristics[m].FirstElementId;
                        elements.Add(db.Element.Single(e => e.Id == firstElementId));
                    }
                }

                return new Dictionary<string, object>
                {
                    { "characteristics", characteristics },
                    { "isFilter", filter },
                    { "filteredResult", filteredResult },
                    { "firstElements", firstElements },
                    { "secondElements", secondElements },
                    { "filterSize", filterSize },
                    { "elements", elements },
                    { "characteristicName", db.CharacteristicType.Single(charact => charact.Id == characteristicId).Name },
                    { "matterName", db.CommonSequence.Single(m => m.Id == sequenceId).Matter.Name },
                    { "notationName", db.CommonSequence.Single(c => c.Id == sequenceId).Notation.Name }
                };
            });
        }

        /// <summary>
        /// The one word characteristic.
        /// </summary>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
        /// </param>
        /// <param name="wordId">
        /// The word id.
        /// </param>
        /// <param name="sequence">
        /// The db sequence.
        /// </param>
        /// <param name="chain">
        /// The current chain.
        /// </param>
        /// <param name="calculator">
        /// The calculator.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string OneWordCharacteristic(
            int characteristicId,
            int linkId,
            long wordId,
            CommonSequence sequence,
            Chain chain,
            IBinaryCalculator calculator,
            Link link)
        {
            int calculatedCount = db.BinaryCharacteristic.Count(b => b.SequenceId == sequence.Id &&
                                                                      b.CharacteristicTypeId == characteristicId &&
                                                                      b.LinkId == linkId && b.FirstElementId == wordId);
            List<long> sequenceElements = DbHelper.GetElementIds(db, sequence.Id);
            if (calculatedCount < chain.Alphabet.Cardinality)
            {
                // TODO: проверить что + - 1 нигде к индексам не надо добавлять
                int firstElementNumber = sequenceElements.IndexOf(wordId);
                for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                {
                    long secondElementId = sequenceElements[i];
                    if (!db.BinaryCharacteristic.Any(b =>
                                                      b.SequenceId == sequence.Id &&
                                                      b.CharacteristicTypeId == characteristicId &&
                                                      b.FirstElementId == wordId &&
                                                      b.SecondElementId == secondElementId &&
                                                      b.LinkId == linkId))
                    {
                        double result = calculator.Calculate(chain.GetRelationIntervalsManager(firstElementNumber + 1, i + 1), link);

                        binaryCharacteristicRepository.CreateBinaryCharacteristic(
                            sequence.Id,
                            characteristicId,
                            linkId,
                            wordId,
                            secondElementId,
                            result);
                    }
                }
            }

            calculatedCount = db.BinaryCharacteristic.Count(b => b.SequenceId == sequence.Id &&
                                                                  b.CharacteristicTypeId == characteristicId &&
                                                                  b.LinkId == linkId && b.SecondElementId == wordId);

            if (calculatedCount < chain.Alphabet.Cardinality)
            {
                int secondElementNumber = sequenceElements.IndexOf(wordId);
                for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                {
                    long firstElementId = sequenceElements[i];
                    if (!db.BinaryCharacteristic.Any(b =>
                                                      b.SequenceId == sequence.Id &&
                                                      b.CharacteristicTypeId == characteristicId &&
                                                      b.FirstElementId == firstElementId &&
                                                      b.SecondElementId == wordId &&
                                                      b.LinkId == linkId))
                    {
                        double result = calculator.Calculate(chain.GetRelationIntervalsManager(secondElementNumber, i + 1), link);
                        binaryCharacteristicRepository.CreateBinaryCharacteristic(
                            sequence.Id,
                            characteristicId,
                            linkId,
                            firstElementId,
                            wordId,
                            result);
                    }
                }
            }

            return db.Element.Single(e => e.Id == wordId).Name;
        }


        private void NotFrequencyCharacteristic(
            int characteristicId,
            int? linkId,
            long sequenceId,
            Chain chain,
            IBinaryCalculator calculator,
            Link link)
        {
            int calculatedCount = db.BinaryCharacteristic.Count(b => b.SequenceId == sequenceId &&
                                                                 b.CharacteristicTypeId == characteristicId &&
                                                                 b.LinkId == linkId);
            if (calculatedCount < chain.Alphabet.Cardinality * chain.Alphabet.Cardinality)
            {
                List<long> sequenceElements = DbHelper.GetElementIds(db, sequenceId);
                for (int i = 0; i < chain.Alphabet.Cardinality; i++)
                {
                    for (int j = 0; j < chain.Alphabet.Cardinality; j++)
                    {
                        long firstElementId = sequenceElements[i];
                        long secondElementId = sequenceElements[i];
                        if (!db.BinaryCharacteristic.Any(b =>
                                                          b.SequenceId == sequenceId &&
                                                          b.CharacteristicTypeId == characteristicId &&
                                                          b.FirstElementId == firstElementId &&
                                                          b.SecondElementId == secondElementId &&
                                                          b.LinkId == linkId))
                        {
                            double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                            binaryCharacteristicRepository.CreateBinaryCharacteristic(
                                sequenceId,
                                characteristicId,
                                linkId,
                                firstElementId,
                                secondElementId,
                                result);
                        }
                    }
                }
            }
        }


        private void FrequencyCharacteristic(
            int characteristicId,
            int? linkId,
            int frequencyCount,
            Chain chain,
            long sequenceId,
            IBinaryCalculator calculator,
            Link link)
        {
            List<long> sequenceElements = DbHelper.GetElementIds(db, sequenceId);

            // считаем частоты слов
            var frequencies = new List<KeyValuePair<IBaseObject, double>>();
            for (int f = 0; f < chain.Alphabet.Cardinality; f++)
            {
                var probabilityCalculator = new Probability();
                var result = probabilityCalculator.Calculate(chain.CongenericChain(f), Link.Both);
                frequencies.Add(new KeyValuePair<IBaseObject, double>(chain.Alphabet[f], result));
            }

            // сортируем алфавит по частоте
            SortKeyValuePairList(frequencies);

            // для заданного числа слов с наибольшей частотой считаем зависимости
            for (int i = 0; i < frequencyCount; i++)
            {
                for (int j = 0; j < frequencyCount; j++)
                {
                    int firstElementNumber = chain.Alphabet.IndexOf(frequencies[i].Key) + 1;
                    int secondElementNumber = chain.Alphabet.IndexOf(frequencies[j].Key) + 1;
                    long firstElementId = sequenceElements[firstElementNumber];
                    long secondElementId = sequenceElements[secondElementNumber];

                    // проверяем не посчитана ли уже эта характеристика
                    if (!db.BinaryCharacteristic.Any(b =>
                                                      b.SequenceId == sequenceId &&
                                                      b.CharacteristicTypeId == characteristicId &&
                                                      b.FirstElementId == firstElementId &&
                                                      b.SecondElementId == secondElementId &&
                                                      b.LinkId == linkId))
                    {
                        double result = calculator.Calculate(chain.GetRelationIntervalsManager(i + 1, j + 1), link);

                        binaryCharacteristicRepository.CreateBinaryCharacteristic(
                            sequenceId,
                            characteristicId,
                            linkId,
                            firstElementId,
                            secondElementId,
                            result);
                    }
                }
            }
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
