using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using Libiada.Database.Tasks;

    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Authorization;
    using LibiadaWeb.Tasks;

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
                var orderTransformer = new OrderTransformer();
                orderTransformer.CalculateTransformations(length);

                var transformationsSelectList = new List<SelectListItem> { new SelectListItem { Value = 0.ToString(), Text = "All" } };
                transformationsSelectList.AddRange(Extensions.EnumExtensions.GetSelectList<OrderTransformation>());

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
