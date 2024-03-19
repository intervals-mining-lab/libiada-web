namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Extensions;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.SequenceGenerator;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;

/// <summary>
/// Calculates accordance of orders by intervals distributions.
/// </summary>
[Authorize(Roles = "Admin")]
public class IntervalsCharacteristicsDistributionController : AbstractResultController
{
    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntervalsCharacteristicsDistributionController"/> class.
    /// </summary>
    public IntervalsCharacteristicsDistributionController(IViewDataHelper viewDataHelper,
                                                          ITaskManager taskManager,
                                                          IFullCharacteristicRepository characteristicTypeLinkRepository)
        : base(TaskType.IntervalsCharacteristicsDistribution, taskManager)
    {
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.viewDataHelper = viewDataHelper;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        var imageTransformers = Extensions.EnumExtensions.GetSelectList<ImageTransformer>();

        Dictionary<string, object> viewData = viewDataHelper.GetCharacteristicsData(CharacteristicCategory.Full);
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
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
    public ActionResult Index(int length, int alphabetCardinality, int generateStrict, short[] characteristicLinkIds)
    {
        // TODO: Reafctor all of this
        return CreateTask(() =>
        {
            var orderGenerator = new OrderGenerator();
            List<int[]> orders = generateStrict switch
            {
                0 => orderGenerator.StrictGenerateOrders(length, alphabetCardinality),
                1 => orderGenerator.GenerateOrders(length, alphabetCardinality),
                _ => throw new ArgumentException($"Invalid type of order generator param: {generateStrict}"),
            };
            CustomSequencesCharacterisitcsCalculator calculator = new(characteristicTypeLinkRepository, characteristicLinkIds);
            var characteristics = calculator.Calculate(orders.Select(order => new Chain(order))).ToList();
            List<SequenceCharacteristics> sequencesCharacteristics = [];
            for (int i = 0; i < orders.Count; i++)
            {
                sequencesCharacteristics.Add(new SequenceCharacteristics
                {
                    MatterName = string.Join(",", orders[i].Select(n => n.ToString()).ToArray()),
                    Characteristics = characteristics[i]
                });
            }

            sequencesCharacteristics.RemoveAll(el => el.Characteristics.Any(v => double.IsInfinity(v) ||
                                                                                 double.IsNaN(v) ||
                                                                                 double.IsNegativeInfinity(v) ||
                                                                                 double.IsPositiveInfinity(v)));

            string[] characteristicNames = new string[characteristicLinkIds.Length];
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

            var index = Enumerable.Range(0, characteristicLinkIds.Length);

            Dictionary<string, Dictionary<IntervalsDistribution, Dictionary<int[], SequenceCharacteristics>>> resultIntervals = [];
            foreach (Link link in EnumExtensions.ToArray<Link>())
            {
                if (link == Link.NotApplied)
                {
                    continue;
                }
                var accordance = IntervalsDistributionExtractor.GetOrdersIntervalsDistributionsAccordance(orders.ToArray(), link);
                Dictionary<IntervalsDistribution, Dictionary<int[], SequenceCharacteristics>> resultAccordance = [];
                foreach (var element in accordance)
                {
                    resultAccordance.Add(element.Key, []);
                    foreach (int[] order in element.Value)
                    {
                        // TODO refactor this
                        SequenceCharacteristics characteristic = sequencesCharacteristics
                                          .FirstOrDefault(el => el.MatterName.SequenceEqual(string.Join(",", order.Select(n => n.ToString()).ToArray())));
                        resultAccordance[element.Key].Add(order, characteristic);
                    }
                }

                resultIntervals.Add(link.GetDisplayValue(), resultAccordance);
            }


            var list = Extensions.EnumExtensions.GetSelectList<Link>().ToList();
            list.RemoveAt(0);
            var result = new Dictionary<string, object>
            {
                { "result", resultIntervals.Select(r => new
                    {
                        link = r.Key,
                        accordance = r.Value.Select(d => new {
                            distributionIntervals = d.Key.Distribution.Select(pair => new
                            {
                                interval = pair.Key,
                                count = pair.Value
                            }).ToArray(),
                            orders = d.Value.Select(o => new
                            {
                                order = o.Key,
                                characteristics = o.Value
                            })
                        })
                    })
                },
                {"linkList", list },
                { "characteristicNames", characteristicNames },
                { "characteristicsList", characteristicsList },
                { "characteristicsIndex", index }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
