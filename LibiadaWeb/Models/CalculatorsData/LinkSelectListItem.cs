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
        public readonly int CharacteristicTypeLinkId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkSelectListItem"/> class.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public LinkSelectListItem(int characteristicTypeLinkId, string value, string text)
        {
            CharacteristicTypeLinkId = characteristicTypeLinkId;
            Value = value;
            Text = text;
        }
    }
}
