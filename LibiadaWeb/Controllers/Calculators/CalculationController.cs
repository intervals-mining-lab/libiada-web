namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaWeb.Models.Repositories.Sequences;
    using Models;
    using Models.Repositories.Catalogs;

    /// <summary>
    /// The calculation controller.
    /// </summary>
    public class CalculationController : AbstractResultController
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
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationController"/> class.
        /// </summary>
        public CalculationController() : base("Calculation", " Characteristics calculation")
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
            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable);

            var characteristicTypes = characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", 1 },
                    { "maximumSelectedMatters", int.MaxValue },
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
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="languageIds">
        /// The language ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translator ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicIds,
            int?[] linkIds,
            int[] notationIds,
            int[] languageIds,
            int?[] translatorIds)
        {
            return Action(() =>
            {
                matterIds = matterIds.OrderBy(m => m).ToArray();
                var characteristics = new List<List<double>>();
                var characteristicNames = new List<string>();

                foreach (var matterId in matterIds)
                {
                    characteristics.Add(new List<double>());
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

                        int characteristicId = characteristicIds[i];
                        int? linkId = linkIds[i];
                        if (db.Characteristic.Any(c => ((linkId == null && c.LinkId == null) || (linkId == c.LinkId)) &&
                                                  c.SequenceId == sequenceId &&
                                                  c.CharacteristicTypeId == characteristicId))
                        {
                            double dataBaseCharacteristic = db.Characteristic.Single(c => ((linkId == null && c.LinkId == null) || linkId == c.LinkId) &&
                                                                                            c.SequenceId == sequenceId &&
                                                                                            c.CharacteristicTypeId == characteristicId).Value;
                            characteristics.Last().Add(dataBaseCharacteristic);
                        }
                        else
                        {
                            Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                            tempChain.FillIntervalManagers();
                            string className =
                                db.CharacteristicType.Single(ct => ct.Id == characteristicId).ClassName;
                            IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                            var link = (Link)(linkId ?? 0);
                            var characteristicValue = calculator.Calculate(tempChain, link);

                            var dataBaseCharacteristic = new Characteristic
                            {
                                SequenceId = sequenceId,
                                CharacteristicTypeId = characteristicIds[i],
                                LinkId = linkId,
                                Value = characteristicValue,
                                ValueString = characteristicValue.ToString()
                            };
                            db.Characteristic.Add(dataBaseCharacteristic);
                            db.SaveChanges();
                            characteristics.Last().Add(characteristicValue);
                        }
                    }
                }

                var links = db.Link;

                for (int k = 0; k < characteristicIds.Length; k++)
                {
                    int characteristicId = characteristicIds[k];
                    int? linkId = linkIds[k];
                    int notationId = notationIds[k];
                    string linkName = linkId.HasValue ? links.Single(l => l.Id == linkId).Name : string.Empty;

                    characteristicNames.Add(string.Join("  ", db.CharacteristicType.Single(c => c.Id == characteristicId).Name, linkName, db.Notation.Single(n => n.Id == notationId).Name));
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

                return new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "matterNames", db.Matter.Where(m => matterIds.Contains(m.Id)).OrderBy(m => m.Id).Select(m => m.Name).ToList() }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "characteristicIds", characteristicIds }, 
                                         { "matterIds", matterIds },
                                         { "characteristicsList", characteristicsList }
                                     };
            });
        }
    }
}
