namespace LibiadaWeb.Models.CalculatorsData
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic types data.
    /// </summary>
    public class CharacteristicTypeData : SelectListItem
    {
        /// <summary>
        /// The characteristic links.
        /// </summary>
        public readonly List<LinkSelectListItem> CharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeData"/> class.
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
        public CharacteristicTypeData(int value, string text, List<LinkSelectListItem> characteristicLinks)
        {
            Value = value.ToString();
            Text = text;
            CharacteristicLinks = characteristicLinks;
        }
    }
}
