namespace LibiadaWeb.Models.Repositories.Catalogs
{
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
