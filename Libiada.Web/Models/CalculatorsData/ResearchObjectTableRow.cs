namespace Libiada.Web.Models.CalculatorsData;

using Libiada.Core.Extensions;
using Libiada.Database.Models;

/// <summary>
/// The research object table row.
/// </summary>
public class ResearchObjectTableRow : SelectListItemWithNature
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
    /// Initializes a new instance of the <see cref="ResearchObjectTableRow"/> class
    /// from given research object instance.
    /// </summary>
    /// <param name="researchObject">
    /// The research object to use data from.
    /// </param>
    /// <param name="selected">
    /// Flag indicating if row is selected.
    /// </param>
    public ResearchObjectTableRow(ResearchObject researchObject, bool selected)
    {
        Value = researchObject.Id.ToString();
        Text = researchObject.Name;
        Selected = selected;
        Nature = (byte)researchObject.Nature;
        SequenceType = researchObject.SequenceType.GetDisplayValue();
        Group = researchObject.Group.GetDisplayValue();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResearchObjectTableRow"/> class
    /// from given sequence group instance.
    /// </summary>
    /// <param name="sequenceGroup">
    /// The sequence group to use data from.
    /// </param>
    /// <param name="selected">
    /// Flag indicating if row is selected.
    /// </param>
    public ResearchObjectTableRow(SequenceGroup sequenceGroup, bool selected)
    {
        Value = sequenceGroup.Id.ToString();
        Text = sequenceGroup.Name;
        Selected = selected;
        Nature = (byte)sequenceGroup.Nature;
        SequenceType = sequenceGroup.SequenceType.GetDisplayValue();
        Group = sequenceGroup.Group.GetDisplayValue();
    }
}
