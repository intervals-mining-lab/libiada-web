namespace LibiadaWeb
{
    using System.Web.Mvc;

    /// <summary>
    /// The libiada razor view engine.
    /// </summary>
    public class LibiadaRazorViewEngine : RazorViewEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibiadaRazorViewEngine"/> class.
        /// </summary>
        public LibiadaRazorViewEngine() : base()
        {
            ViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml",
                    "~/Views/Sequences/{1}/{0}.cshtml",
                    "~/Views/Calculators/{1}/{0}.cshtml",
                    "~/Views/Catalogs/{1}/{0}.cshtml",
                    "~/Views/Characteristics/{1}/{0}.cshtml",
                    "~/Views/Partial/{0}.cshtml"
                };

            PartialViewLocationFormats = ViewLocationFormats;
            ViewLocationFormats = ViewLocationFormats;
        }
    }
}
