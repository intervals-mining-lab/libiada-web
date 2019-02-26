namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;
    using LibiadaCore.Misc;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;
    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// The custom sequence order transformation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CustomSequenceOrderTransformationCalculationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceOrderTransformationCalculationController"/> class.
        /// </summary>
        public CustomSequenceOrderTransformationCalculationController() : base(TaskType.CustomSequenceOrderTransformationCalculation)
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
            var data = new Dictionary<string, object>
                {
                    { "characteristicTypes", FullCharacteristicRepository.Instance.GetCharacteristicTypes() }
                };

            var transformations = EnumHelper.GetSelectList(typeof(OrderTransformation));
            data.Add("transformations", transformations);

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
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
            HttpPostedFileBase[] file)
        {
            return CreateTask(() =>
            {
                var characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
                int sequencesCount = localFile ? Request.Files.Count : customSequences.Length;
                var sequences = new string[sequencesCount];
                var names = new string[sequencesCount];

                for (int i = 0; i < sequencesCount; i++)
                {
                    if (localFile)
                    {
                        Stream sequenceStream = FileHelper.GetFileStream(file[i]);
                        ISequence fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                        sequences[i] = fastaSequence.ConvertToString();
                        names[i] = fastaSequence.ID;
                    }
                    else
                    {
                        sequences[i] = customSequences[i];
                        names[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                    }
                }

                var characteristics = new double[sequences.Length, characteristicLinkIds.Length];

                for (int j = 0; j < sequences.Length; j++)
                {
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

                        characteristics[j, k] = calculator.Calculate(sequence, link);
                    }
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

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationsSequence.Length; i++)
                {
                    transformations.Add(i, transformationsSequence[i].GetDisplayValue());
                }

                var result = new Dictionary<string, object>
                                 {
                                     { "characteristics", characteristics },
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
