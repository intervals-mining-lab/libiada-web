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

    /// <summary>
    /// The order transformation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationCharacteristicsDynamicVisualizationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationCharacteristicsDynamicVisualizationController"/> class.
        /// </summary>
        public OrderTransformationCharacteristicsDynamicVisualizationController() : base(TaskType.OrderTransformationCharacteristicsDynamicVisualization)
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
        /// <param name="characteristicLinkId">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="notation">
        /// The notation ids.
        /// </param>
        /// <param name="language">
        /// The language ids.
        /// </param>
        /// <param name="translator">
        /// The translator ids.
        /// </param>
        /// <param name="pauseTreatment">
        /// Pause treatment parameters of music sequences.
        /// </param>
        /// <param name="sequentialTransfer">
        /// Sequential transfer flag used in music sequences.
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
            short characteristicLinkId,
            Notation notation,
            Language? language,
            Translator? translator,
            PauseTreatment? pauseTreatment,
            bool? sequentialTransfer)
        {
            return CreateTask(() =>
            {
                var db = new LibiadaWebEntities();
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                var commonSequenceRepository = new CommonSequenceRepository(db);
                var mattersCharacteristics = new object[matterIds.Length];
                matterIds = matterIds.OrderBy(m => m).ToArray();
                Dictionary<long, Matter> matters = Cache.GetInstance().Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);

                for (int i = 0; i < matterIds.Length; i++)
                {
                    long matterId = matterIds[i];
                    long sequenceId;
                    switch (matters[matterId].Nature)
                    {
                        case Nature.Literature:
                            sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId
                                                                        && l.Notation == notation
                                                                        && l.Language == language
                                                                        && l.Translator == translator).Id;
                            break;
                        case Nature.Music:
                            sequenceId = db.MusicSequence.Single(m => m.MatterId == matterId
                                                                   && m.Notation == notation
                                                                   && m.PauseTreatment == pauseTreatment
                                                                   && m.SequentialTransfer == sequentialTransfer).Id;
                            break;
                        default:
                            sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                            break;
                    }

                    Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkId);
                    FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkId);
                    IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                    Chain sequence = commonSequenceRepository.GetLibiadaChain(sequenceId);

                    var characteristics = new double[transformationsSequence.Length * iterationsCount];
                    for (int j = 0; j < iterationsCount; j++)
                    {
                        for (int k = 0; k < transformationsSequence.Length; k++)
                        {
                            sequence = transformationsSequence[k] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                                 : HighOrderFactory.Create(sequence, transformationsSequence[k].GetLink());
                            characteristics[transformationsSequence.Length * j + k] = calculator.Calculate(sequence, link);
                        }
                    }

                    mattersCharacteristics[i] = new { matterName = matters[matterId].Name, characteristics };
                }

                string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);


                var result = new Dictionary<string, object>
                                 {
                                     { "characteristics", mattersCharacteristics },
                                     { "characteristicName", characteristicName },
                                     { "transformationsList", transformationsSequence.Select(ts => ts.GetDisplayValue()) },
                                     { "iterationsCount", iterationsCount }
                                 };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
