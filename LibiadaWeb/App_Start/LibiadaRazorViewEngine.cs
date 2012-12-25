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
                    "~/Views/{1}/{0}.vbhtml",
                    "~/Views/Shared/{0}.cshtml",
                    "~/Views/Shared/{0}.vbhtml",
                    "~/Views/Chains/{1}/{0}.cshtml",
                    "~/Views/Chains/{1}/{0}.vbhtml",
                    "~/Views/Calculators/{1}/{0}.cshtml",
                    "~/Views/Calculators/{1}/{0}.vbhtml",
                    "~/Views/Catalogs/{1}/{0}.cshtml",
                    "~/Views/Catalogs/{1}/{0}.vbhtml"

                };

            PartialViewLocationFormats = ViewLocationFormats;
            ViewLocationFormats = ViewLocationFormats;
        }
    }
}