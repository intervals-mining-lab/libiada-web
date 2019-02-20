using System;
using System.Threading.Tasks;
using System.Web.Services.Description;
using LibiadaCore.Misc;
using LibiadaWeb.Models;
using LibiadaWeb.Models.CalculatorsData;

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
        public ActionResult Index(int length, bool multiThreading)
        {
            return CreateTask(() =>
            {
                var orderGenerator = new OrderGenerator();
                var orders = orderGenerator.GenerateOrders(length);
                var orderTransformer = new OrderTransformer(orders);
                orderTransformer.TransformateOrders(multiThreading);
                var typesOfTransformationsList = new SelectListItem[orderTransformer.TypesOfTransformations.Length+1];
                typesOfTransformationsList[0] = new SelectListItem
                {
                    Value = 0.ToString(),
                    Text = "All",
                    Selected = false
                };
                for (int i = 0; i < orderTransformer.TypesOfTransformations.Length; i++)
                {
                    typesOfTransformationsList[i+1] = new SelectListItem
                    {
                        Value = (i+1).ToString(),
                        Text = orderTransformer.TypesOfTransformations[i],
                        Selected = false
                    };
                }

                var data = new Dictionary<string, object>
                {
                    { "transformationsData", orderTransformer.TransformationsData},
                    { "orders", orderTransformer.Orders},
                    { "transformationsName", orderTransformer.TypesOfTransformations},
                    { "transformationsList", typesOfTransformationsList},

                };

                return new Dictionary<string, object>
                {
                    { "data", JsonConvert.SerializeObject(data) }
                };
            });
        }

    }
}