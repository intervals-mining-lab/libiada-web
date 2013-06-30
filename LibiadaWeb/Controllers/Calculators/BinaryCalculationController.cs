using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.BinaryCalculators;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class BinaryCalculationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly NotationRepository notationRepository;
        private readonly ChainRepository chainRepository;

        public BinaryCalculationController()
        {
            characteristicRepository = new CharacteristicTypeRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            notationRepository = new NotationRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /Calculation/

        public ActionResult Index()
        {
            ViewBag.chains = db.chain.ToList();
            List<literature_chain> literatureChains = new List<literature_chain>();
            foreach (chain chain in ViewBag.chains)
            {
                if (chain.matter.nature.id == Aliases.NatureLiterature)
                {
                    literatureChains.Add(db.literature_chain.Single(l => l.id == chain.id));
                }
                else
                {
                    literatureChains.Add(null);
                }
            }
            ViewBag.literatureChains = literatureChains;
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            ViewBag.chainsList = chainRepository.GetSelectListItems(null);
            IEnumerable<characteristic_type> characteristics =
                db.characteristic_type.Where(c => Aliases.ApplicabilityBinary.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristics, null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(long chainId, int characteristicId, int linkUpId,
                                  int filterSize, bool filter, 
                                  bool frequency, int frequencyCount, 
                                  bool oneWord, long wordId)
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


            Chain currentChain = chainRepository.FromDbChainToLibiadaChain(dbChain.id);
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
            TempData["filtersize"] = filterSize;
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

            if (calculatedCount < currentChain.Alphabet.Power)
            {
                int firstElementNumber = dbChain.alphabet.Single(a => a.element_id == wordId).number;
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    long secondElementId = dbChain.alphabet.Single(a => a.number == i + 1).element_id;
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == wordId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_up_id == linkUpId))
                    {
                        binary_characteristic currentCharacteristic = new binary_characteristic();
                        currentCharacteristic.id =
                            db.ExecuteStoreQuery<long>("SELECT seq_next_value('characteristics_id_seq')")
                              .First();
                        currentCharacteristic.chain_id = dbChain.id;
                        currentCharacteristic.characteristic_type_id = characteristicId;
                        currentCharacteristic.link_up_id = linkUpId;
                        currentCharacteristic.first_element_id = wordId;
                        currentCharacteristic.second_element_id = secondElementId;
                        currentCharacteristic.value = calculator.Calculate(currentChain,
                                                                           currentChain.Alphabet[firstElementNumber - 1],
                                                                           currentChain.Alphabet[i], linkUp);
                        currentCharacteristic.value_string = currentCharacteristic.value.ToString();
                        currentCharacteristic.creation_date = DateTime.Now;
                        db.binary_characteristic.AddObject(currentCharacteristic);
                        db.SaveChanges();
                    }
                }
            }

            

            calculatedCount = db.binary_characteristic.Count(b => b.chain_id == dbChain.id &&
                                                                  b.characteristic_type_id == characteristicId &&
                                                                  b.link_up_id == linkUpId && b.second_element_id == wordId);

            if (calculatedCount < currentChain.Alphabet.Power)
            {
                int secondElementNumber = dbChain.alphabet.Single(a => a.element_id == wordId).number;
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    long firstElementId = dbChain.alphabet.Single(a => a.number == i + 1).element_id;
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == wordId &&
                                                      b.link_up_id == linkUpId))
                    {
                        binary_characteristic currentCharacteristic = new binary_characteristic();
                        currentCharacteristic.id =
                            db.ExecuteStoreQuery<long>("SELECT seq_next_value('characteristics_id_seq')")
                              .First();
                        currentCharacteristic.chain_id = dbChain.id;
                        currentCharacteristic.characteristic_type_id = characteristicId;
                        currentCharacteristic.link_up_id = linkUpId;
                        currentCharacteristic.first_element_id = firstElementId;
                        currentCharacteristic.second_element_id = wordId;
                        currentCharacteristic.value = calculator.Calculate(currentChain, currentChain.Alphabet[i],
                                                                           currentChain.Alphabet[secondElementNumber - 1],
                                                                           linkUp);
                        currentCharacteristic.value_string = currentCharacteristic.value.ToString();
                        currentCharacteristic.creation_date = DateTime.Now;
                        db.binary_characteristic.AddObject(currentCharacteristic);
                        db.SaveChanges();
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
                for (int i = 0; i < currentChain.Alphabet.Power; i++)
                {
                    for (int j = 0; j < currentChain.Alphabet.Power; j++)
                    {
                        long firstElementId = dbChain.alphabet.Single(a => a.number == i + 1).element_id;
                        long secondElementId = dbChain.alphabet.Single(a => a.number == j + 1).element_id;
                        if (!db.binary_characteristic.Any(b =>
                                                          b.chain_id == dbChain.id &&
                                                          b.characteristic_type_id == characteristicId &&
                                                          b.first_element_id == firstElementId &&
                                                          b.second_element_id == secondElementId &&
                                                          b.link_up_id == linkUpId))
                        {
                            binary_characteristic currentCharacteristic = new binary_characteristic();
                            currentCharacteristic.id =
                                db.ExecuteStoreQuery<long>("SELECT seq_next_value('characteristics_id_seq')")
                                  .First();
                            currentCharacteristic.chain_id = dbChain.id;
                            currentCharacteristic.characteristic_type_id = characteristicId;
                            currentCharacteristic.link_up_id = linkUpId;
                            currentCharacteristic.first_element_id = firstElementId;
                            currentCharacteristic.second_element_id = secondElementId;
                            currentCharacteristic.value = calculator.Calculate(currentChain,
                                                                               currentChain.Alphabet[i],
                                                                               currentChain.Alphabet[j], linkUp);
                            currentCharacteristic.value_string = currentCharacteristic.value.ToString();
                            currentCharacteristic.creation_date = DateTime.Now;
                            db.binary_characteristic.AddObject(currentCharacteristic);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private void FrequencyCharacteristic(int characteristicId, int linkUpId, int frequencyCount, Chain currentChain,
                                             chain dbChain, IBinaryCalculator calculator, LinkUp linkUp)
        {
            //считаем частоты слов
            List<KeyValuePair<IBaseObject, double>> frequences = new List<KeyValuePair<IBaseObject, double>>();
            for (int f = 0; f < currentChain.Alphabet.Power; f++)
            {
                Probability calc = new Probability();
                frequences.Add(new KeyValuePair<IBaseObject, double>(currentChain.Alphabet[f],
                                                                     calc.Calculate(currentChain.UniformChain(f),
                                                                                    LinkUp.Both)));
            }
            //сорьтруем алфавит по частоте
            SortKeyValuePairList(frequences);
            //для заданного числа слов с наибольшей частотой считаем зависимости
            for (int i = 0; i < frequencyCount; i++)
            {
                for (int j = 0; j < frequencyCount; j++)
                {
                    int firstElementNumber = currentChain.Alphabet.IndexOf(frequences[i].Key) + 1;
                    int secondElementNumber = currentChain.Alphabet.IndexOf(frequences[j].Key) + 1;
                    long firstElementId = dbChain.alphabet.Single(a => a.number == firstElementNumber).element_id;
                    long secondElementId = dbChain.alphabet.Single(a => a.number == secondElementNumber).element_id;
                    binary_characteristic currentCharacteristic;
                    //проверяем не посчитана ли уже эта характеристика
                    if (!db.binary_characteristic.Any(b =>
                                                      b.chain_id == dbChain.id &&
                                                      b.characteristic_type_id == characteristicId &&
                                                      b.first_element_id == firstElementId &&
                                                      b.second_element_id == secondElementId &&
                                                      b.link_up_id == linkUpId))
                    {
                        //считаем характеристику 
                        currentCharacteristic = new binary_characteristic();
                        currentCharacteristic.id =
                            db.ExecuteStoreQuery<long>("SELECT seq_next_value('characteristics_id_seq')")
                              .First();
                        currentCharacteristic.chain_id = dbChain.id;
                        currentCharacteristic.characteristic_type_id = characteristicId;
                        currentCharacteristic.link_up_id = linkUpId;
                        currentCharacteristic.first_element_id = firstElementId;
                        currentCharacteristic.second_element_id = secondElementId;
                        currentCharacteristic.value = calculator.Calculate(currentChain,
                                                                           currentChain.Alphabet[i],
                                                                           currentChain.Alphabet[j], linkUp);
                        currentCharacteristic.value_string = currentCharacteristic.value.ToString();
                        currentCharacteristic.creation_date = DateTime.Now;
                        db.binary_characteristic.AddObject(currentCharacteristic);
                        //сохраняем её в базу
                        db.SaveChanges();
                    }
                    else
                    {
                        //достаём характеристику из базы
                        currentCharacteristic = db.binary_characteristic.Single(b =>
                                                                                b.chain_id == dbChain.id &&
                                                                                b.characteristic_type_id ==
                                                                                characteristicId &&
                                                                                b.first_element_id == firstElementId &&
                                                                                b.second_element_id ==
                                                                                secondElementId &&
                                                                                b.link_up_id == linkUpId);
                    }
                }
            }
        }

        private void SortKeyValuePairList(List<KeyValuePair<IBaseObject, double>> arrayForSort)
        {
            arrayForSort.Sort(
                delegate(KeyValuePair<IBaseObject, double> firstPair,
                         KeyValuePair<IBaseObject, double> nextPair)
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
                );
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
                ViewBag.filtersize = TempData["filtersize"];
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
