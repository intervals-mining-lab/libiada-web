﻿using LibiadaCore.Core;
using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
using LibiadaCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class CongenericCharacteristicRepository : IDisposable
    {

        /// <summary>
        /// The congeneric characteristic links.
        /// </summary>
        private readonly List<CongenericCharacteristicLink> congenericCharacteristicLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CongenericCharacteristicRepository(LibiadaWebEntities db)
        {
            congenericCharacteristicLinks = db.CongenericCharacteristicLink.ToList();
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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}