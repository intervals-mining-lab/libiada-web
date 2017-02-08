using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Account;
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
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceCalculationController"/> class.
        /// </summary>
        public CustomSequenceCalculationController() : base("Custom sequence calculation")
        {
            db = new LibiadaWebEntities();
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
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

            Func<CharacteristicType, bool> filter;
            if (UserHelper.IsAdmin())
            {
                filter = c => c.FullSequenceApplicable;
            }
            else
            {
                filter = c => c.FullSequenceApplicable && Aliases.UserAvailableCharacteristics.Contains((Aliases.CharacteristicType)c.Id);
            }

            ViewBag.data = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    { "characteristicTypes", viewDataHelper.GetCharacteristicTypes(filter) }
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
                            var sequenceStream = FileHelper.GetFileStream(file[i]);
                            var fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
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

                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                            var calculator = FullCalculatorsFactory.CreateFullCalculator(className);

                            characteristics[j, k] = calculator.Calculate(chain, link);
                        }
                    }

                    var characteristicNames = characteristicTypeLinkIds.Select(c => characteristicTypeLinkRepository.GetCharacteristicName(c)).ToList();

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
