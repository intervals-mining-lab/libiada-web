namespace Libiada.Web.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using Bio;
    using Bio.Extensions;

    using Libiada.Database;
    using Libiada.Database.Models.CalculatorsData;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Libiada.Database.Tasks;
    using Libiada.Database.Helpers;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.DataTransformers;
    using LibiadaCore.Extensions;

    using Libiada.Web.Helpers;

    using Newtonsoft.Json;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;
    using Libiada.Web.Tasks;


    /// <summary>
    /// The custom sequence order transformation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CustomSequenceOrderTransformationCalculationController : AbstractResultController
    {
        private readonly IViewDataHelper viewDataHelper;
        private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceOrderTransformationCalculationController"/> class.
        /// </summary>
        public CustomSequenceOrderTransformationCalculationController(IViewDataHelper viewDataHelper, 
                                                                      ITaskManager taskManager,
                                                                      IFullCharacteristicRepository characteristicTypeLinkRepository)
            : base(TaskType.CustomSequenceOrderTransformationCalculation, taskManager)
        {
            this.viewDataHelper = viewDataHelper;
            this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var data = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);

            var transformations = Extensions.EnumExtensions.GetSelectList<OrderTransformation>();
            data.Add("transformations", transformations);

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="transformationsSequence">
        /// The transformation ids.
        /// </param>
        /// <param name="iterationsCount">
        /// Number of transformations iterations.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="customSequences">
        /// Custom sequences input by user.
        /// </param>
        /// <param name="localFile">
        /// Local file flag.
        /// </param>
        /// <param name="file">
        /// Sequences as fasta files.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            OrderTransformation[] transformationsSequence,
            int iterationsCount,
            short[] characteristicLinkIds,
            string[] customSequences,
            bool localFile,
            IFormFileCollection file)
        {
            return CreateTask(() =>
            {
                int sequencesCount = localFile ? file.Count : customSequences.Length;
                var sequences = new string[sequencesCount];
                var sequencesNames = new string[sequencesCount];

                for (int i = 0; i < sequencesCount; i++)
                {
                    if (localFile)
                    {
                        Stream sequenceStream = Helpers.FileHelper.GetFileStream(file[i]);
                        ISequence fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                        sequences[i] = fastaSequence.ConvertToString();
                        sequencesNames[i] = fastaSequence.ID;
                    }
                    else
                    {
                        sequences[i] = customSequences[i];
                        sequencesNames[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                    }
                }

                var sequencesCharacteristics = new SequenceCharacteristics[sequences.Length];
                for (int j = 0; j < sequences.Length; j++)
                {
                    var characteristics = new double[characteristicLinkIds.Length];
                    for (int k = 0; k < characteristicLinkIds.Length; k++)
                    {
                        var sequence = new Chain(sequences[j]);
                        for (int l = 0; l < iterationsCount; l++)
                        {
                            for (int w = 0; w < transformationsSequence.Length; w++)
                            {
                                sequence = transformationsSequence[w] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                                     : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[w]));
                            }
                        }

                        Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[k]);
                        FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[k]);
                        IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                        characteristics[k] = calculator.Calculate(sequence, link);
                    }

                    sequencesCharacteristics[j] = new SequenceCharacteristics
                    {
                        MatterName = sequencesNames[j],
                        Characteristics = characteristics
                    };
                }

                string[] characteristicNames = characteristicLinkIds.Select(c => characteristicTypeLinkRepository.GetCharacteristicName(c)).ToArray();

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
                                     { "characteristics", sequencesCharacteristics },
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
