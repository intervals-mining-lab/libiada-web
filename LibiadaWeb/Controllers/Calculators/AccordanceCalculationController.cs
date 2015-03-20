namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Core.IntervalsManagers;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Math;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The accordance calculation controller.
    /// </summary>
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
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordanceCalculationController"/> class.
        /// </summary>
        public AccordanceCalculationController() : base("AccordanceCalculation", "Accordance calculation")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
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
            var calculatorsHelper = new ViewDataHelper(db);
            ViewBag.data = calculatorsHelper.FillCalculationData(c => c.AccordanceApplicable, 2, 2, true);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
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
        public ActionResult Index(
            long[] matterIds,
            int characteristicTypeLinkId,
            int notationId,
            int? languageId,
            int? translatorId,
            string calculationType)
        {
            return Action(() =>
            {
                if (matterIds.Length != 2)
                {
                    throw new ArgumentException("Count of selected matters must be 2.", "matterIds");
                }

                var characteristics = new List<double>();

                var firstMatterId = matterIds[0];
                var secondMatterId = matterIds[1];
                long firstSequenceId;
                if (db.Matter.Single(m => m.Id == firstMatterId).NatureId == Aliases.Nature.Literature)
                {
                    firstSequenceId = db.LiteratureSequence.Single(l => l.MatterId == firstMatterId &&
                                l.NotationId == notationId && l.LanguageId == languageId
                                && MathLogic.NullableCompare(translatorId, l.TranslatorId)).Id;
                }
                else
                {
                    firstSequenceId = db.CommonSequence.Single(c => c.MatterId == firstMatterId && c.NotationId == notationId).Id;
                }

                Chain firstChain = commonSequenceRepository.ToLibiadaChain(firstSequenceId);
                firstChain.FillIntervalManagers();

                long secondSequenceId;
                if (db.Matter.Single(m => m.Id == secondMatterId).NatureId == Aliases.Nature.Literature)
                {
                    secondSequenceId = db.LiteratureSequence.Single(l => l.MatterId == secondMatterId &&
                                l.NotationId == notationId && l.LanguageId == languageId
                                && MathLogic.NullableCompare(translatorId, l.TranslatorId)).Id;
                }
                else
                {
                    secondSequenceId = db.CommonSequence.Single(c => c.MatterId == secondMatterId && c.NotationId == notationId).Id;
                }

                Chain secondChain = commonSequenceRepository.ToLibiadaChain(secondSequenceId);
                secondChain.FillIntervalManagers();

                switch (calculationType)
                {
                    case "Equality":
                        if (!firstChain.Alphabet.Equals(secondChain.Alphabet))
                        {
                            throw new Exception("Alphabets of sequences are not equal.");
                        }

                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                        IAccordanceCalculator calculator = CalculatorsFactory.CreateAccordanceCalculator(className);
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                        
                        for (int i = 0; i < firstChain.Alphabet.Cardinality; i++)
                        {
                            var element = firstChain.Alphabet[i];
                            var intervalManager = new AccordanceIntervalsManager(firstChain.CongenericChain(element), secondChain.CongenericChain(element));
                            var characteristicValue = calculator.Calculate(intervalManager, link);
                            characteristics.Add(characteristicValue);
                        }

                        break;
                }

                var characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkId, notationId);

                return new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "matterNames", db.Matter.Where(m => matterIds.Contains(m.Id)).Select(m => m.Name).ToList() }, 
                                         { "characteristicName", characteristicName },
                                         { "calculationType", calculationType }
                                     };
            });
        }
    }
}
