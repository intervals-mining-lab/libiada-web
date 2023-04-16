namespace Libiada.Web.Models.CalculatorsData
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The characteristic types data.
    /// </summary>
    public class CharacteristicSelectListItem : SelectListItem
    {
        /// <summary>
        /// The characteristic links.
        /// </summary>
        public readonly List<SelectListItem> Links;

        /// <summary>
        /// The arrangement types.
        /// </summary>
        public readonly List<SelectListItem> ArrangementTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicSelectListItem"/> class.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="links">
        /// Applicable to characteristic links.
        /// </param>
        /// <param name="arrangementTypes">
        /// Applicable to characteristic arrangement types.
        /// </param>
        public CharacteristicSelectListItem(int value, string text, List<SelectListItem> links, List<SelectListItem> arrangementTypes)
        {
            Value = value.ToString();
            Text = text;
            Links = links;
            ArrangementTypes = arrangementTypes;
        }
    }
}
