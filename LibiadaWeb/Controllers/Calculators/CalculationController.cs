namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Core.SimpleTypes;
    using LibiadaCore.Misc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Account;
    using LibiadaWeb.Models.Repositories.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;
    using Models;
    using Models.Repositories.Catalogs;

    using Newtonsoft.Json;

    /// <summary>
    /// The calculation controller.
    /// </summary>
    [Authorize]
    public class CalculationController : AbstractResultController
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
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicRepository characteristicRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationController"/> class.
        /// </summary>
        public CalculationController() : base("Characteristics calculation")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            characteristicRepository = new CharacteristicRepository(db);
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
            ViewBag.MattersCheckboxes = true;

            Func<CharacteristicType, bool> filter;
            if (UserHelper.IsAdmin())
            {
                filter = c => c.FullSequenceApplicable;
            }
            else
            {
                var characteristicIds = new List<int>
                                            {
                                                Aliases.CharacteristicType.ATSkew, 
                                                Aliases.CharacteristicType.AlphabetCardinality, 
                                                Aliases.CharacteristicType.AverageRemoteness, 
                                                Aliases.CharacteristicType.GCRatio, 
                                                Aliases.CharacteristicType.GCSkew, 
                                                Aliases.CharacteristicType.GCToATRatio, 
                                                Aliases.CharacteristicType.IdentificationInformation, 
                                                Aliases.CharacteristicType.Length, 
                                                Aliases.CharacteristicType.MKSkew, 
                                                Aliases.CharacteristicType.RYSkew, 
                                                Aliases.CharacteristicType.SWSkew
                                            };
                filter = c => c.FullSequenceApplicable && characteristicIds.Contains(c.Id);
            }

            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillViewData(filter, 1, int.MaxValue, true, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notations ids.
        /// </param>
        /// <param name="languageIds">
        /// The languages ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translators ids.
        /// </param>
        /// <param name="rotate">
        /// Rotation flag.
        /// </param>
        /// <param name="complementary">
        /// Complement flag.
        /// </param>
        /// <param name="rotationLength">
        /// The rotation length.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
            int[] notationIds,
            int[] languageIds,
            int?[] translatorIds,
            bool rotate,
            bool complementary,
            uint? rotationLength)
        {
            return Action(() =>
            {
                var mattersCharacteristics = new List<object>();

                matterIds = matterIds.OrderBy(m => m).ToArray();
                var matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToList();
                var characteristicNames = new List<string>();
                var newCharacteristics = new List<Characteristic>();

                foreach (var matterId in matterIds)
                {
                    var characteristics = new List<double>();
                    for (int i = 0; i < notationIds.Length; i++)
                    {
                        int notationId = notationIds[i];

                        long sequenceId;
                        if (db.Matter.Single(m => m.Id == matterId).NatureId == Aliases.Nature.Literature)
                        {
                            int languageId = languageIds[i];
                            int? translatorId = translatorIds[i];

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

                        int characteristicTypeLinkId = characteristicTypeLinkIds[i];

                        Func<Characteristic, bool> characteristicFilter = c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId;

                        if (!rotate && !complementary && db.Characteristic.Any(characteristicFilter))
                        {
                            double characteristicValue = db.Characteristic.Single(characteristicFilter).Value;
                            characteristics.Add(characteristicValue);
                        }
                        else
                        {
                            Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                            if (complementary)
                            {
                                var sourceSequence = new Sequence(Alphabets.DNA, tempChain.ToString());
                                var complementarySequence = sourceSequence.GetReverseComplementedSequence();
                                tempChain = new Chain(complementarySequence.ConvertToString());
                            }

                            if (rotate)
                            {
                                var building = ArrayManipulator.RotateArray(tempChain.Building, rotationLength ?? 0);
                                var newSequence = building.Select(t => new ValueInt(t)).Cast<IBaseObject>().ToList();
                                tempChain = new Chain(newSequence);
                            }

                            tempChain.FillIntervalManagers();

                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;

                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            var characteristicValue = calculator.Calculate(tempChain, link);
                            if (!rotate && !complementary)
                            {
                                var characteristic = new Characteristic
                                {
                                    SequenceId = sequenceId,
                                    CharacteristicTypeLinkId = characteristicTypeLinkIds[i],
                                    Value = characteristicValue
                                };

                                newCharacteristics.Add(characteristic);
                            }

                            characteristics.Add(characteristicValue);
                        }
                    }

                    mattersCharacteristics.Add(new { matterName = matters.Single(m => m.Id == matterId).Name, characteristics });
                }

                // trying to save calculated characteristics to database
                characteristicRepository.TrySaveCharacteristicsToDatabase(newCharacteristics);

                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notationIds[k]));
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    });
                }

                var result = new Dictionary<string, object>()
                                 {
                                     { "characteristics", mattersCharacteristics },
                                     { "characteristicNames", characteristicNames },
                                     { "characteristicsList", characteristicsList }
                                 };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
