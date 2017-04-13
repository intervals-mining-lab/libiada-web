namespace LibiadaWeb.Models.CalculatorsData
{
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic link data.
    /// </summary>
    public class LinkSelectListItem : SelectListItem
    {
        /// <summary>
        /// The value.
        /// </summary>
        public readonly int CharacteristicLinkId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkSelectListItem"/> class.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public LinkSelectListItem(int characteristicLinkId, string value, string text)
        {
            CharacteristicLinkId = characteristicLinkId;
            Value = value;
            Text = text;
        }
    }
}
