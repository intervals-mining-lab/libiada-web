namespace LibiadaWeb.Models.Calculators
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic data.
    /// </summary>
    public class CharacteristicData : SelectListItem
    {
        /// <summary>
        /// The characteristic links.
        /// </summary>
        public List<CharacteristicLinkData> CharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicData"/> class.
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
        public CharacteristicData(int value, string text, List<CharacteristicLinkData> characteristicLinks)
        {
            Value = value.ToString();
            Text = text;
            CharacteristicLinks = characteristicLinks;
        }
    }
}