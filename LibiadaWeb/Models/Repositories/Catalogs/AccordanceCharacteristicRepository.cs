using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
using LibiadaCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class AccordanceCharacteristicRepository : IDisposable
    {
        /// <summary>
        /// The accordance characteristic links.
        /// </summary>
        private readonly List<AccordanceCharacteristicLink> accordanceCharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public AccordanceCharacteristicRepository(LibiadaWebEntities db)
        {
            accordanceCharacteristicLinks = db.AccordanceCharacteristicLink.ToList();
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
            var characteristicType = GetAccordanceCharacteristic(characteristicLinkId).GetDisplayValue();

            var databaseLink = GetLinkForAccordanceCharacteristic(characteristicLinkId);
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