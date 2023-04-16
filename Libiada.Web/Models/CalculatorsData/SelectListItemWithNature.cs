﻿namespace Libiada.Web.Models.CalculatorsData
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The select list item with nature param.
    /// </summary>
    public class SelectListItemWithNature : SelectListItem
    {
        /// <summary>
        /// Gets or sets the nature param.
        /// </summary>
        public byte Nature { get; set; }
    }
}
