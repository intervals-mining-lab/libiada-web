using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
using LibiadaCore.Extensions;
using LibiadaWeb.Models.Account;
using LibiadaWeb.Models.CalculatorsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class FullCharacteristicRepository : IDisposable
    {
        /// <summary>
        /// The characteristic type links.
        /// </summary>
        private readonly List<FullCharacteristicLink> fullCharacteristicLinks;

        private static FullCharacteristicRepository instance;

        private static object syncRoot = new object();

        public static FullCharacteristicRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            using (var db = new LibiadaWebEntities())
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
        /// Initializes a new instance of the <see cref="CharacteristicLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private FullCharacteristicRepository(LibiadaWebEntities db)
        {
            fullCharacteristicLinks = db.FullCharacteristicLink.ToList();
        }

        /// <summary>
        /// Gets the characteristic type links.
        /// </summary>
        public IEnumerable<FullCharacteristicLink> FullCharacteristicLinks
        {
            get
            {
                return fullCharacteristicLinks.ToArray();
            }
        }

        /// <summary>
        /// The get libiada link.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForFullCharacteristic(int characteristicLinkId)
        {
            return fullCharacteristicLinks.Single(c => c.Id == characteristicLinkId).Link;
        }
        
        /// <summary>
        /// The get characteristic type.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="CharacteristicType"/>.
        /// </returns>
        public FullCharacteristic GetFullCharacteristic(int characteristicLinkId)
        {
            return fullCharacteristicLinks.Single(c => c.Id == characteristicLinkId).FullCharacteristic;
        }
        
        /// <summary>
        /// The get characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFullCharacteristicName(int characteristicLinkId, Notation notation)
        {
            return string.Join("  ", GetFullCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFullCharacteristicName(int characteristicLinkId)
        {
            var characteristicType = GetFullCharacteristic(characteristicLinkId).GetDisplayValue();

            var databaseLink = GetLinkForFullCharacteristic(characteristicLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }


        /// <summary>
        /// Gets characteristics types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetFullCharacteristicTypes()
        {
            Link[] links;
            FullCharacteristic[] characteristics;
            if (UserHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<FullCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableFullCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (FullCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = fullCharacteristicLinks
                    .Where(cl => cl.FullCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }



        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}