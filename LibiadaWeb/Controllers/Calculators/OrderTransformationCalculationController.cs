namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.DataTransformers;
    using LibiadaCore.Extensions;
    using LibiadaCore.Music;

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
        /// <param name="pauseTreatments">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfers">
        /// Sequential transfer flag used in music sequences.
        /// </param>
        /// <param name="trajectories">
        /// Reading trajectories for images.
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
            Translator?[] translators,
            PauseTreatment[] pauseTreatments,
            bool[] sequentialTransfers,
            ImageOrderExtractor[] trajectories)
        {
            return CreateTask(() =>
            {
                Dictionary<long, string> mattersNames = Cache.GetInstance().Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);
                Chain[][] sequences = new Chain[matterIds.Length][];

                using (var db = new LibiadaWebEntities())
                {
                    var commonSequenceRepository = new CommonSequenceRepository(db);
                    long[][] sequenceIds = commonSequenceRepository.GetSequenceIds(matterIds, notations, languages, translators, pauseTreatments, sequentialTransfers, trajectories);
                    for (int i = 0; i < matterIds.Length; i++)
                    {
                        sequences[i] = new Chain[characteristicLinkIds.Length];
                        for (int j = 0; j < characteristicLinkIds.Length; j++)
                        {
                            sequences[i][j] = commonSequenceRepository.GetLibiadaChain(sequenceIds[i][j]);
                        }
                    }
                }

                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                var mattersCharacteristics = new object[matterIds.Length];
                matterIds = matterIds.OrderBy(m => m).ToArray();

                for (int i = 0; i < matterIds.Length; i++)
                {
                    long matterId = matterIds[i];
                    var characteristics = new double[characteristicLinkIds.Length];
                    for (int j = 0; j < characteristicLinkIds.Length; j++)
                    {
                        Notation notation = notations[j];


                        Chain sequence = sequences[i][j];
                        for (int l = 0; l < iterationsCount; l++)
                        {
                            for (int k = 0; k < transformationsSequence.Length; k++)
                            {
                                sequence = transformationsSequence[k] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                                     : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[k]));
                            }
                        }

                        int characteristicLinkId = characteristicLinkIds[j];
                        Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                        FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);

                        IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);
                        characteristics[j] = calculator.Calculate(sequence, link);
                    }

                    mattersCharacteristics[i] = new { matterName = mattersNames[matterId], characteristics };
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

                var transformations = transformationsSequence.Select(ts => ts.GetDisplayValue());

                var result = new Dictionary<string, object>
                {
                    { "characteristics", mattersCharacteristics },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList },
                    { "transformationsList", transformations },
                    { "iterationsCount", iterationsCount }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
