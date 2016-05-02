using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bio.Extensions;
using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics;
using LibiadaCore.Core.Characteristics.Calculators;
using LibiadaCore.Misc;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Account;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Sequences;
using Newtonsoft.Json;

namespace LibiadaWeb.Controllers.Calculators
{
    public class CustomSequenceOrderTransformationCalculationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationCalculationController"/> class.
        /// </summary>
        public CustomSequenceOrderTransformationCalculationController(): base("Custom sequences order transformation/derivative characteristics calculation")
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

            var data = new Dictionary<string, object>
                {
                    { "characteristicTypes", viewDataHelper.GetCharacteristicTypes(filter) }
                };

            var transformationLinks = new[] { LibiadaCore.Core.Link.Start, LibiadaCore.Core.Link.End, LibiadaCore.Core.Link.CycleStart, LibiadaCore.Core.Link.CycleEnd };
            transformationLinks = transformationLinks.OrderBy(n => (int)n).ToArray();
            data.Add("transformationLinks", transformationLinks.ToSelectList());
            
            var operations = new List<SelectListItem> { new SelectListItem { Text = "Dissimilar", Value = 1.ToString() }, new SelectListItem { Text = "Higher order", Value = 2.ToString() } };
            data.Add("operations", operations);
            
            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="transformationLinkIds">
        /// The transformation link ids.
        /// </param>
        /// <param name="transformationIds">
        /// The transformation ids.
        /// </param>
        /// <param name="iterationsCount">
        /// Number of transformations iterations.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type link ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            int[] transformationLinkIds, 
            int[] transformationIds,
            int iterationsCount,
            int[] characteristicTypeLinkIds,
            string[] customSequences, 
            bool localFile, 
            HttpPostedFileBase[] file)
        {
            return Action(() =>
            {
                var db = new LibiadaWebEntities();
                var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
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
                        var sequence = new Chain(sequences[j]);
                        for (int l = 0; l < iterationsCount; l++)
                        {
                            for (int w = 0; w < transformationIds.Length; w++)
                            {
                                if (transformationIds[w] == 1)
                                {
                                    sequence = DissimilarChainFactory.Create(sequence);
                                }
                                else
                                {
                                    sequence = HighOrderFactory.Create(sequence, (LibiadaCore.Core.Link)transformationLinkIds[w]);
                                }
                            }
                        }
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkIds[k]);
                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkIds[k]).ClassName;
                        var calculator = CalculatorsFactory.CreateFullCalculator(className);

                        characteristics[j, k] = calculator.Calculate(sequence, link);
                    }
                }

                var characteristicNames = characteristicTypeLinkIds.Select(c => characteristicTypeLinkRepository.GetCharacteristicName(c)).ToArray();

                var characteristicsList = new SelectListItem[characteristicTypeLinkIds.Length];
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
                for (int i = 0; i < transformationIds.Length; i++)
                {
                    var link = ((LibiadaCore.Core.Link)transformationLinkIds[i]).GetDisplayValue();
                    transformations.Add(i, transformationIds[i] == 1 ? "dissimilar" : "higher order " + link);
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