namespace LibiadaWeb.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Controller for partial views.
    /// Needed for angular templates.
    /// </summary>
    public class PartialController : Controller
    {
        /// <summary>
        /// The matters table partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public PartialViewResult _MattersTable() => PartialView();

        /// <summary>
        /// The scroll jumper partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public PartialViewResult _ScrollJumper() => PartialView();

        /// <summary>
        /// The loading Window partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public PartialViewResult _LoadingWindow() => PartialView();

        /// <summary>
        /// The Characteristic partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public PartialViewResult _Characteristic() => PartialView();

        /// <summary>
        /// The Characteristics partial view.
        /// </summary>
        /// <returns>
        /// The <see cref="PartialViewResult"/>.
        /// </returns>
        public PartialViewResult _Characteristics() => PartialView();
    }
}
