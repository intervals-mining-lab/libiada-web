using System;
using System.Web.Mvc.Html;
using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models.CalculatorsData;
using LibiadaWeb.Models.Repositories.Catalogs;

namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using SequenceGenerator;

    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderCalculationController : AbstractResultController
    {
        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly FullCharacteristicRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCalculationController"/> class.
        /// </summary>
        public OrderCalculationController() : base(TaskType.OrderCalculation)
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
            var imageTransformers = EnumHelper.GetSelectList(typeof(ImageTransformer));

            using (var db = new LibiadaWebEntities())
            {
                var viewDataHelper = new ViewDataHelper(db);
                Dictionary<string, object> viewData = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);
                ViewBag.data = JsonConvert.SerializeObject(viewData);
                return View();
            }
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="alphabetCardinality">
        /// The alphabet cardinality.
        /// </param>
        /// <param name="generateStrict">
        /// The generate strict.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(int length, int alphabetCardinality, bool generateStrict, short[] characteristicLinkIds)
        {
            return CreateTask(() =>
            {
                var orderGenerator = new OrderGenerator();
                var orders = generateStrict ?
                                 orderGenerator.StrictGenerateOrders(length, alphabetCardinality) :
                                 orderGenerator.GenerateOrders(length, alphabetCardinality);

                var characteristics = new double[orders.Count][];
                var sequencesCharacteristics = new List<SequenceCharacteristics>();
                for (int i = 0; i < orders.Count; i++)
                {
                    sequencesCharacteristics.Add(new SequenceCharacteristics());
                }
                for (int j = 0; j < orders.Count; j++)
                {
                    var sequence = new Chain(orders[j].Select(Convert.ToInt16).ToArray());
                    sequence.FillIntervalManagers();
                    characteristics[j] = new double[characteristicLinkIds.Length];
                    for (int k = 0; k < characteristicLinkIds.Length; k++)
                    {

                        Link link = characteristicTypeLinkRepository.GetLinkForCharacteristic(characteristicLinkIds[k]);
                        FullCharacteristic characteristic = characteristicTypeLinkRepository.GetCharacteristic(characteristicLinkIds[k]);
                        IFullCalculator calculator = FullCalculatorsFactory.CreateCalculator(characteristic);

                        characteristics[j][k] = calculator.Calculate(sequence, link);
                    }

                    sequencesCharacteristics[j] = new SequenceCharacteristics
                    {
                        MatterName = String.Join(",", orders[j].Select(n => n.ToString()).ToArray()),
                        Characteristics = characteristics[j]
                    };
                }

                var characteristicNames = new string[characteristicLinkIds.Length];
                var characteristicsList = new SelectListItem[characteristicLinkIds.Length];
                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    characteristicNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k]);
                    characteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = characteristicNames[k],
                        Selected = false
                    };
                }

                sequencesCharacteristics.RemoveAll(el => el.Characteristics.Any(v => Double.IsInfinity(v) ||
                                                                                            Double.IsNaN(v) ||
                                                                                            Double.IsNegativeInfinity(v) ||
                                                                                            Double.IsPositiveInfinity(v)));
                var index = new int[characteristicsList.Length];
                for (int i = 0; i < index.Length; i++)
                {
                    index[i] = i;
                }
                var result = new Dictionary<string, object>
                {
                    { "characteristics", sequencesCharacteristics.ToArray() },
                    { "characteristicNames", characteristicNames },
                    { "characteristicsList", characteristicsList },
                    { "characteristicsIndex", index }
                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(result) }
                };
            });
        }
    }
}