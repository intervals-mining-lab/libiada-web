namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Bio.Extensions;
    using Bio.Util;

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
                var characteristicIds = new List<int>
                                            {
                                                Aliases.CharacteristicType.ATSkew, 
                                                Aliases.CharacteristicType.AlphabetCardinality, 
                                                Aliases.CharacteristicType.AverageRemoteness, 
                                                Aliases.CharacteristicType.GCRatio, 
                                                Aliases.CharacteristicType.GCSkew, 
                                                Aliases.CharacteristicType.GCToATRatio, 
                                                Aliases.CharacteristicType.IdentificationInformation, 
                                                Aliases.CharacteristicType.Length, 
                                                Aliases.CharacteristicType.MKSkew, 
                                                Aliases.CharacteristicType.RYSkew, 
                                                Aliases.CharacteristicType.SWSkew
                                            };
                filter = c => c.FullSequenceApplicable && characteristicIds.Contains(c.Id);
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
                    string[] sequences;
                    string[] names;

                    if (localFile)
                    {
                        sequences = new string[Request.Files.Count];
                        names = new string[customSequences.Length];
                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            var sequenceStream = FileHelper.GetFileStream(file[i]);
                            var fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                            sequences[i] = fastaSequence.ConvertToString();
                            names[i] = fastaSequence.ID;
                        }
                    }
                    else
                    {
                        sequences = new string[customSequences.Length];
                        names = new string[customSequences.Length];
                        for (int i = 0; i < customSequences.Length; i++)
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
                            var calculator = CalculatorsFactory.CreateFullCalculator(className);

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
