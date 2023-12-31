namespace Libiada.Web.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.DataTransformers;
    using LibiadaCore.Extensions;
    using LibiadaCore.Music;

    using Libiada.Web.Helpers;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Libiada.Database.Models.Repositories.Sequences;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Authorization;
    using Libiada.Web.Tasks;

    /// <summary>
    /// The order transformation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationCharacteristicsDynamicVisualizationController : AbstractResultController
    {
        private readonly ILibiadaDatabaseEntitiesFactory dbFactory;
        private readonly IViewDataHelper viewDataHelper;
        private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
        private readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationCharacteristicsDynamicVisualizationController"/> class.
        /// </summary>
        public OrderTransformationCharacteristicsDynamicVisualizationController(ILibiadaDatabaseEntitiesFactory dbFactory,
                                                                                IViewDataHelper viewDataHelper,
                                                                                ITaskManager taskManager,
                                                                                IFullCharacteristicRepository characteristicTypeLinkRepository,
                                                                                Cache cache)
            : base(TaskType.OrderTransformationCharacteristicsDynamicVisualization, taskManager)
        {
            this.dbFactory = dbFactory;
            this.viewDataHelper = viewDataHelper;
            this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
            this.cache = cache;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            Dictionary<string, object> data = viewDataHelper.FillViewData(CharacteristicCategory.Full, 1, int.MaxValue, "Calculate");

            var transformations = Extensions.EnumExtensions.GetSelectList<OrderTransformation>();
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
        /// <param name="trajectory">
        /// Reading trajectory for images.
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
            bool? sequentialTransfer,
            ImageOrderExtractor? trajectory)
        {
            return CreateTask(() =>
            {
                var commonSequenceRepository = new CommonSequenceRepository(dbFactory, cache);
                var mattersCharacteristics = new object[matterIds.Length];
                Array.Sort(matterIds);
                Dictionary<long, Matter> matters = cache.Matters.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);

                for (int i = 0; i < matterIds.Length; i++)
                {
                    long matterId = matterIds[i];
                    long sequenceId = commonSequenceRepository.GetSequenceIds(new[] { matterId },
                                                                              notation,
                                                                              language,
                                                                              translator,
                                                                              pauseTreatment,
                                                                              sequentialTransfer,
                                                                              trajectory).Single();

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
