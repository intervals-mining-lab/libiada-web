using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
using LibiadaCore.Extensions;
using LibiadaWeb.Models.Account;
using LibiadaWeb.Models.CalculatorsData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class BinaryCharacteristicRepository : IDisposable
    {
        /// <summary>
        /// The binary characteristic links.
        /// </summary>
        private readonly List<BinaryCharacteristicLink> binaryCharacteristicLinks;

        private static BinaryCharacteristicRepository instance;

        private static object syncRoot = new object();

        public static BinaryCharacteristicRepository Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (syncRoot)
                    {
                        if(instance == null)
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
        /// Initializes a new instance of the <see cref="CharacteristicLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private BinaryCharacteristicRepository(LibiadaWebEntities db)
        {
            binaryCharacteristicLinks = db.BinaryCharacteristicLink.ToList();
        }

        /// <summary>
        /// Gets the binary characteristic links.
        /// </summary>
        public IEnumerable<BinaryCharacteristicLink> BinaryCharacteristicLinks
        {
            get
            {
                return binaryCharacteristicLinks.ToArray();
            }
        }

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
        /// The characteristic type link id.
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
        /// The characteristic type link id.
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
        /// The characteristic type link id.
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
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetBinaryCharacteristicName(int characteristicLinkId)
        {
            var characteristicType = GetBinaryCharacteristic(characteristicLinkId).GetDisplayValue();

            var databaseLink = GetLinkForBinaryCharacteristic(characteristicLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
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
            if (UserHelper.IsAdmin())
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

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}