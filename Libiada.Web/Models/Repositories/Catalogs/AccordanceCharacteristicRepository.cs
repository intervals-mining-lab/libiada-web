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
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The instance.
        /// </summary>
        private static volatile AccordanceCharacteristicRepository instance;

        /// <summary>
        /// The accordance characteristics links.
        /// </summary>
        private readonly AccordanceCharacteristicLink[] characteristicsLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordanceCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private AccordanceCharacteristicRepository(LibiadaDatabaseEntities db)
        {
            characteristicsLinks = db.AccordanceCharacteristicLink.ToArray();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static AccordanceCharacteristicRepository Instance
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
                                instance = new AccordanceCharacteristicRepository(db);
                            }
                        }
                    }
                }

                return instance;
            }
        }

        

        /// <summary>
        /// The get accordance characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicSelectListItem> GetCharacteristicTypes(IPrincipal currentUser)
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
