// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LibiadaRazorViewEngine.cs" company="">
//   
// </copyright>
// <summary>
//   The libiada razor view engine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Web.Mvc;

namespace LibiadaWeb
{
    /// <summary>
    /// The libiada razor view engine.
    /// </summary>
    public class LibiadaRazorViewEngine : RazorViewEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibiadaRazorViewEngine"/> class.
        /// </summary>
        public LibiadaRazorViewEngine()
            : base()
        {
            this.ViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml", 
                    "~/Views/Shared/{0}.cshtml", 
                    "~/Views/Chains/{1}/{0}.cshtml", 
                    "~/Views/Calculators/{1}/{0}.cshtml", 
                    "~/Views/Catalogs/{1}/{0}.cshtml", 
                    "~/Views/Characteristics/{1}/{0}.cshtml"
                };

            this.PartialViewLocationFormats = this.ViewLocationFormats;
            this.ViewLocationFormats = this.ViewLocationFormats;
        }
    }
}