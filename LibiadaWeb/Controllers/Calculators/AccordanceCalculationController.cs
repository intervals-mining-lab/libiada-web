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
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The common sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordanceCalculationController"/> class.
        /// </summary>
        public AccordanceCalculationController()
            : base("AccordanceCalculation", "Accordance calculation")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var characteristicsList = db.CharacteristicType.Where(c => c.AccordanceApplicable);

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

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicId">
        /// The characteristic id.
        /// </param>
        /// <param name="linkId">
        /// The link id.
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
            int characteristicId,
            int? linkId,
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

                        string className = db.CharacteristicType.Single(ct => ct.Id == characteristicId).ClassName;
                        IBinaryCalculator calculator = CalculatorsFactory.CreateBinaryCalculator(className);
                        var link = (Link)(linkId ?? 0);
                        
                        for (int i = 0; i < firstChain.Alphabet.Cardinality; i++)
                        {
                            var element = firstChain.Alphabet[i];
                            var intervalManager = new BinaryIntervalsManager(firstChain.CongenericChain(element), secondChain.CongenericChain(element));
                            var characteristicValue = calculator.Calculate(intervalManager, link);
                            characteristics.Add(characteristicValue);
                        }

                        break;
                }

                var linkName = linkId.HasValue ? db.Link.Single(l => l.Id == linkId).Name : string.Empty;
                var characteristicTypeName = db.CharacteristicType.Single(c => c.Id == characteristicId).Name;
                var notationName = db.Notation.Single(n => n.Id == notationId).Name;
                var characteristicName = string.Join("  ", characteristicTypeName, linkName, notationName);

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
