namespace Libiada.Web.Models.Repositories.Catalogs;

using System.Security.Principal;

using Libiada.Core.Core;
using Libiada.Core.Core.ArrangementManagers;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;
using Libiada.Core.Extensions;

using Libiada.Web.Models.CalculatorsData;
using Libiada.Web.Extensions;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;

/// <summary>
/// The full characteristic repository.
/// </summary>
public class FullCharacteristicRepository
{
    /// <summary>
    /// The characteristic type links.
    /// </summary>
    private readonly FullCharacteristicLink[] characteristicsLinks;
    private readonly IPrincipal currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// The db.
    /// </param>
    public FullCharacteristicRepository(LibiadaDatabaseEntities db, IPrincipal currentUser)
    {
        characteristicsLinks = db.FullCharacteristicLinks.ToArray();
        this.currentUser = currentUser;
    }

    /// <summary>
    /// Gets characteristics types.
    /// </summary>
    /// <returns>
    /// The <see cref="List{CharacteristicData}"/>.
    /// </returns>
    public List<CharacteristicSelectListItem> GetCharacteristicTypes()
    {
        Link[] links;
        FullCharacteristic[] characteristics;
        ArrangementType[] arrangementTypes;

        if (currentUser.IsAdmin())
        {
            links = EnumExtensions.ToArray<Link>();
            characteristics = EnumExtensions.ToArray<FullCharacteristic>();
            arrangementTypes = EnumExtensions.ToArray<ArrangementType>();
        }
        else
        {
            links = StaticCollections.UserAvailableLinks.ToArray();
            characteristics = StaticCollections.UserAvailableFullCharacteristics.ToArray();
            arrangementTypes = StaticCollections.UserAvailableArrangementTypes.ToArray();
        }

        var result = new List<CharacteristicSelectListItem>(characteristics.Length);

        foreach (FullCharacteristic characteristic in characteristics)
        {
            List<SelectListItem> linkSelectListItems = characteristicsLinks
                .Where(cl => cl.FullCharacteristic == characteristic && links.Contains(cl.Link))
                .Select(cl => new SelectListItem { Value = ((byte)cl.Link).ToString(), Text = cl.Link.GetDisplayValue() })
                .Distinct(new SelectListItemComparer())
                .ToList();
            List<SelectListItem> arrangementTypeSelectListItems = characteristicsLinks
                .Where(cl => cl.FullCharacteristic == characteristic && arrangementTypes.Contains(cl.ArrangementType))
                .Select(cl => new SelectListItem { Value = ((byte)cl.ArrangementType).ToString(), Text = cl.ArrangementType.GetDisplayValue() })
                .Distinct(new SelectListItemComparer())
                .ToList();

            result.Add(new CharacteristicSelectListItem((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems, arrangementTypeSelectListItems));
        }

        return result;
    }
}
