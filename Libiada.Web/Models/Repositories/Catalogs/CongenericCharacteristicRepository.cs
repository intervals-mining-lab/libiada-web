﻿namespace Libiada.Web.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;

    using Libiada.Database;
    using Libiada.Database.Models;

    using LibiadaCore.Core;
    using LibiadaCore.Core.ArrangementManagers;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Extensions;

    using Libiada.Web.Models.CalculatorsData;

    /// <summary>
    /// The congeneric characteristic repository.
    /// </summary>
    public class CongenericCharacteristicRepository
    {
        /// <summary>
        /// The congeneric characteristic links.
        /// </summary>
        private readonly CongenericCharacteristicLink[] characteristicsLinks;
        private readonly IPrincipal currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CongenericCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CongenericCharacteristicRepository(LibiadaDatabaseEntities db, IPrincipal currentUser)
        {
            characteristicsLinks = db.CongenericCharacteristicLink.ToArray();
            this.currentUser = currentUser;
        }

        /// <summary>
        /// The get congeneric characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicSelectListItem> GetCharacteristicTypes()
        {
            Link[] links;
            CongenericCharacteristic[] characteristics;
            ArrangementType[] arrangementTypes;

            if (currentUser.IsInRole("admin"))
            {
                links = EnumExtensions.ToArray<Link>();
                characteristics = EnumExtensions.ToArray<CongenericCharacteristic>();
                arrangementTypes = EnumExtensions.ToArray<ArrangementType>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableCongenericCharacteristics.ToArray();
                arrangementTypes = Aliases.UserAvailableArrangementTypes.ToArray();
            }

            var result = new List<CharacteristicSelectListItem>(characteristics.Length);

            foreach (CongenericCharacteristic characteristic in characteristics)
            {
                List<SelectListItem> linkSelectListItems = characteristicsLinks
                    .Where(cl => cl.CongenericCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(cl => new SelectListItem { Value = ((byte)cl.Link).ToString(), Text = cl.Link.GetDisplayValue() })
                    .Distinct(new SelectListItemComparer())
                    .ToList();
                List<SelectListItem> arrangementTypeSelectListItems = characteristicsLinks
                    .Where(cl => cl.CongenericCharacteristic == characteristic && arrangementTypes.Contains(cl.ArrangementType))
                    .Select(cl => new SelectListItem { Value = ((byte)cl.ArrangementType).ToString(), Text = cl.ArrangementType.GetDisplayValue() })
                    .Distinct(new SelectListItemComparer())
                    .ToList();

                result.Add(new CharacteristicSelectListItem((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems, arrangementTypeSelectListItems));
            }

            return result;
        }

    }
}
