using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;

    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    public class CharacteristicTypeLinkRepository : ICharacteristicTypeLinkRepository
    {
        /// <summary>
        /// The characteristic type links.
        /// </summary>
        private readonly List<FullCharacteristicLink> fullCharacteristicLinks;
        private readonly List<CongenericCharacteristicLink> congenericCharacteristicLinks;
        private readonly List<AccordanceCharacteristicLink> accordanceCharacteristicLinks;
        private readonly List<BinaryCharacteristicLink> binaryCharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicTypeLinkRepository(LibiadaWebEntities db)
        {
            fullCharacteristicLinks = db.FullCharacteristicLink.Include(ctl => ctl.FullCharacteristic).ToList();
            congenericCharacteristicLinks = db.CongenericCharacteristicLink.Include(ctl => ctl.CongenericCharacteristic).ToList();
            accordanceCharacteristicLinks = db.AccordanceCharacteristicLink.Include(ctl => ctl.AccordanceCharacteristic).ToList();
            binaryCharacteristicLinks = db.BinaryCharacteristicLink.Include(ctl => ctl.BinaryCharacteristic).ToList();
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

        public IEnumerable<CongenericCharacteristicLink> CongenericCharacteristicLinks
        {
            get
            {
                return congenericCharacteristicLinks.ToArray();
            }
        }

        public IEnumerable<AccordanceCharacteristicLink> AccordanceCharacteristicLinks
        {
            get
            {
                return accordanceCharacteristicLinks.ToArray();
            }
        }

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

        public Link GetLinkForCongenericCharacteristic(int characteristicTypeLinkId)
        {
            return congenericCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
        }

        public Link GetLinkForAccordanceCharacteristic(int characteristicTypeLinkId)
        {
            return accordanceCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
        }

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
        public FullCharacteristic GetFullCharacteristicType(int characteristicTypeLinkId)
        {
            return fullCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).FullCharacteristic;
        }

        public CongenericCharacteristic GetCongenericCharacteristicType(int characteristicTypeLinkId)
        {
            return congenericCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).CongenericCharacteristic;
        }

        public AccordanceCharacteristic GetAccordanceCharacteristicType(int characteristicTypeLinkId)
        {
            return accordanceCharacteristicLinks.Single(c => c.Id == characteristicTypeLinkId).AccordanceCharacteristic;
        }

        public BinaryCharacteristic GetBinaryCharacteristicType(int characteristicTypeLinkId)
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

        public string GetCongenericCharacteristicName(int characteristicTypeLinkId, Notation notation)
        {
            return string.Join("  ", GetCongenericCharacteristicName(characteristicTypeLinkId), notation.GetDisplayValue());
        }

        public string GetAccordanceCharacteristicName(int characteristicTypeLinkId, Notation notation)
        {
            return string.Join("  ", GetAccordanceCharacteristicName(characteristicTypeLinkId), notation.GetDisplayValue());
        }

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
            var characteristicType = GetFullCharacteristicType(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForFullCharacteristic(characteristicTypeLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }

        public string GetCongenericCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetCongenericCharacteristicType(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForCongenericCharacteristic(characteristicTypeLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }

        public string GetAccordanceCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetAccordanceCharacteristicType(characteristicTypeLinkId).GetDisplayValue();

            var databaseLink = GetLinkForAccordanceCharacteristic(characteristicTypeLinkId);
            var link = databaseLink == Link.NotApplied ? string.Empty : databaseLink.GetDisplayValue();

            return string.Join("  ", characteristicType, link);
        }

        public string GetBinaryCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetBinaryCharacteristicType(characteristicTypeLinkId).GetDisplayValue();

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
