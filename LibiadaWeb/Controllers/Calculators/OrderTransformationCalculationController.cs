namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;
    using LibiadaCore.Misc;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;
    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// The order transformation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationCalculationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationCalculationController"/> class.
        /// </summary>
        public OrderTransformationCalculationController() : base(TaskType.OrderTransformationCalculation)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var db = new LibiadaWebEntities();
            var viewDataHelper = new ViewDataHelper(db);
            Dictionary<string, object> data = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate");

            var transformations = EnumHelper.GetSelectList(typeof(OrderTransformation));
            data.Add("transformations", transformations);

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="transformationLinkIds">
        /// The transformation link ids.
        /// </param>
        /// <param name="transformationsSequence">
        /// The transformation ids.
        /// </param>
        /// <param name="iterationsCount">
        /// Number of transformations iterations.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <param name="languages">
        /// The language ids.
        /// </param>
        /// <param name="translators">
        /// The translator ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            OrderTransformation[] transformationsSequence,
            int iterationsCount,
            short[] characteristicLinkIds,
            Notation[] notations,
            Language[] languages,
            Translator?[] translators)
        {
            return CreateTask(() =>
            {
                var db = new LibiadaWebEntities();
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                var commonSequenceRepository = new CommonSequenceRepository(db);
                var mattersCharacteristics = new object[matterIds.Length];
                matterIds = matterIds.OrderBy(m => m).ToArray();
                Dictionary<long, Matter> matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);

                for (int i = 0; i < matterIds.Length; i++)
                {
                    long matterId = matterIds[i];
                    var characteristics = new double[characteristicLinkIds.Length];
                    for (int k = 0; k < characteristicLinkIds.Length; k++)
                    {
                        Notation notation = notations[k];
                        long sequenceId;
                        if (matters[matterId].Nature == Nature.Literature)
                        {
                            Language language = languages[k];
                            Translator? translator = translators[k];

                            sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId &&
                                                                           l.Notation == notation
                                                                           && l.Language == language
                                                                           && translator == l.Translator).Id;
                        }
                        else
                        {
                            sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                        }

                        Chain sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);
                        for (int l = 0; l < iterationsCount; l++)
                        {
                            for (int j = 0; j < transformationsSequence.Length; j++)
                            {
                                sequence = transformationsSequence[j] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                                     : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[j]));
                            }
                        }

                        int characteristicLinkId = characteristicLinkIds[k];
                        Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                        FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);

                        IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                        characteristics[k] = calculator.Calculate(sequence, link);
                    }

                    mattersCharacteristics[i] = new { matterName = matters[matterId].Name, characteristics };
                }

                var characteristicNames = new string[characteristicLinkIds.Length];
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k], notations[k]);
                }

                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                for (int i = 0; i < characteristicNames.Length; i++)
                {
                    characteristicsList[i] = new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    };
                }

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationsSequence.Length; i++)
                {
                    transformations.Add(i, transformationsSequence[i].GetDisplayValue());
                }

                var result = new Dictionary<string, object>
                                 {
                                     { "characteristics", mattersCharacteristics },
                                     { "characteristicNames", characteristicNames },
                                     { "characteristicsList", characteristicsList },
                                     { "transformationsList", transformations },
                                     { "iterationsCount", iterationsCount }
                                 };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
