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
        public CustomSequenceCalculationController()
            : base("CustomSequenceCalculation", "Custom sequence calculation")
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

            ViewBag.data = new Dictionary<string, object>
                {
                    { "characteristicTypes", viewDataHelper.GetCharacteristicTypes(filter) }
                };
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
        public ActionResult Index(int[] characteristicTypeLinkIds, string[] customSequences, bool localFile, HttpPostedFileBase[] file)
        {
            return Action(() =>
                {
                    var sequences = new List<string>();
                    var names = new List<string>();

                    if (localFile)
                    {
                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            var sequenceStream = FileHelper.GetFileStream(file[i]);
                            var fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                            sequences.Add(fastaSequence.ConvertToString());
                            names.Add(fastaSequence.ID);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < customSequences.Length; i++)
                        {
                            sequences.Add(customSequences[i]);
                            names.Add("Custom sequence " + (i + 1) + ". Length: " + customSequences[i].Length);
                        }
                    }

                    var characteristics = new List<List<double>>();

                    foreach (var sequence in sequences)
                    {
                        characteristics.Add(new List<double>());
                        foreach (int characteristicTypeLinkId in characteristicTypeLinkIds)
                        {
                            var chain = new Chain(sequence);

                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                            var calculator = CalculatorsFactory.CreateFullCalculator(className);

                            characteristics.Last().Add(calculator.Calculate(chain, link));
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
