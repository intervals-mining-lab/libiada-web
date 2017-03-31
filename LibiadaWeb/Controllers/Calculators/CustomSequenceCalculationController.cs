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

    using Newtonsoft.Json;

    /// <summary>
    /// The quick calculation controller.
    /// </summary>
    [Authorize]
    public class CustomSequenceCalculationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly CharacteristicLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceCalculationController"/> class.
        /// </summary>
        public CustomSequenceCalculationController() : base("Custom sequence calculation")
        {
            db = new LibiadaWebEntities();
            characteristicTypeLinkRepository = new CharacteristicLinkRepository(db);
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

            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "characteristicTypes", viewDataHelper.GetFullCharacteristicTypes() }
                });

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="characteristicTypeLinkIds">
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
        public ActionResult Index(int[] characteristicTypeLinkIds, string[] customSequences, bool localFile, HttpPostedFileBase[] file)
        {
            return Action(() =>
                {
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
                            names[i] = "Custom sequence " + (i + 1) + ". Length: " + customSequences[i].Length;
                        }
                    }

                    var characteristics = new double[sequences.Length, characteristicTypeLinkIds.Length];

                    for (int j = 0; j < sequences.Length; j++)
                    {
                        for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                        {
                            var chain = new Chain(sequences[j]);

                            Link link = characteristicTypeLinkRepository.GetLinkForFullCharacteristic(characteristicTypeLinkIds[k]);
                            FullCharacteristic characteristic = characteristicTypeLinkRepository.GetFullCharacteristic(characteristicTypeLinkIds[k]);
                            IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                            characteristics[j, k] = calculator.Calculate(chain, link);
                        }
                    }

                    List<string> characteristicNames = characteristicTypeLinkIds.Select(c => characteristicTypeLinkRepository.GetFullCharacteristicName(c)).ToList();

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
