using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.BinaryCalculators;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class BinaryCalculationController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;
        private readonly BinaryCharacteristicRepository binaryCharacteristicRepository;

        public BinaryCalculationController()
        {
            db = new LibiadaWebEntities();
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
            binaryCharacteristicRepository = new BinaryCharacteristicRepository(db);
        }

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            List<chain> chains = db.chain.Include("matter").ToList();
            ViewBag.chainCheckBoxes = chainRepository.GetSelectListItems(chains, null);
            ViewBag.chains = chains;
            List<String> languages = new List<String>();
            List<String> fastaHeaders = new List<String>();
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

            ViewBag.chainsList = chainRepository.GetSelectListItems(null);
            IEnumerable<characteristic_type> characteristics =
                db.characteristic_type.Where(c => Aliases.ApplicabilityBinary.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristics, null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long chainId, int characteristicId, int linkUpId,
                                  int filterSize, bool filter, 
                                  bool frequency, int frequencyCount, 
                                  bool oneWord, long wordId = 0)
        {
            List<binary_characteristic> characteristics = new List<binary_characteristic>();
            List<element> elements = new List<element>();
            List<binary_characteristic> filteredResult = null;
            List<binary_characteristic> filteredResult1 = null;
            List<binary_characteristic> filteredResult2 = null;
            List<element> firstElements = new List<element>();
            List<element> secondElements = new List<element>();
            String word = null;

            chain dbChain = db.chain.Single(c => c.id == chainId);


            Chain currentChain = chainRepository.ToLibiadaChain(dbChain.id);
            String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;

            IBinaryCalculator calculator = BinaryCalculatorsFactory.Create(className);
            LinkUp linkUp = (LinkUp) linkUpId;

            if (oneWord)
            {
                word = OneWordCharacteristic(characteristicId, linkUpId, wordId, dbChain, currentChain, calculator, linkUp);

                filteredResult1 = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_up_id == linkUpId && b.first_element_id == wordId)
                                .OrderBy(b => b.second_element_id)
                                .ToList();

                filteredResult2 = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_up_id == linkUpId && b.second_element_id == wordId)
                                .OrderBy(b => b.first_element_id)
                                .ToList();

                for (int l = 0; l < currentChain.Alphabet.Power; l++)
                {
                    long elementId = filteredResult1[l].second_element_id;
                    elements.Add(db.element.Single(e => e.id == elementId));
                }
            }
            else
            {
                if (frequency)
                {
                    FrequencyCharacteristic(characteristicId, linkUpId, frequencyCount, currentChain, dbChain,
                                            calculator, linkUp);
                }
                else
                {
                    NotFrequencyCharacteristic(characteristicId, linkUpId, dbChain, currentChain, calculator, linkUp);
                }

                if (filter)
                {
                    filteredResult = db.binary_characteristic.Where(b => b.chain_id == dbChain.id &&
                                                                         b.characteristic_type_id == characteristicId &&
                                                                         b.link_up_id == linkUpId)
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
                                                                          b.characteristic_type_id == characteristicId &&
                                                                          b.link_up_id == linkUpId)
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

            TempData["filter"] = filter;
            TempData["filteredResult"] = filteredResult;
            TempData["firstElements"] = firstElements;
            TempData["secondElements"] = secondElements;
            TempData["filterSize"] = filterSize;
            TempData["characteristics"] = characteristics;
            TempData["elements"] = elements;
            TempData["characteristicName"] =
                db.characteristic_type.Single(charact => charact.id == characteristicId).name;
            TempData["chainName"] = db.chain.Single(m => m.id == chainId).matter.name;
            TempData["notationName"] = db.chain.Single(c => c.id == chainId).notation.name;
            TempData["filteredResult1"] = filteredResult1;
            TempData["filteredResult2"] = filteredResult2;
            TempData["oneWord"] = oneWord;
            TempData["word"] = word;

            return RedirectToAction("Result");
        }

        private String OneWordCharacteristic(int characteristicId, int linkUpId, long wordId, chain dbChain, Chain currentChain,
                                           IBinaryCalculator calculator, LinkUp linkUp)
        {
            int calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                      b.characteristic_type_id == characteristicId &&
                                                                      b.link_up_id == linkUpId && b.first_element_id == wordId);
            List<long> chainElements = chainRepository.GetElementIds(dbChain.id);
            if (calculatedCount < currentChain.Alphabet.Power)
            {
                //TODO: проверить что + - 1 нигде к индексам не надо добавлять
                
                int firstElementNumber = chainElements.IndexOf(wordId);
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    long secondElementId = chainElements[i];
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == wordId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_up_id == linkUpId))
                    {
                        double result = calculator.Calculate(currentChain, currentChain.Alphabet[firstElementNumber],
                                                             currentChain.Alphabet[i],
                                                             linkUp);
                        binaryCharacteristicRepository.CreateBinaryCharacteristic(dbChain.id, characteristicId, linkUpId,
                                                                                  wordId, secondElementId, result);

                    }
                }
            }

            

            calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_up_id == linkUpId && b.second_element_id == wordId);

            if (calculatedCount < currentChain.Alphabet.Power)
            {
                int secondElementNumber = chainElements.IndexOf(wordId);
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    long firstElementId = chainElements[i];
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == wordId &&
                                                      b.link_up_id == linkUpId))
                    {
                        double result = calculator.Calculate(currentChain, currentChain.Alphabet[secondElementNumber - 1],
                                                             currentChain.Alphabet[i],
                                                             linkUp);
                        binaryCharacteristicRepository.CreateBinaryCharacteristic(dbChain.id, characteristicId, linkUpId,
                                                                                  firstElementId, wordId, result);
                    }
                }
            }
            return db.element.Single(e => e.id == wordId).name;
        }

        private void NotFrequencyCharacteristic(int characteristicId, int linkUpId, chain dbChain, Chain currentChain,
                                                IBinaryCalculator calculator, LinkUp linkUp)
        {
            int calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                 b.characteristic_type_id == characteristicId &&
                                                                 b.link_up_id == linkUpId);
            if (calculatedCount < currentChain.Alphabet.Power*currentChain.Alphabet.Power)
            {
                List<long> chainElements = chainRepository.GetElementIds(dbChain.id);
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    for (int j = 0; j < currentChain.Alphabet.Power; j++)
                    {
                        long firstElementId = chainElements[i];
                        long secondElementId = chainElements[i];
                        if (!db.binary_characteristic.Any(b =>
                                                          b.chain_id == dbChain.id &&
                                                          b.characteristic_type_id == characteristicId &&
                                                          b.first_element_id == firstElementId &&
                                                          b.second_element_id == secondElementId &&
                                                          b.link_up_id == linkUpId))
                        {
                            double result = calculator.Calculate(currentChain,
                                                                 currentChain.Alphabet[i],
                                                                 currentChain.Alphabet[j], linkUp);
                            binaryCharacteristicRepository.CreateBinaryCharacteristic(dbChain.id, characteristicId,
                                                                                      linkUpId, firstElementId,
                                                                                      secondElementId, result);
                        }
                    }
                }
            }
        }

        private void FrequencyCharacteristic(int characteristicId, int linkUpId, int frequencyCount, Chain currentChain,
                                             chain dbChain, IBinaryCalculator calculator, LinkUp linkUp)
        {
            List<long> chainElements = chainRepository.GetElementIds(dbChain.id);
            //считаем частоты слов
            List<KeyValuePair<IBaseObject, double>> frequences = new List<KeyValuePair<IBaseObject, double>>();
            for (int f = 0; f < currentChain.Alphabet.Power; f++)
            {
                Probability calc = new Probability();
                frequences.Add(new KeyValuePair<IBaseObject, double>(currentChain.Alphabet[f],
                                                                     calc.Calculate(currentChain.CongenericChain(f),
                                                                                    LinkUp.Both)));
            }
            //сортируем алфавит по частоте
            SortKeyValuePairList(frequences);
            //для заданного числа слов с наибольшей частотой считаем зависимости
            for (int i = 0; i < frequencyCount; i++)
            {
                for (int j = 0; j < frequencyCount; j++)
                {
                    int firstElementNumber = currentChain.Alphabet.IndexOf(frequences[i].Key) + 1;
                    int secondElementNumber = currentChain.Alphabet.IndexOf(frequences[j].Key) + 1;
                    long firstElementId = chainElements[firstElementNumber];
                    long secondElementId = chainElements[secondElementNumber];
                    //проверяем не посчитана ли уже эта характеристика
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_up_id == linkUpId))
                    {
                        double result = calculator.Calculate(currentChain,
                                                                 currentChain.Alphabet[i],
                                                                 currentChain.Alphabet[j], linkUp);
                        binaryCharacteristicRepository.CreateBinaryCharacteristic(dbChain.id, characteristicId,
                                                                                  linkUpId, firstElementId,
                                                                                  secondElementId, result);
                    }
                }
            }
        }

        private void SortKeyValuePairList(List<KeyValuePair<IBaseObject, double>> arrayForSort)
        {
            arrayForSort.Sort(
                (firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));
        }

        public ActionResult Result()
        {
            ViewBag.chainName = TempData["chainName"] as String;
            ViewBag.characteristicName = TempData["characteristicName"] as String;
            ViewBag.notationName = TempData["notationName"];
            ViewBag.isFilter = TempData["filter"];
            ViewBag.filteredResult1 = TempData["filteredResult1"];
            ViewBag.filteredResult2 = TempData["filteredResult2"];
            ViewBag.oneWord = TempData["oneWord"];
            ViewBag.word = TempData["word"];
            ViewBag.firstElements = TempData["firstElements"];
            ViewBag.secondElements = TempData["secondElements"];
            ViewBag.elements = TempData["elements"] as List<element>;

            if ((bool) TempData["filter"])
            {
                ViewBag.filtersize = TempData["filterSize"];
                ViewBag.filteredResult = TempData["filteredResult"] as List<binary_characteristic>;
            }
            else
            {
                ViewBag.characteristics = TempData["characteristics"] as List<binary_characteristic>;
            }

            TempData.Keep();

            return View();
        }
    }
}
