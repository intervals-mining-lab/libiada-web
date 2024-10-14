namespace Libiada.Web.Models.CalculatorsData;

using Libiada.Core.Extensions;
using Libiada.Database.Models;

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
    /// Initializes a new instance of the <see cref="MattersTableRow"/> class
    /// from given matter instance.
    /// </summary>
    /// <param name="matter">
    /// The matter to use data from.
    /// </param>
    /// <param name="selected">
    /// Flag indicating if row is selected.
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MattersTableRow"/> class
    /// from given sequence group instance.
    /// </summary>
    /// <param name="sequenceGroup">
    /// The sequence group to use data from.
    /// </param>
    /// <param name="selected">
    /// Flag indicating if row is selected.
    /// </param>
    public MattersTableRow(SequenceGroup sequenceGroup, bool selected)
    {
        Value = sequenceGroup.Id.ToString();
        Text = sequenceGroup.Name;
        Selected = selected;
        Nature = (byte)sequenceGroup.Nature;
        SequenceType = sequenceGroup.SequenceType.GetDisplayValue();
        Group = sequenceGroup.Group.GetDisplayValue();
    }
}
