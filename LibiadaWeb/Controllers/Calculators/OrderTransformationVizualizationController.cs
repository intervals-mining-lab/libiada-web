using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OrderTransformationVisualizationController : AbstractResultController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderTransformationVisualizationController"/> class.
        /// </summary>
        public OrderTransformationVisualizationController() : base(TaskType.OrderTransformationVisualization)
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
                var orderTransformer = new OrderTransformer();
                orderTransformer.CalculateTransformations(length);

                var transformationsSelectList = new List<SelectListItem> { new SelectListItem { Value = 0.ToString(), Text = "All" } };
                transformationsSelectList.AddRange(EnumHelper.GetSelectList(typeof(OrderTransformation)));

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
}
