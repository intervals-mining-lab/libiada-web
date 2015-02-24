namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Link = LibiadaCore.Core.Link;

    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    public class CharacteristicTypeLinkRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicTypeLinkRepository(LibiadaWebEntities db)
        {
            this.db = db;
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
        public Link GetLibiadaLink(int characteristicTypeLinkId)
        {
            return (Link)db.CharacteristicTypeLink.Single(c => c.Id == characteristicTypeLinkId).LinkId;
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
        public CharacteristicType GetCharacteristicType(int characteristicTypeLinkId)
        {
            var characteristicTypeId = db.CharacteristicTypeLink.Single(c => c.Id == characteristicTypeLinkId).CharacteristicTypeId;
            return db.CharacteristicType.Single(c => c.Id == characteristicTypeId);
        }

        /// <summary>
        /// Extracts characteristics types list with links.
        /// </summary>
        /// <param name="filter">
        /// The filter of characteristics.
        /// </param>
        /// <returns>
        /// The <see cref="List{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetCharacteristics(Func<CharacteristicType, bool> filter)
        {
            var characteristicsList = db.CharacteristicType.Where(filter).Select(c => c.Id);
            var characteristicTypeLinks = db.CharacteristicTypeLink.Where(c => characteristicsList.Contains(c.CharacteristicTypeId)).Include(c => c.Link).Include(c => c.CharacteristicType);

            return characteristicTypeLinks.Select(c => new
                        {
                            Value = c.Id,
                            CharacteristicType = new { Value = c.CharacteristicType.Id, Text = c.CharacteristicType.Name },
                            Link = new { Value = c.Link.Id, Text = c.Link.Name }
                        }).ToList();
        }

        /// <summary>
        /// The get characteristic name.
        /// </summary>
        /// <param name="characteristicTypeLinkId">
        /// The characteristic type and link id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCharacteristicName(int characteristicTypeLinkId, int notationId)
        {
            var characteristicType = GetCharacteristicType(characteristicTypeLinkId).Name;

            var databaseLink = db.CharacteristicTypeLink.Single(c => c.Id == characteristicTypeLinkId).Link;
            var link = databaseLink.Id != Aliases.Link.NotApplied ? databaseLink.Name : string.Empty;

            var notation = db.Notation.Single(n => n.Id == notationId).Name;

            return string.Join("  ", characteristicType, link, notation);
        }
    }
}
