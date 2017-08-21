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
        private static AccordanceCharacteristicRepository instance;

        /// <summary>
        /// The accordance characteristic links.
        /// </summary>
        private readonly List<AccordanceCharacteristicLink> accordanceCharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordanceCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private AccordanceCharacteristicRepository(LibiadaWebEntities db)
        {
            accordanceCharacteristicLinks = db.AccordanceCharacteristicLink.ToList();
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
        public IEnumerable<AccordanceCharacteristicLink> AccordanceCharacteristicLinks
        {
            get
            {
                return accordanceCharacteristicLinks.ToArray();
            }
        }

        /// <summary>
        /// The get link for accordance characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForAccordanceCharacteristic(int characteristicLinkId)
        {
            return accordanceCharacteristicLinks.Single(c => c.Id == characteristicLinkId).Link;
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
        public AccordanceCharacteristic GetAccordanceCharacteristic(int characteristicLinkId)
        {
            return accordanceCharacteristicLinks.Single(c => c.Id == characteristicLinkId).AccordanceCharacteristic;
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
        public string GetAccordanceCharacteristicName(int characteristicLinkId, Notation notation)
        {
            return string.Join("  ", GetAccordanceCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
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
        public string GetAccordanceCharacteristicName(int characteristicLinkId)
        {
            string characteristicTypeName = GetAccordanceCharacteristic(characteristicLinkId).GetDisplayValue();

            Link link = GetLinkForAccordanceCharacteristic(characteristicLinkId);
            string linkName = link == Link.NotApplied ? string.Empty : link.GetDisplayValue();

            return string.Join("  ", characteristicTypeName, linkName);
        }



        /// <summary>
        /// The get accordance characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetAccordanceCharacteristicTypes()
        {
            Link[] links;
            AccordanceCharacteristic[] characteristics;
            if (AccountHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<AccordanceCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableAccordanceCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (AccordanceCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = accordanceCharacteristicLinks
                    .Where(cl => cl.AccordanceCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }
    }
}