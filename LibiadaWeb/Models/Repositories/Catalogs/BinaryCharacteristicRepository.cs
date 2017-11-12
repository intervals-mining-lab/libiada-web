namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
    using LibiadaCore.Extensions;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.CalculatorsData;

    /// <summary>
    /// The binary characteristic repository.
    /// </summary>
    public class BinaryCharacteristicRepository
    {
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The instance.
        /// </summary>
        private static BinaryCharacteristicRepository instance;

        /// <summary>
        /// The binary characteristic links.
        /// </summary>
        private readonly BinaryCharacteristicLink[] binaryCharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private BinaryCharacteristicRepository(LibiadaWebEntities db)
        {
            binaryCharacteristicLinks = db.BinaryCharacteristicLink.ToArray();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static BinaryCharacteristicRepository Instance
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
                                instance = new BinaryCharacteristicRepository(db);
                            }
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the binary characteristic links.
        /// </summary>
        public IEnumerable<BinaryCharacteristicLink> BinaryCharacteristicLinks => binaryCharacteristicLinks.ToArray();

        /// <summary>
        /// The create binary characteristic.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="characteristicLinkId">
        /// The characteristic id.
        /// </param>
        /// <param name="firstElementId">
        /// The first element id.
        /// </param>
        /// <param name="secondElementId">
        /// The second element id.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="BinaryCharacteristicValue"/>.
        /// </returns>
        public BinaryCharacteristicValue CreateBinaryCharacteristic(long sequenceId, short characteristicLinkId, long firstElementId, long secondElementId, double value)
        {
            var characteristic = new BinaryCharacteristicValue
            {
                SequenceId = sequenceId,
                CharacteristicLinkId = characteristicLinkId,
                FirstElementId = firstElementId,
                SecondElementId = secondElementId,
                Value = value
            };
            return characteristic;
        }

        /// <summary>
        /// The get link for binary characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForBinaryCharacteristic(int characteristicLinkId)
        {
            return binaryCharacteristicLinks.Single(c => c.Id == characteristicLinkId).Link;
        }

        /// <summary>
        /// The get binary characteristic type.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic link id.
        /// </param>
        /// <returns>
        /// The <see cref="BinaryCharacteristic"/>.
        /// </returns>
        public BinaryCharacteristic GetBinaryCharacteristic(int characteristicLinkId)
        {
            return binaryCharacteristicLinks.Single(c => c.Id == characteristicLinkId).BinaryCharacteristic;
        }

        /// <summary>
        /// The get binary characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic link id.
        /// </param>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetBinaryCharacteristicName(int characteristicLinkId, Notation notation)
        {
            return string.Join("  ", GetBinaryCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get binary characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetBinaryCharacteristicName(int characteristicLinkId)
        {
            string characteristicTypeName = GetBinaryCharacteristic(characteristicLinkId).GetDisplayValue();

            Link link = GetLinkForBinaryCharacteristic(characteristicLinkId);
            string linkName = link == Link.NotApplied ? string.Empty : link.GetDisplayValue();

            return string.Join("  ", characteristicTypeName, linkName);
        }



        /// <summary>
        /// The get binary characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetBinaryCharacteristicTypes()
        {
            Link[] links;
            BinaryCharacteristic[] characteristics;
            if (AccountHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<BinaryCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableBinaryCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (BinaryCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = binaryCharacteristicLinks
                    .Where(cl => cl.BinaryCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }
    }
}