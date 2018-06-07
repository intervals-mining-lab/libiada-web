namespace LibiadaWeb.Models.CalculatorsData
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic types data.
    /// </summary>
    public class CharacteristicSelectListItem : SelectListItem
    {
        /// <summary>
        /// The characteristic links.
        /// </summary>
        public readonly List<SelectListItem> CharacteristicLinks;

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
        /// <param name="characteristicLinks">
        /// The characteristic links.
        /// </param>
        /// <param name="arrangementTypes">
        /// The arrangement types.
        /// </param>
        public CharacteristicSelectListItem(int value, string text, List<SelectListItem> characteristicLinks, List<SelectListItem> arrangementTypes)
        {
            Value = value.ToString();
            Text = text;
            CharacteristicLinks = characteristicLinks;
            ArrangementTypes = arrangementTypes;
        }
    }
}
