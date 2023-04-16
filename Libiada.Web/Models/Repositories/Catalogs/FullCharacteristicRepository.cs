namespace Libiada.Web.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;

    using Libiada.Database;
    using Libiada.Database.Models;

    using LibiadaCore.Core;
    using LibiadaCore.Core.ArrangementManagers;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;

    using Libiada.Web.Models.CalculatorsData;

    /// <summary>
    /// The full characteristic repository.
    /// </summary>
    public class FullCharacteristicRepository
    {
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The instance.
        /// </summary>
        private static volatile FullCharacteristicRepository instance;

        /// <summary>
        /// The characteristic type links.
        /// </summary>
        private readonly FullCharacteristicLink[] characteristicsLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private FullCharacteristicRepository(LibiadaDatabaseEntities db)
        {
            characteristicsLinks = db.FullCharacteristicLink.ToArray();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static FullCharacteristicRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            using (var db = new LibiadaDatabaseEntities())
                            {
                                instance = new FullCharacteristicRepository(db);
                            }
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets characteristics types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicSelectListItem> GetCharacteristicTypes(IPrincipal currentUser)
        {
            Link[] links;
            FullCharacteristic[] characteristics;
            ArrangementType[] arrangementTypes;

            if (currentUser.IsInRole("admin"))
            {
                links = EnumExtensions.ToArray<Link>();
                characteristics = EnumExtensions.ToArray<FullCharacteristic>();
                arrangementTypes = EnumExtensions.ToArray<ArrangementType>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableFullCharacteristics.ToArray();
                arrangementTypes = Aliases.UserAvailableArrangementTypes.ToArray();
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
}
