namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Link = LibiadaCore.Core.Link;

    /// <summary>
    /// The characteristic type link repository.
    /// </summary>
    public class CharacteristicTypeLinkRepository : ICharacteristicTypeLinkRepository
    {
        /// <summary>
        /// The characteristic type links.
        /// </summary>
        private readonly List<CharacteristicTypeLink> characteristicTypeLinks;

        /// <summary>
        /// All notations.
        /// </summary>
        private readonly List<Notation> notations;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeLinkRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicTypeLinkRepository(LibiadaWebEntities db)
        {
            characteristicTypeLinks = db.CharacteristicTypeLink.Include(ctl => ctl.CharacteristicType).Include(ctl => ctl.Link).ToList();
            notations = db.Notation.ToList();
        }

        /// <summary>
        /// Gets the characteristic type links.
        /// </summary>
        public IEnumerable<CharacteristicTypeLink> CharacteristicTypeLinks
        {
            get
            {
                return characteristicTypeLinks.ToArray();
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
            return characteristicTypeLinks.Single(c => c.Id == characteristicTypeLinkId).CharacteristicType;
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
            var notation = notations.Single(n => n.Id == notationId).Name;

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
            var link = databaseLink.Id == (byte)Link.NotApplied ? string.Empty : databaseLink.Name;

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
