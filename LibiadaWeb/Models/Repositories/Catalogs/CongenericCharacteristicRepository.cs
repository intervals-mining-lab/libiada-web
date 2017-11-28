namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The congeneric characteristic repository.
    /// </summary>
    public class CongenericCharacteristicRepository
    {
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The instance.
        /// </summary>
        private static CongenericCharacteristicRepository instance;

        /// <summary>
        /// The congeneric characteristic links.
        /// </summary>
        private readonly CongenericCharacteristicLink[] characteristicsLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CongenericCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private CongenericCharacteristicRepository(LibiadaWebEntities db)
        {
            characteristicsLinks = db.CongenericCharacteristicLink.ToArray();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static CongenericCharacteristicRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            using (var db = new LibiadaWebEntities())
                            {
                                instance = new CongenericCharacteristicRepository(db);
                            }
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the congeneric characteristic links.
        /// </summary>
        public IEnumerable<CongenericCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

        /// <summary>
        /// The get link for congeneric characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForCharacteristic(int characteristicLinkId)
        {
            return characteristicsLinks.Single(c => c.Id == characteristicLinkId).Link;
        }

        /// <summary>
        /// The get congeneric characteristic type.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="CongenericCharacteristic"/>.
        /// </returns>
        public CongenericCharacteristic GetCharacteristic(int characteristicLinkId)
        {
            return characteristicsLinks.Single(c => c.Id == characteristicLinkId).CongenericCharacteristic;
        }

        /// <summary>
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCharacteristicName(int characteristicLinkId, Notation notation)
        {
            return string.Join("  ", GetCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCharacteristicName(int characteristicLinkId)
        {
            string characteristicTypeName = GetCharacteristic(characteristicLinkId).GetDisplayValue();

            Link link = GetLinkForCharacteristic(characteristicLinkId);
            string linkName = link == Link.NotApplied ? string.Empty : link.GetDisplayValue();

            return string.Join("  ", characteristicTypeName, linkName);
        }

        /// <summary>
        /// The get congeneric characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetCharacteristicTypes()
        {
            Link[] links;
            CongenericCharacteristic[] characteristics;
            if (AccountHelper.IsAdmin())
            {
                links = EnumExtensions.ToArray<Link>();
                characteristics = EnumExtensions.ToArray<CongenericCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableCongenericCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (CongenericCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = characteristicsLinks
                    .Where(cl => cl.CongenericCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }
    }
}