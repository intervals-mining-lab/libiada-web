namespace LibiadaWeb.Models.Calculators
{
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic link data.
    /// </summary>
    public class CharacteristicLinkData : SelectListItem
    {
        /// <summary>
        /// The value.
        /// </summary>
        public readonly int CharacteristicTypeLinkId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicLinkData"/> class.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The value.
        /// </param>
        public CharacteristicLinkData(int characteristicTypeLinkId)
        {
            CharacteristicTypeLinkId = characteristicTypeLinkId;
        }
    }
}
