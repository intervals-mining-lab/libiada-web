namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;

    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    public class CharacteristicLinkRepository : ICharacteristicTypeLinkRepository
    {
        /// <summary>
        /// The characteristic type links.
        /// </summary>
        private readonly List<FullCharacteristicLink> fullCharacteristicLinks;

        /// <summary>
        /// The congeneric characteristic links.
        /// </summary>
        private readonly List<CongenericCharacteristicLink> congenericCharacteristicLinks;

        /// <summary>
        /// The accordance characteristic links.
        /// </summary>
        private readonly List<AccordanceCharacteristicLink> accordanceCharacteristicLinks;

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
        public CharacteristicLinkRepository(LibiadaWebEntities db)
        {
            fullCharacteristicLinks = db.FullCharacteristicLink.ToList();
            congenericCharacteristicLinks = db.CongenericCharacteristicLink.ToList();
            accordanceCharacteristicLinks = db.AccordanceCharacteristicLink.ToList();
            binaryCharacteristicLinks = db.BinaryCharacteristicLink.ToList();
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
        /// Gets the congeneric characteristic links.
        /// </summary>
        public IEnumerable<CongenericCharacteristicLink> CongenericCharacteristicLinks
        {
            get
            {
                return congenericCharacteristicLinks.ToArray();
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
        /// The get libiada link.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForFullCharacteristic(int characteristicTypeLinkId)
        {
            return fullCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
        }

        /// <summary>
        /// The get link for congeneric characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForCongenericCharacteristic(int characteristicTypeLinkId)
        {
            return congenericCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
        }

        /// <summary>
        /// The get link for accordance characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForAccordanceCharacteristic(int characteristicTypeLinkId)
        {
            return accordanceCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
        }

        /// <summary>
        /// The get link for binary characteristic.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForBinaryCharacteristic(int characteristicTypeLinkId)
        {
            return binaryCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
        }

        /// <summary>
        /// The get characteristic type.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="CharacteristicType"/>.
        /// </returns>
        public FullCharacteristic GetFullCharacteristic(int characteristicTypeLinkId)
        {
            return fullCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).FullCharacteristic;
        }

        /// <summary>
        /// The get congeneric characteristic type.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="CongenericCharacteristic"/>.
        /// </returns>
        public CongenericCharacteristic GetCongenericCharacteristic(int characteristicTypeLinkId)
        {
            return congenericCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).CongenericCharacteristic;
        }

        /// <summary>
        /// The get accordance characteristic type.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="AccordanceCharacteristic"/>.
        /// </returns>
        public AccordanceCharacteristic GetAccordanceCharacteristic(int characteristicTypeLinkId)
        {
            return accordanceCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).AccordanceCharacteristic;
        }

        /// <summary>
        /// The get binary characteristic type.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="BinaryCharacteristic"/>.
        /// </returns>
        public BinaryCharacteristic GetBinaryCharacteristic(int characteristicTypeLinkId)
        {
            return binaryCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).BinaryCharacteristic;
        }

        /// <summary>
        /// The get characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notation">
        /// The notation id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFullCharacteristicName(int characteristicTypeLinkId, Notation notation)
        {
            return string.Join("  ", GetFullCharacteristicName(characteristicTypeLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCongenericCharacteristicName(int characteristicTypeLinkId, Notation notation)
        {
            return string.Join("  ", GetCongenericCharacteristicName(characteristicTypeLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get accordance characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAccordanceCharacteristicName(int characteristicTypeLinkId, Notation notation)
        {
            return string.Join("  ", GetAccordanceCharacteristicName(characteristicTypeLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get binary characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetBinaryCharacteristicName(int characteristicTypeLinkId, Notation notation)
        {
            return string.Join("  ", GetBinaryCharacteristicName(characteristicTypeLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFullCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetFullCharacteristic(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForFullCharacteristic(characteristicTypeLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }

        /// <summary>
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCongenericCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetCongenericCharacteristic(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForCongenericCharacteristic(characteristicTypeLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }

        /// <summary>
        /// The get accordance characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAccordanceCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetAccordanceCharacteristic(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForAccordanceCharacteristic(characteristicTypeLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }

        /// <summary>
        /// The get binary characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetBinaryCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetBinaryCharacteristic(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForBinaryCharacteristic(characteristicTypeLinkId);
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
