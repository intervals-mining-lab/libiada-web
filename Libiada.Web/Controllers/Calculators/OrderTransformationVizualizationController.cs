namespace Libiada.Web.Controllers.Calculators;

using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Models;

/// <summary>
/// Calculates distribution of sequences by order.
/// </summary>
[Authorize(Roles = "Admin")]
public class OrderTransformationVisualizationController : AbstractResultController
{

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTransformationVisualizationController"/> class.
    /// </summary>
    public OrderTransformationVisualizationController(ITaskManager taskManager) : base(TaskType.OrderTransformationVisualization, taskManager)
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
        ViewBag.data = "{}";
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="length">
    /// The length.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    [HttpPost]
    public ActionResult Index(int length)
    {
        return CreateTask(() =>
        {
            OrderTransformer orderTransformer = new();
            orderTransformer.CalculateTransformations(length);

            List<SelectListItem> transformationsSelectList =
            [
                new() { Value = 0.ToString(), Text = "All" },
                .. Extensions.EnumExtensions.GetSelectList<OrderTransformation>(),
            ];

            var result = new Dictionary<string, object>
            {
                { "transformationsData", orderTransformer.TransformationsData },
                { "orders", orderTransformer.Orders },
                { "transformationsList", transformationsSelectList },

            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
