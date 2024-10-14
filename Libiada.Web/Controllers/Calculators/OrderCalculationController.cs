namespace Libiada.Web.Controllers.Calculators;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;

using Libiada.Database.Tasks;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Database.Models.Repositories.Catalogs;

using Newtonsoft.Json;

using Libiada.SequenceGenerator;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

/// <summary>
/// Calculates distribution of sequences by order.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrderCalculationController : AbstractResultController
{
    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCalculationController"/> class.
    /// </summary>
    public OrderCalculationController(IViewDataHelper viewDataHelper, 
                                      ITaskManager taskManager,
                                      IFullCharacteristicRepository characteristicTypeLinkRepository) 
        : base(TaskType.OrderCalculation, taskManager)
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
    /// <param name="characteristicLinkIds">
    /// CharacteristicLinks ids.
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
            List<int[]> orders = generateStrict ?
                             orderGenerator.StrictGenerateOrders(length, alphabetCardinality) :
                             orderGenerator.GenerateOrders(length, alphabetCardinality);

            double[][] characteristics = new double[orders.Count][];
            List<SequenceCharacteristics> sequencesCharacteristics = [];
            for (int i = 0; i < orders.Count; i++)
            {
                sequencesCharacteristics.Add(new SequenceCharacteristics());
            }
            for (int j = 0; j < orders.Count; j++)
            {
                var sequence = new Chain(orders[j].Select(Convert.ToInt16).ToArray());
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
                    MatterName = string.Join(",", orders[j].Select(n => n.ToString()).ToArray()),
                    Characteristics = characteristics[j]
                };
            }

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

            sequencesCharacteristics.RemoveAll(el => el.Characteristics.Any(v => double.IsInfinity(v) ||
                                                                                 double.IsNaN(v) ||
                                                                                 double.IsNegativeInfinity(v) ||
                                                                                 double.IsPositiveInfinity(v)));
            int[] index = new int[characteristicsList.Length];
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

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}