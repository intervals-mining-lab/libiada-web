namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
    using LibiadaCore.Music;
    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// The accordance calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AccordanceCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly AccordanceCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordanceCalculationController"/> class.
        /// </summary>
        public AccordanceCalculationController() : base(TaskType.AccordanceCalculation)
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = AccordanceCharacteristicRepository.Instance;
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
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(CharacteristicCategory.Accordance, 2, 2, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
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
        /// <param name="calculationType">
        /// The calculation type.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if count of matter ids is not 2.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown alphabets of sequences are not equal.
        /// </exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int characteristicLinkId,
            Notation notation,
            Language? language,
            Translator? translator,
            PauseTreatment? pauseTreatment,
            bool? sequentialTransfer,
            string calculationType)
        {
            return CreateTask(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Number of selected matters must be 2.", nameof(matterIds));
                }

                var characteristics = new Dictionary<int, Dictionary<int, double>>();
                string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);
                var result = new Dictionary<string, object>
                                 {
                                     { "characteristics", characteristics },
                                     { "matterNames", Cache.GetInstance().Matters.Where(m => matterIds.Contains(m.Id)).Select(m => m.Name).ToList() },
                                     { "characteristicName", characteristicName },
                                     { "calculationType", calculationType }
                                 };

                long firstMatterId = matterIds[0];
                long secondMatterId = matterIds[1];
                long firstSequenceId;
                long secondSequenceId;
                switch (notation.GetNature())
                {
                    case Nature.Literature:
                        firstSequenceId = db.LiteratureSequence.Single(l => l.MatterId == firstMatterId
                                                                         && l.Notation == notation
                                                                         && l.Language == language
                                                                         && l.Translator == translator).Id;
                        secondSequenceId = db.LiteratureSequence.Single(l => l.MatterId == secondMatterId
                                                                          && l.Notation == notation
                                                                          && l.Language == language
                                                                          && l.Translator == translator).Id;
                        break;
                    case Nature.Music:
                        firstSequenceId = db.MusicSequence.Single(m => m.MatterId == firstMatterId
                                                                    && m.Notation == notation
                                                                    && m.PauseTreatment == pauseTreatment
                                                                    && m.SequentialTransfer == sequentialTransfer).Id;
                        secondSequenceId = db.MusicSequence.Single(m => m.MatterId == secondMatterId
                                                                     && m.Notation == notation
                                                                     && m.PauseTreatment == pauseTreatment
                                                                     && m.SequentialTransfer == sequentialTransfer).Id;
                        break;
                    default:
                        firstSequenceId = db.CommonSequence.Single(c => c.MatterId == firstMatterId && c.Notation == notation).Id;
                        secondSequenceId = db.CommonSequence.Single(c => c.MatterId == secondMatterId && c.Notation == notation).Id;
                        break;
                }

                Chain firstChain = commonSequenceRepository.GetLibiadaChain(firstSequenceId);
                Chain secondChain = commonSequenceRepository.GetLibiadaChain(secondSequenceId);

                AccordanceCharacteristic accordanceCharacteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                IAccordanceCalculator calculator = AccordanceCalculatorsFactory.CreateCalculator(accordanceCharacteristic);
                Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                Alphabet firstChainAlphabet = firstChain.Alphabet;
                Alphabet secondChainAlphabet = secondChain.Alphabet;

                switch (calculationType)
                {
                    case "Equality":
                        if (!firstChainAlphabet.SetEquals(secondChainAlphabet))
                        {
                            throw new Exception("Alphabets of sequences are not equal.");
                        }

                        characteristics.Add(0, new Dictionary<int, double>());
                        characteristics.Add(1, new Dictionary<int, double>());
                        var alphabet = new List<string>();

                        for (int i = 0; i < firstChainAlphabet.Cardinality; i++)
                        {
                            IBaseObject element = firstChainAlphabet[i];
                            alphabet.Add(element.ToString());

                            CongenericChain firstCongenericChain = firstChain.CongenericChain(element);
                            CongenericChain secondCongenericChain = secondChain.CongenericChain(element);

                            double characteristicValue = calculator.Calculate(firstCongenericChain, secondCongenericChain, link);
                            characteristics[0].Add(i, characteristicValue);

                            characteristicValue = calculator.Calculate(secondCongenericChain, firstCongenericChain, link);
                            characteristics[1].Add(i, characteristicValue);
                        }

                        result.Add("alphabet", alphabet);
                        break;

                    case "All":
                        var firstAlphabet = new List<string>();
                        for (int i = 0; i < firstChain.Alphabet.Cardinality; i++)
                        {
                            characteristics.Add(i, new Dictionary<int, double>());
                            IBaseObject firstElement = firstChainAlphabet[i];
                            firstAlphabet.Add(firstElement.ToString());
                            for (int j = 0; j < secondChainAlphabet.Cardinality; j++)
                            {
                                var secondElement = secondChainAlphabet[j];

                                var firstCongenericChain = firstChain.CongenericChain(firstElement);
                                var secondCongenericChain = secondChain.CongenericChain(secondElement);

                                var characteristicValue = calculator.Calculate(firstCongenericChain, secondCongenericChain, link);
                                characteristics[i].Add(j, characteristicValue);
                            }
                        }

                        var secondAlphabet = new List<string>();
                        for (int j = 0; j < secondChainAlphabet.Cardinality; j++)
                        {
                            secondAlphabet.Add(secondChainAlphabet[j].ToString());
                        }

                        result.Add("firstAlphabet", firstAlphabet);
                        result.Add("secondAlphabet", secondAlphabet);
                        break;

                    case "Specified":
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentException("Calculation type is not implemented", nameof(calculationType));
                }

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
