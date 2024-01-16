namespace Libiada.Web.Models.CalculatorsData;

/// <summary>
/// The arrangement type select list item.
/// </summary>
public class ArrangementTypeSelectListItem : SelectListItem
{
    /// <summary>
    /// The value.
    /// </summary>
    public readonly int CharacteristicLinkId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrangementTypeSelectListItem"/> class.
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
    public ArrangementTypeSelectListItem(int characteristicLinkId, string value, string text)
    {
        CharacteristicLinkId = characteristicLinkId;
        Value = value;
        Text = text;
    }
}