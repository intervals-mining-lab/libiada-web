namespace Libiada.Web.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;

    using Libiada.Database;
    using Libiada.Database.Models;

    using LibiadaCore.Core;
    using LibiadaCore.Core.ArrangementManagers;
    using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
    using LibiadaCore.Extensions;

    using Libiada.Web.Models.CalculatorsData;
    
    

    /// <summary>
    /// The accordance characteristic repository.
    /// </summary>
    public class AccordanceCharacteristicRepository
    {
        /// <summary>
        /// The accordance characteristics links.
        /// </summary>
        private readonly AccordanceCharacteristicLink[] characteristicsLinks;
        private readonly IPrincipal currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordanceCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public AccordanceCharacteristicRepository(LibiadaDatabaseEntities db, IPrincipal currentUser)
        {
            characteristicsLinks = db.AccordanceCharacteristicLink.ToArray();
            this.currentUser = currentUser;
        }
        

        /// <summary>
        /// The get accordance characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicSelectListItem> GetCharacteristicTypes()
        {
            Link[] links;
            AccordanceCharacteristic[] characteristics;
            ArrangementType arrangementType = ArrangementType.Intervals;
            if (currentUser.IsInRole("admin"))
            {
                links = EnumExtensions.ToArray<Link>();
                characteristics = EnumExtensions.ToArray<AccordanceCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableAccordanceCharacteristics.ToArray();
            }

            var result = new List<CharacteristicSelectListItem>(characteristics.Length);

            foreach (AccordanceCharacteristic characteristic in characteristics)
            {
                List<SelectListItem> linkSelectListItems = characteristicsLinks
                    .Where(cl => cl.AccordanceCharacteristic == characteristic && links.Contains(cl.Link))
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
}
