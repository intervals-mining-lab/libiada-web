namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

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
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// The binary characteristic repository.
        /// </summary>
        private readonly BinaryCharacteristicRepository binaryCharacteristicRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationCalculationController"/> class.
        /// </summary>
        public RelationCalculationController()
        {
            db = new LibiadaWebEntities();
            this.characteristicRepository = new CharacteristicTypeRepository(db);
            this.linkRepository = new LinkRepository(db);
            this.chainRepository = new ChainRepository(db);
            this.binaryCharacteristicRepository = new BinaryCharacteristicRepository(db);

            ControllerName = "RelationCalculation";
            DisplayName = "Relation calculation";
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
            List<chain> chains = db.chain.Include("matter").ToList();
            ViewBag.chainCheckBoxes = this.chainRepository.GetSelectListItems(chains, null);
            ViewBag.chains = chains;
            var languages = new List<string>();
            var fastaHeaders = new List<string>();
            foreach (chain chain in ViewBag.chains)
            {
                languages.Add(chain.matter.nature.id == Aliases.NatureLiterature
                                         ? db.literature_chain.Single(l => l.id == chain.id).language.name
                                         : null);
                fastaHeaders.Add(chain.matter.nature.id == Aliases.NatureGenetic
                                         ? db.dna_chain.Single(l => l.id == chain.id).fasta_header
                                         : null);

            }

            ViewBag.languages = languages;
            ViewBag.fastaHeaders = fastaHeaders;

            ViewBag.chainsList = this.chainRepository.GetSelectListItems(null);
            IEnumerable<characteristic_type> characteristics =
                db.characteristic_type.Where(c => c.binary_chain_applicable);
            ViewBag.characteristicsList = this.characteristicRepository.GetSelectListItems(characteristics, null);
            ViewBag.linksList = this.linkRepository.GetSelectListItems(null);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="chainId">
        /// The chain id.
        /// </param>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
        /// </param>
        /// <param name="filterSize">
        /// The filter size.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="frequency">
        /// The frequency.
        /// </param>
        /// <param name="frequencyCount">
        /// The frequency count.
        /// </param>
        /// <param name="oneWord">
        /// The one word.
        /// </param>
        /// <param name="wordId">
        /// The word id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long chainId, 
            int characteristicId, 
            int linkId, 
            int filterSize, 
            bool filter, 
            bool frequency, 
            int frequencyCount, 
            bool oneWord, 
            long wordId = 0)
        {
            return Action(() =>
            {
                var characteristics = new List<binary_characteristic>();
                var elements = new List<element>();
                List<binary_characteristic> filteredResult = null;
                List<binary_characteristic> filteredResult1 = null;
                List<binary_characteristic> filteredResult2 = null;
                var firstElements = new List<element>();
                var secondElements = new List<element>();
                string word = null;

                chain dbChain = db.chain.Single(c => c.id == chainId);


                Chain currentChain = this.chainRepository.ToLibiadaChain(dbChain.id);
                string className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

                IBinaryCalculator calculator = CalculatorsFactory.CreateBinaryCalculator(className);
                Link link = (Link) linkId;

                if (oneWord)
                {
                    word = this.OneWordCharacteristic(characteristicId, linkId, wordId, dbChain, currentChain,
                        calculator, link);

                    filteredResult1 = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                          b.characteristic_type_id == characteristicId &&
                                                                          b.link_id == linkId &&
                                                                          b.first_element_id == wordId)
                        .OrderBy(b => b.second_element_id)
                        .ToList();

                    filteredResult2 = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                          b.characteristic_type_id == characteristicId &&
                                                                          b.link_id == linkId &&
                                                                          b.second_element_id == wordId)
                        .OrderBy(b => b.first_element_id)
                        .ToList();

                    for (int l = 0; l < currentChain.Alphabet.Cardinality; l++)
                    {
                        long elementId = filteredResult1[l].second_element_id;
                        elements.Add(db.element.Single(e => e.id == elementId));
                    }
                }
                else
                {
                    if (frequency)
                    {
                        this.FrequencyCharacteristic(
                            characteristicId,
                            linkId,
                            frequencyCount,
                            currentChain,
                            dbChain,
                            calculator,
                            link);
                    }
                    else
                    {
                        this.NotFrequencyCharacteristic(characteristicId, linkId, dbChain, currentChain, calculator,
                            link);
                    }

                    if (filter)
                    {
                        filteredResult = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                             b.characteristic_type_id ==
                                                                             characteristicId &&
                                                                             b.link_id == linkId)
                            .OrderByDescending(b => b.value)
                            .Take(filterSize).ToList();

                        for (int l = 0; l < filterSize; l++)
                        {
                            long firstElementId = filteredResult[l].first_element_id;
                            firstElements.Add(db.element.Single(e => e.id == firstElementId));
                        }

                        for (int m = 0; m < filterSize; m++)
                        {
                            long secondElementId = filteredResult[m].second_element_id;
                            secondElements.Add(db.element.Single(e => e.id == secondElementId));
                        }
                    }
                    else
                    {
                        characteristics = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                              b.characteristic_type_id ==
                                                                              characteristicId &&
                                                                              b.link_id == linkId)
                            .OrderBy(b => b.second_element_id)
                            .ThenBy(b => b.first_element_id)
                            .ToList();
                        for (int m = 0; m < Math.Sqrt(characteristics.Count()); m++)
                        {
                            long firstElementId = characteristics[m].first_element_id;
                            elements.Add(db.element.Single(e => e.id == firstElementId));
                        }
                    }
                }

                return new Dictionary<string, object>
                {
                    {"characteristics", characteristics},
                    {"isFilter", filter},
                    {"filteredResult", filteredResult},
                    {"firstElements", firstElements},
                    {"secondElements", secondElements},
                    {"filterSize", filterSize},
                    {"elements", elements},
                    {
                        "characteristicName",
                        db.characteristic_type.Single(charact => charact.id == characteristicId).name
                    },
                    {"chainName", db.chain.Single(m => m.id == chainId).matter.name},
                    {"notationName", db.chain.Single(c => c.id == chainId).notation.name},
                    {"filteredResult1", filteredResult1},
                    {"filteredResult2", filteredResult2},
                    {"oneWord", oneWord},
                    {"word", word}
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
        /// <param name="dbChain">
        /// The db chain.
        /// </param>
        /// <param name="currentChain">
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
            chain dbChain,
            Chain currentChain,
            IBinaryCalculator calculator,
            Link link)
        {
            int calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                      b.characteristic_type_id == characteristicId &&
                                                                      b.link_id == linkId && b.first_element_id == wordId);
            List<long> chainElements = this.chainRepository.GetElementIds(dbChain.id);
            if (calculatedCount < currentChain.Alphabet.Cardinality)
            {
                // TODO: проверить что + - 1 нигде к индексам не надо добавлять
                int firstElementNumber = chainElements.IndexOf(wordId);
                for (int i = 0; i < currentChain.Alphabet.Cardinality; i++)
                {
                    long secondElementId = chainElements[i];
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == wordId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_id == linkId))
                    {
                        double result = calculator.Calculate(currentChain.GetRelationIntervalsManager(firstElementNumber + 1, i + 1), link);

                        this.binaryCharacteristicRepository.CreateBinaryCharacteristic(
                            dbChain.id,
                            characteristicId,
                            linkId,
                            wordId,
                            secondElementId,
                            result);
                    }
                }
            }

            calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_id == linkId && b.second_element_id == wordId);

            if (calculatedCount < currentChain.Alphabet.Cardinality)
            {
                int secondElementNumber = chainElements.IndexOf(wordId);
                for (int i = 0; i < currentChain.Alphabet.Cardinality; i++)
                {
                    long firstElementId = chainElements[i];
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == wordId &&
                                                      b.link_id == linkId))
                    {
                        double result = calculator.Calculate(currentChain.GetRelationIntervalsManager(secondElementNumber, i + 1), link);
                        this.binaryCharacteristicRepository.CreateBinaryCharacteristic(
                            dbChain.id,
                            characteristicId,
                            linkId,
                            firstElementId,
                            wordId,
                            result);
                    }
                }
            }

            return db.element.Single(e => e.id == wordId).name;
        }

        /// <summary>
        /// The not frequency characteristic.
        /// </summary>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
        /// </param>
        /// <param name="dbChain">
        /// The db chain.
        /// </param>
        /// <param name="currentChain">
        /// The current chain.
        /// </param>
        /// <param name="calculator">
        /// The calculator.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        private void NotFrequencyCharacteristic(
            int characteristicId,
            int linkId,
            chain dbChain,
            Chain currentChain,
            IBinaryCalculator calculator,
            Link link)
        {
            int calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                 b.characteristic_type_id == characteristicId &&
                                                                 b.link_id == linkId);
            if (calculatedCount < currentChain.Alphabet.Cardinality * currentChain.Alphabet.Cardinality)
            {
                List<long> chainElements = this.chainRepository.GetElementIds(dbChain.id);
                for (int i = 0; i < currentChain.Alphabet.Cardinality; i++)
                {
                    for (int j = 0; j < currentChain.Alphabet.Cardinality; j++)
                    {
                        long firstElementId = chainElements[i];
                        long secondElementId = chainElements[i];
                        if (!db.binary_characteristic.Any(b =>
                                                          b.chain_id == dbChain.id &&
                                                          b.characteristic_type_id == characteristicId &&
                                                          b.first_element_id == firstElementId &&
                                                          b.second_element_id == secondElementId &&
                                                          b.link_id == linkId))
                        {
                            double result = calculator.Calculate(currentChain.GetRelationIntervalsManager(i + 1, j + 1), link);

                            this.binaryCharacteristicRepository.CreateBinaryCharacteristic(
                                dbChain.id,
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

        /// <summary>
        /// The frequency characteristic.
        /// </summary>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
        /// </param>
        /// <param name="frequencyCount">
        /// The frequency count.
        /// </param>
        /// <param name="currentChain">
        /// The current chain.
        /// </param>
        /// <param name="dbChain">
        /// The db chain.
        /// </param>
        /// <param name="calculator">
        /// The calculator.
        /// </param>
        /// <param name="link">
        /// The link.
        /// </param>
        private void FrequencyCharacteristic(
            int characteristicId,
            int linkId,
            int frequencyCount,
            Chain currentChain,
            chain dbChain,
            IBinaryCalculator calculator,
            Link link)
        {
            List<long> chainElements = this.chainRepository.GetElementIds(dbChain.id);

            // считаем частоты слов
            var frequences = new List<KeyValuePair<IBaseObject, double>>();
            for (int f = 0; f < currentChain.Alphabet.Cardinality; f++)
            {
                var calc = new Probability();
                frequences.Add(new KeyValuePair<IBaseObject, double>(
                        currentChain.Alphabet[f],
                        calc.Calculate(currentChain.CongenericChain(f), Link.Both)));
            }

            // сортируем алфавит по частоте
            this.SortKeyValuePairList(frequences);

            // для заданного числа слов с наибольшей частотой считаем зависимости
            for (int i = 0; i < frequencyCount; i++)
            {
                for (int j = 0; j < frequencyCount; j++)
                {
                    int firstElementNumber = currentChain.Alphabet.IndexOf(frequences[i].Key) + 1;
                    int secondElementNumber = currentChain.Alphabet.IndexOf(frequences[j].Key) + 1;
                    long firstElementId = chainElements[firstElementNumber];
                    long secondElementId = chainElements[secondElementNumber];

                    // проверяем не посчитана ли уже эта характеристика
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_id == linkId))
                    {
                        double result = calculator.Calculate(currentChain.GetRelationIntervalsManager(i + 1, j + 1), link);

                        this.binaryCharacteristicRepository.CreateBinaryCharacteristic(
                            dbChain.id,
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
            arrayForSort.Sort(
                (firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }
    }
}