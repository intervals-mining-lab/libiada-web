using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
using LibiadaCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class BinaryCharacteristicRepository : IDisposable
    {
        /// <summary>
        /// The binary characteristic links.
        /// </summary>
        private readonly List<BinaryCharacteristicLink> binaryCharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public BinaryCharacteristicRepository(LibiadaWebEntities db)
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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}