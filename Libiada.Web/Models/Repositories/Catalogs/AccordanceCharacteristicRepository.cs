namespace Libiada.Web.Models.Repositories.Catalogs;

using System.Security.Claims;

using Libiada.Core.Core;
using Libiada.Core.Core.ArrangementManagers;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;
using Libiada.Core.Extensions;

using Libiada.Web.Models.CalculatorsData;
using Libiada.Web.Extensions;

using EnumExtensions = Core.Extensions.EnumExtensions;


/// <summary>
/// The accordance characteristic repository.
/// </summary>
public class AccordanceCharacteristicRepository
{
    /// <summary>
    /// The accordance characteristics links.
    /// </summary>
    private readonly AccordanceCharacteristicLink[] characteristicsLinks;
    private readonly ClaimsPrincipal currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccordanceCharacteristicRepository"/> class.
    /// </summary>
    /// <param name="db">
    /// Database context.
    /// </param>
    public AccordanceCharacteristicRepository(LibiadaDatabaseEntities db, ClaimsPrincipal currentUser)
    {
        characteristicsLinks = db.AccordanceCharacteristicLinks.ToArray();
        this.currentUser = currentUser;
    }


    /// <summary>
    /// The get accordance characteristic types.
    /// </summary>
    /// <returns>
    /// The <see cref="List{Libiada.Web.Models.CalculatorsData.CharacteristicSelectListItem}"/>.
    /// </returns>
    public List<CharacteristicSelectListItem> GetCharacteristicTypes()
    {
        Link[] links;
        AccordanceCharacteristic[] characteristics;
        ArrangementType arrangementType = ArrangementType.Intervals;
        if (currentUser.IsAdmin())
        {
            links = EnumExtensions.ToArray<Link>();
            characteristics = EnumExtensions.ToArray<AccordanceCharacteristic>();
        }
        else
        {
            links = StaticCollections.UserAvailableLinks.ToArray();
            characteristics = StaticCollections.UserAvailableAccordanceCharacteristics.ToArray();
        }

        List<CharacteristicSelectListItem> result = new(characteristics.Length);

        foreach (AccordanceCharacteristic characteristic in characteristics)
        {
            List<SelectListItem> linkSelectListItems = characteristicsLinks
                .Where(cl => cl.AccordanceCharacteristic == characteristic && links.Contains(cl.Link))
                .Select(cl => new SelectListItem { Value = ((byte)cl.Link).ToString(), Text = cl.Link.GetDisplayValue() })
                .ToList();
            List<SelectListItem> arrangementTypeSelectListItems =
                                                     [
                                                         new() { Value = ((byte)arrangementType).ToString(), Text = arrangementType.GetDisplayValue() }
                                                     ];
            result.Add(new CharacteristicSelectListItem((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems, arrangementTypeSelectListItems));
        }

        return result;
    }
}
