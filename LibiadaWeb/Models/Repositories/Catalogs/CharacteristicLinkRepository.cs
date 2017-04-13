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
        /// The get link for congeneric characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForCongenericCharacteristic(int characteristicLinkId)
        {
            return congenericCharacteristicLinks.Single(c => c.Id == characteristicLinkId).Link;
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
        /// The get congeneric characteristic type.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="CongenericCharacteristic"/>.
        /// </returns>
        public CongenericCharacteristic GetCongenericCharacteristic(int characteristicLinkId)
        {
            return congenericCharacteristicLinks.Single(c => c.Id == characteristicLinkId).CongenericCharacteristic;
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
        public string GetCongenericCharacteristicName(int characteristicLinkId, Notation notation)
        {
            return string.Join("  ", GetCongenericCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
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
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCongenericCharacteristicName(int characteristicLinkId)
        {
            var characteristicType = GetCongenericCharacteristic(characteristicLinkId).GetDisplayValue();

            var databaseLink = GetLinkForCongenericCharacteristic(characteristicLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
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
            var characteristicType = GetAccordanceCharacteristic(characteristicLinkId).GetDisplayValue();

            var databaseLink = GetLinkForAccordanceCharacteristic(characteristicLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
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
