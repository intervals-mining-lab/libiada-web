namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;
    using LibiadaCore.Extensions;
    using LibiadaCore.Misc;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Account;

    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The order transformation calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationCalculationController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationCalculationController"/> class.
        /// </summary>
        public OrderTransformationCalculationController() : base("Order transformation/derivative characteristics calculation")
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
            Func<CharacteristicType, bool> filter;
            if (UserHelper.IsAdmin())
            {
                filter = c => c.FullSequenceApplicable;
            }
            else
            {
                filter = c => c.FullSequenceApplicable && Aliases.UserAvailableCharacteristics.Contains((Aliases.CharacteristicType)c.Id);
            }

            var db = new LibiadaWebEntities();
            var viewDataHelper = new ViewDataHelper(db);
            var data = viewDataHelper.FillViewData(filter, 1, int.MaxValue, "Calculate");

            var transformationLinks = new[] { Link.Start, Link.End, Link.CycleStart, Link.CycleEnd };
            data.Add("transformationLinks", transformationLinks.ToSelectList());

            var operations = new List<SelectListItem>
            {
                new SelectListItem { Text = "Dissimilar", Value = 1.ToString() },
                new SelectListItem { Text = "Higher order", Value = 2.ToString() }
            };
            data.Add("operations", operations);

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
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
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <param name="languages">
        /// The language ids.
        /// </param>
        /// <param name="translators">
        /// The translator ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            Link[] transformationLinkIds,
            int[] transformationIds,
            int iterationsCount,
            int[] characteristicTypeLinkIds,
            Notation[] notations,
            Language[] languages,
            Translator?[] translators)
        {
            return Action(() =>
            {
                var db = new LibiadaWebEntities();
                var characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
                var commonSequenceRepository = new CommonSequenceRepository(db);
                var mattersCharacteristics = new object[matterIds.Length];
                matterIds = matterIds.OrderBy(m => m).ToArray();
                var matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id);

                for (int i = 0; i < matterIds.Length; i++)
                {
                    var matterId = matterIds[i];
                    var characteristics = new List<double>();
                    for (int k = 0; k < notations.Length; k++)
                    {
                        var notation = notations[k];
                        long sequenceId;
                        if (matters[matterId].Nature == Nature.Literature)
                        {
                            Language language = languages[k];
                            Translator? translator = translators[k];

                            sequenceId = db.LiteratureSequence.Single(l => l.MatterId == matterId &&
                                                                           l.Notation == notation
                                                                           && l.Language == language
                                                                           && translator == l.Translator).Id;
                        }
                        else
                        {
                            sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.Notation == notation).Id;
                        }

                        var sequence = commonSequenceRepository.ToLibiadaChain(sequenceId);
                        for (int l = 0; l < iterationsCount; l++)
                        {
                            for (int j = 0; j < transformationIds.Length; j++)
                            {
                                sequence = transformationIds[j] == 1 ? DissimilarChainFactory.Create(sequence)
                                                                     : HighOrderFactory.Create(sequence, transformationLinkIds[j]);
                            }
                        }

                        int characteristicTypeLinkId = characteristicTypeLinkIds[k];
                        var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                        string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;

                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        characteristics.Add(calculator.Calculate(sequence, link));
                    }

                    mattersCharacteristics[i] = new { matterName = matters[matterId].Name, characteristics };
                }

                var characteristicNames = new string[characteristicTypeLinkIds.Length];
                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notations[k]);
                }

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
                    transformations.Add(i, transformationIds[i] == 1 ? "dissimilar" : "higher order " + transformationLinkIds[i].GetDisplayValue());
                }

                var result = new Dictionary<string, object>
                                 {
                                     { "characteristics", mattersCharacteristics },
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
