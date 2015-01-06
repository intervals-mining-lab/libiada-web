namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    public class CharacteristicTypeRepository : ICharacteristicTypeRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacteristicTypeRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CharacteristicTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="characteristicTypes">
        /// The characteristic types.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(IEnumerable<CharacteristicType> characteristicTypes)
        {
            HashSet<int> characteristicTypeIds = characteristicTypes != null
                                                ? new HashSet<int>(characteristicTypes.Select(c => c.Id))
                                                : new HashSet<int>();
            var allCharacteristicTypes = db.CharacteristicType;
            var characteristicTypesList = new List<SelectListItem>();
            foreach (var characteristicType in allCharacteristicTypes)
            {
                characteristicTypesList.Add(new SelectListItem
                    {
                        Value = characteristicType.Id.ToString(),
                        Text = characteristicType.Name,
                        Selected = characteristicTypeIds.Contains(characteristicType.Id)
                    });
            }

            return characteristicTypesList;
        }

        /// <summary>
        /// The get select list items.
        /// </summary>
        /// <param name="allcharacteristicTypes">
        /// The allcharacteristic types.
        /// </param>
        /// <param name="selectedCharacteristicTypes">
        /// The selected characteristic types.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SelectListItem> GetSelectListItems(
            IEnumerable<CharacteristicType> allcharacteristicTypes,
            IEnumerable<CharacteristicType> selectedCharacteristicTypes)
        {
            HashSet<int> characteristicTypeIds;
            if (selectedCharacteristicTypes != null)
            {
                characteristicTypeIds = new HashSet<int>(selectedCharacteristicTypes.Select(c => c.Id));
            }
            else
            {
                characteristicTypeIds = new HashSet<int>();
            }

            if (allcharacteristicTypes == null)
            {
                allcharacteristicTypes = db.CharacteristicType;
            }

            var characteristicTypesList = new List<SelectListItem>();
            foreach (var characteristicType in allcharacteristicTypes)
            {
                characteristicTypesList.Add(new SelectListItem
                    {
                        Value = characteristicType.Id.ToString(),
                        Text = characteristicType.Name,
                        Selected = characteristicTypeIds.Contains(characteristicType.Id)
                    });
            }

            return characteristicTypesList;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// The get select list with linkable.
        /// </summary>
        /// <param name="characteristicTypes">
        /// The characteristic types.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithLinkable(IEnumerable<CharacteristicType> characteristicTypes)
        {
            return db.CharacteristicType.Where(c => characteristicTypes.Contains(c)).Select(c => new
            {
                Value = c.Id, 
                Text = c.Name, 
                Selected = false, 
                Linkable = c.Linkable
            });
        }
    }
}
