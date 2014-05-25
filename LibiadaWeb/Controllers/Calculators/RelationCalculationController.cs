// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationCalculationController.cs" company="">
//   
// </copyright>
// <summary>
//   The relation calculation controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.BinaryCalculators;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The relation calculation controller.
    /// </summary>
    public class RelationCalculationController : Controller
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
            this.db = new LibiadaWebEntities();
            this.characteristicRepository = new CharacteristicTypeRepository(this.db);
            this.linkRepository = new LinkRepository(this.db);
            this.chainRepository = new ChainRepository(this.db);
            this.binaryCharacteristicRepository = new BinaryCharacteristicRepository(this.db);
        }

        // GET: /RelationCalculation/
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            this.ViewBag.dbName = DbHelper.GetDbName(this.db);
            List<chain> chains = this.db.chain.Include("matter").ToList();
            this.ViewBag.chainCheckBoxes = this.chainRepository.GetSelectListItems(chains, null);
            this.ViewBag.chains = chains;
            var languages = new List<string>();
            var fastaHeaders = new List<string>();
            foreach (chain chain in this.ViewBag.chains)
            {
                languages.Add(chain.matter.nature.id == Aliases.NatureLiterature
                                         ? this.db.literature_chain.Single(l => l.id == chain.id).language.name
                                         : null);
                fastaHeaders.Add(chain.matter.nature.id == Aliases.NatureGenetic
                                         ? this.db.dna_chain.Single(l => l.id == chain.id).fasta_header
                                         : null);

            }

            this.ViewBag.languages = languages;
            this.ViewBag.fastaHeaders = fastaHeaders;

            this.ViewBag.chainsList = this.chainRepository.GetSelectListItems(null);
            IEnumerable<characteristic_type> characteristics =
                this.db.characteristic_type.Where(c => c.binary_chain_applicable);
            this.ViewBag.characteristicsList = this.characteristicRepository.GetSelectListItems(characteristics, null);
            this.ViewBag.linksList = this.linkRepository.GetSelectListItems(null);
            return this.View();
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
            var characteristics = new List<binary_characteristic>();
            var elements = new List<element>();
            List<binary_characteristic> filteredResult = null;
            List<binary_characteristic> filteredResult1 = null;
            List<binary_characteristic> filteredResult2 = null;
            var firstElements = new List<element>();
            var secondElements = new List<element>();
            string word = null;

            chain dbChain = this.db.chain.Single(c => c.id == chainId);


            Chain currentChain = this.chainRepository.ToLibiadaChain(dbChain.id);
            string className = this.db.characteristic_type.Single(c => c.id == characteristicId).class_name;

            IBinaryCalculator calculator = BinaryCalculatorsFactory.Create(className);
            Link link = (Link)linkId;

            if (oneWord)
            {
                word = this.OneWordCharacteristic(characteristicId, linkId, wordId, dbChain, currentChain, calculator, link);

                filteredResult1 = this.db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_id == linkId && b.first_element_id == wordId)
                                .OrderBy(b => b.second_element_id)
                                .ToList();

                filteredResult2 = this.db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_id == linkId && b.second_element_id == wordId)
                                .OrderBy(b => b.first_element_id)
                                .ToList();

                for (int l = 0; l < currentChain.Alphabet.Cardinality; l++)
                {
                    long elementId = filteredResult1[l].second_element_id;
                    elements.Add(this.db.element.Single(e => e.id == elementId));
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
                    this.NotFrequencyCharacteristic(characteristicId, linkId, dbChain, currentChain, calculator, link);
                }

                if (filter)
                {
                    filteredResult = this.db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                         b.characteristic_type_id == characteristicId &&
                                                                         b.link_id == linkId)
                                       .OrderByDescending(b => b.value)
                                       .Take(filterSize).ToList();

                    for (int l = 0; l < filterSize; l++)
                    {
                        long firstElementId = filteredResult[l].first_element_id;
                        firstElements.Add(this.db.element.Single(e => e.id == firstElementId));
                    }

                    for (int m = 0; m < filterSize; m++)
                    {
                        long secondElementId = filteredResult[m].second_element_id;
                        secondElements.Add(this.db.element.Single(e => e.id == secondElementId));
                    }
                }
                else
                {
                    characteristics = this.db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                          b.characteristic_type_id == characteristicId &&
                                                                          b.link_id == linkId)
                                        .OrderBy(b => b.second_element_id)
                                        .ThenBy(b => b.first_element_id)
                                        .ToList();
                    for (int m = 0; m < Math.Sqrt(characteristics.Count()); m++)
                    {
                        long firstElementId = characteristics[m].first_element_id;
                        elements.Add(this.db.element.Single(e => e.id == firstElementId));
                    }
                }
            }

            this.TempData["filter"] = filter;
            this.TempData["filteredResult"] = filteredResult;
            this.TempData["firstElements"] = firstElements;
            this.TempData["secondElements"] = secondElements;
            this.TempData["filterSize"] = filterSize;
            this.TempData["characteristics"] = characteristics;
            this.TempData["elements"] = elements;
            this.TempData["characteristicName"] =
                this.db.characteristic_type.Single(charact => charact.id == characteristicId).name;
            this.TempData["chainName"] = this.db.chain.Single(m => m.id == chainId).matter.name;
            this.TempData["notationName"] = this.db.chain.Single(c => c.id == chainId).notation.name;
            this.TempData["filteredResult1"] = filteredResult1;
            this.TempData["filteredResult2"] = filteredResult2;
            this.TempData["oneWord"] = oneWord;
            this.TempData["word"] = word;

            return this.RedirectToAction("Result");
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
            int calculatedCount = this.db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
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
                    if (!this.db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == wordId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_id == linkId))
                    {
                        double result = calculator.Calculate(
                            currentChain, 
                            currentChain.Alphabet[firstElementNumber], 
                            currentChain.Alphabet[i], 
                            link);

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

            calculatedCount = this.db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_id == linkId && b.second_element_id == wordId);

            if (calculatedCount < currentChain.Alphabet.Cardinality)
            {
                int secondElementNumber = chainElements.IndexOf(wordId);
                for (int i = 0; i < currentChain.Alphabet.Cardinality; i++)
                {
                    long firstElementId = chainElements[i];
                    if (!this.db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == wordId &&
                                                      b.link_id == linkId))
                    {
                        double result = calculator.Calculate(
                            currentChain, 
                            currentChain.Alphabet[secondElementNumber - 1], 
                            currentChain.Alphabet[i], 
                            link);
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

            return this.db.element.Single(e => e.id == wordId).name;
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
            int calculatedCount = this.db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
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
                        if (!this.db.binary_characteristic.Any(b =>
                                                          b.chain_id == dbChain.id &&
                                                          b.characteristic_type_id == characteristicId &&
                                                          b.first_element_id == firstElementId &&
                                                          b.second_element_id == secondElementId &&
                                                          b.link_id == linkId))
                        {
                            double result = calculator.Calculate(
                                currentChain, 
                                currentChain.Alphabet[i], 
                                currentChain.Alphabet[j], 
                                link);
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
                    if (!this.db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_id == linkId))
                    {
                        double result = calculator.Calculate(
                            currentChain, 
                            currentChain.Alphabet[i], 
                            currentChain.Alphabet[j], 
                            link);
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

        /// <summary>
        /// The result.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Result()
        {
            this.ViewBag.chainName = this.TempData["chainName"] as String;
            this.ViewBag.characteristicName = this.TempData["characteristicName"] as String;
            this.ViewBag.notationName = this.TempData["notationName"];
            this.ViewBag.isFilter = this.TempData["filter"];
            this.ViewBag.filteredResult1 = this.TempData["filteredResult1"];
            this.ViewBag.filteredResult2 = this.TempData["filteredResult2"];
            this.ViewBag.oneWord = this.TempData["oneWord"];
            this.ViewBag.word = this.TempData["word"];
            this.ViewBag.firstElements = this.TempData["firstElements"];
            this.ViewBag.secondElements = this.TempData["secondElements"];
            this.ViewBag.elements = this.TempData["elements"] as List<element>;

            if ((bool)this.TempData["filter"])
            {
                this.ViewBag.filtersize = this.TempData["filterSize"];
                this.ViewBag.filteredResult = this.TempData["filteredResult"] as List<binary_characteristic>;
            }
            else
            {
                this.ViewBag.characteristics = this.TempData["characteristics"] as List<binary_characteristic>;
            }

            this.TempData.Keep();

            return this.View();
        }
    }
}