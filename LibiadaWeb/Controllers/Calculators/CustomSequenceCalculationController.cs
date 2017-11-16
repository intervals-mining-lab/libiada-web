namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Bio;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;
    using LibiadaCore.Images;
    using SixLabors.ImageSharp;

    /// <summary>
    /// The quick calculation controller.
    /// </summary>
    [Authorize]
    public class CustomSequenceCalculationController : AbstractResultController
    {
        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceCalculationController"/> class.
        /// </summary>
        public CustomSequenceCalculationController() : base(TaskType.CustomSequenceCalculation)
        {
            characteristicTypeLinkRepository = FullCharacteristicRepository.Instance;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var fullCharacteristicRepository = FullCharacteristicRepository.Instance;
            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "characteristicTypes", fullCharacteristicRepository.GetFullCharacteristicTypes() }
                });

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="characteristicLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <param name="customSequences">
        /// The custom sequences.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <param name="file">
        /// The files.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int[] characteristicLinkIds, string[] customSequences, bool localFile, bool fileIsImage, HttpPostedFileBase[] file)
        {
            return CreateTask(() =>
                {
                    int sequencesCount = localFile ? Request.Files.Count : customSequences.Length;
                    var names = new string[sequencesCount];
                    var sequences = new Chain[sequencesCount];

                    for (int i = 0; i < sequencesCount; i++)
                    {
                        if (localFile)
                        {
                            Stream sequenceStream = FileHelper.GetFileStream(file[i]);
                            if (fileIsImage)
                            {
                                var image = Image.Load(sequenceStream);
                                var sequence = ImageProcessor.ProcessImage(image, new IImageTransformer[0], new IMatrixTransformer[0], new LineOrderExtractor());
                                sequences[i] = new Chain(sequence.Building, sequence.Alphabet);
                            }
                            else
                            {
                                ISequence fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                                var stringSequence = fastaSequence.ConvertToString();
                                sequences[i] = new Chain(stringSequence);
                                names[i] = fastaSequence.ID;
                            }
                        }
                        else
                        {
                            sequences[i] = new Chain(customSequences[i]);
                            names[i] = "Custom sequence " + (i + 1) + ". Length: " + customSequences[i].Length;
                        }
                    }

                    var characteristics = new double[sequences.Length, characteristicLinkIds.Length];

                    for (int j = 0; j < sequences.Length; j++)
                    {
                        for (int k = 0; k < characteristicLinkIds.Length; k++)
                        {
                            Link link = characteristicTypeLinkRepository.GetLinkForFullCharacteristic(characteristicLinkIds[k]);
                            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetFullCharacteristic(characteristicLinkIds[k]);
                            IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                            characteristics[j, k] = calculator.Calculate(sequences[j], link);
                        }
                    }

                    List<string> characteristicNames = characteristicLinkIds.Select(c => characteristicTypeLinkRepository.GetFullCharacteristicName(c)).ToList();

                    return new Dictionary<string, object>
                    {
                        { "names", names },
                        { "characteristicNames", characteristicNames },
                        { "characteristics", characteristics }
                    };
                });
        }
    }
}
