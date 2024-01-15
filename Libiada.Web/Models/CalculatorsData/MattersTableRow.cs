namespace Libiada.Web.Models.CalculatorsData;

using Libiada.Core.Extensions;

/// <summary>
/// The matters table row.
/// </summary>
public class MattersTableRow : SelectListItemWithNature
{
    /// <summary>
    /// The group.
    /// </summary>
    public new readonly string Group;

    /// <summary>
    /// The sequence type.
    /// </summary>
    public readonly string SequenceType;

    /// <summary>
    /// Initializes a new instance of the <see cref="MattersTableRow"/> class.
    /// from given matter instance.
    /// </summary>
    /// <param name="matter">
    /// The matter.
    /// </param>
    /// <param name="selected">
    /// The selected.
    /// </param>
    public MattersTableRow(Matter matter, bool selected)
    {
        Value = matter.Id.ToString();
        Text = matter.Name;
        Selected = selected;
        Nature = (byte)matter.Nature;
        SequenceType = matter.SequenceType.GetDisplayValue();
        Group = matter.Group.GetDisplayValue();
    }
}