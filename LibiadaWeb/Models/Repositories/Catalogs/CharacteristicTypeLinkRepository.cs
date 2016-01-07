namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    using Link = LibiadaCore.Core.Link;

    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    public class CharacteristicTypeLinkRepository : ICharacteristicTypeLinkRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The characteristic type links.
        /// </summary>
        private readonly List<CharacteristicTypeLink> characteristicTypeLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicTypeLinkRepository(LibiadaWebEntities db)
        {
            this.db = db;
            characteristicTypeLinks = db.CharacteristicTypeLink.ToList();
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
            return (Link)characteristicTypeLinks.Single(c => c.Id == characteristicTypeLinkId).LinkId;
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
            var characteristicTypeId = characteristicTypeLinks.Single(c => c.Id == characteristicTypeLinkId).CharacteristicTypeId;
            return db.CharacteristicType.Single(c => c.Id == characteristicTypeId);
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
            var notation = db.Notation.Single(n => n.Id == notationId).Name;

            return string.Join("  ", GetCharacteristicName(characteristicTypeLinkId), notation);
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
        public string GetCharacteristicName(int characteristicTypeLinkId)
        {
            var characteristicType = GetCharacteristicType(characteristicTypeLinkId).Name;

            var databaseLink = characteristicTypeLinks.Single(c => c.Id == characteristicTypeLinkId).Link;
            var link = databaseLink.Id != Aliases.Link.NotApplied ? databaseLink.Name : string.Empty;

            return string.Join("  ", characteristicType, link);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
