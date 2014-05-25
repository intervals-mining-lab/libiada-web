// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterConfig.cs" company="">
//   
// </copyright>
// <summary>
//   The filter config.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Web;
using System.Web.Mvc;

namespace LibiadaWeb
{
    /// <summary>
    /// The filter config.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// The register global filters.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
