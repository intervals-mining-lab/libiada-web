namespace Libiada.Web.Models.Repositories.Catalogs;

using System.Security.Principal;

using Libiada.Core.Core;
using Libiada.Core.Core.ArrangementManagers;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;
using Libiada.Core.Extensions;

using Libiada.Web.Models.CalculatorsData;
using Libiada.Web.Extensions;

using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;

/// <summary>
/// The binary characteristic repository.
/// </summary>
public class BinaryCharacteristicRepository
{
    /// <summary>
    /// The binary characteristic links.
    /// </summary>
    private readonly BinaryCharacteristicLink[] characteristicsLinks;
    private readonly IPrincipal currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public BinaryCharacteristicRepository(LibiadaDatabaseEntities db, IPrincipal currentUser)
    {
        characteristicsLinks = db.BinaryCharacteristicLinks.ToArray();
        this.currentUser = currentUser;
    }

    /// <summary>
    /// The get binary characteristic types.
    /// </summary>
    /// <returns>
    /// The <see cref="List{CharacteristicData}"/>.
    /// </returns>
    public List<CharacteristicSelectListItem> GetCharacteristicTypes()
    {
        Link[] links;
        BinaryCharacteristic[] characteristics;
        ArrangementType arrangementType = ArrangementType.Intervals;
        if (currentUser.IsAdmin())
        {
            links = EnumExtensions.ToArray<Link>();
            characteristics = EnumExtensions.ToArray<BinaryCharacteristic>();
        }
        else
        {
            links = StaticCollections.UserAvailableLinks.ToArray();
            characteristics = StaticCollections.UserAvailableBinaryCharacteristics.ToArray();
        }

        var result = new List<CharacteristicSelectListItem>(characteristics.Length);

        foreach (BinaryCharacteristic characteristic in characteristics)
        {
            List<SelectListItem> linkSelectListItems = characteristicsLinks
                .Where(cl => cl.BinaryCharacteristic == characteristic && links.Contains(cl.Link))
                .Select(cl => new SelectListItem { Value = ((byte)cl.Link).ToString(), Text = cl.Link.GetDisplayValue() })
                .ToList();
            var arrangementTypeSelectListItems = new List<SelectListItem>
                                                     {
                                                         new SelectListItem { Value = ((byte)arrangementType).ToString(), Text = arrangementType.GetDisplayValue() }
                                                     };
            result.Add(new CharacteristicSelectListItem((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems, arrangementTypeSelectListItems));
        }

        return result;
    }
}
