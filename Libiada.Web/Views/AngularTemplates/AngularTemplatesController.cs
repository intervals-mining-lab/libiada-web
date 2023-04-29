namespace Libiada.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller for partial views.
    /// Needed for angular templates.
    /// </summary>
    public class AngularTemplatesController : Controller
    {
        /// <summary>
        /// The matters table partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _MattersTable() => PartialView();

        /// <summary>
        /// The scroll jumper partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _ScrollJumper() => PartialView();

        /// <summary>
        /// The loading Window partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _LoadingWindow() => PartialView();

        /// <summary>
        /// The Characteristic partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _Characteristic() => PartialView();

        /// <summary>
        /// Characteristics nature parameters partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _CharacteristicNatureParams() => PartialView();

        /// <summary>
        /// The Characteristics partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _Characteristics() => PartialView();

        /// <summary>
        /// The characteristics without notation.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _CharacteristicsWithoutNotation() => PartialView();

        /// <summary>
        /// The Order Transformations partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public IActionResult _OrderTransformations() => PartialView();
    }
}
