using System.Web.Mvc;

namespace LibiadaWeb
{
    public class LibiadaRazorViewEngine : RazorViewEngine
    {

        public LibiadaRazorViewEngine()
            : base()
        {
            ViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml",
                    "~/Views/Chains/{1}/{0}.cshtml",
                    "~/Views/Calculators/{1}/{0}.cshtml",
                    "~/Views/Catalogs/{1}/{0}.cshtml",
                    "~/Views/Characteristics/{1}/{0}.cshtml"
                };

            PartialViewLocationFormats = ViewLocationFormats;
            ViewLocationFormats = ViewLocationFormats;
        }
    }
}