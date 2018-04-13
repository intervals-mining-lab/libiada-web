namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.CalculatorsData;

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
        private AccordanceCharacteristicRepository(LibiadaWebEntities db)
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
                            using (var db = new LibiadaWebEntities())
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
        /// Gets the accordance characteristic links.
        /// </summary>
        public IEnumerable<AccordanceCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

        /// <summary>
        /// The get link for accordance characteristic.
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
        /// The get accordance characteristic type.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="AccordanceCharacteristic"/>.
        /// </returns>
        public AccordanceCharacteristic GetCharacteristic(int characteristicLinkId)
        {
            return characteristicsLinks.Single(c => c.Id == characteristicLinkId).AccordanceCharacteristic;
        }

        /// <summary>
        /// The get accordance characteristic name.
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
        /// The get accordance characteristic name.
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
        /// The get accordance characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicTypeData> GetCharacteristicTypes()
        {
            Link[] links;
            AccordanceCharacteristic[] characteristics;
            if (AccountHelper.IsAdmin())
            {
                links = EnumExtensions.ToArray<Link>();
                characteristics = EnumExtensions.ToArray<AccordanceCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableAccordanceCharacteristics.ToArray();
            }

            var result = new List<CharacteristicTypeData>(characteristics.Length);

            foreach (AccordanceCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = characteristicsLinks
                    .Where(cl => cl.AccordanceCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicTypeData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }
    }
}