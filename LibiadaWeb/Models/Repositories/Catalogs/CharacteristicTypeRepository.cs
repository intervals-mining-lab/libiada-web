// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacteristicTypeRepository.cs" company="">
//   
// </copyright>
// <summary>
//   The characteristic type repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
        public List<SelectListItem> GetSelectListItems(IEnumerable<characteristic_type> characteristicTypes)
        {
            HashSet<int> characteristicTypeIds = characteristicTypes != null
                                                ? new HashSet<int>(characteristicTypes.Select(c => c.id))
                                                : new HashSet<int>();
            var allCharacteristicTypes = this.db.characteristic_type;
            var characteristicTypesList = new List<SelectListItem>();
            foreach (var characteristicType in allCharacteristicTypes)
            {
                characteristicTypesList.Add(new SelectListItem
                    {
                        Value = characteristicType.id.ToString(), 
                        Text = characteristicType.name, 
                        Selected = characteristicTypeIds.Contains(characteristicType.id)
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
        public List<SelectListItem> GetSelectListItems(IEnumerable<characteristic_type> allcharacteristicTypes, 
                                                       IEnumerable<characteristic_type> selectedCharacteristicTypes)
        {
            HashSet<int> characteristicTypeIds;
            if (selectedCharacteristicTypes != null)
            {
                characteristicTypeIds = new HashSet<int>(selectedCharacteristicTypes.Select(c => c.id));
            }
            else
            {
                characteristicTypeIds = new HashSet<int>();
            }

            if (allcharacteristicTypes == null)
            {
                allcharacteristicTypes = this.db.characteristic_type;
            }

            var characteristicTypesList = new List<SelectListItem>();
            foreach (var characteristicType in allcharacteristicTypes)
            {
                characteristicTypesList.Add(new SelectListItem
                    {
                        Value = characteristicType.id.ToString(), 
                        Text = characteristicType.name, 
                        Selected = characteristicTypeIds.Contains(characteristicType.id)
                    });
            }

            return characteristicTypesList;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.db.Dispose();
        }

        /// <summary>
        /// The get select list with linkable.
        /// </summary>
        /// <param name="characteristicTypes">
        /// The characteristic types.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithLinkable(IEnumerable<characteristic_type> characteristicTypes)
        {
            return this.db.characteristic_type.Where(c => characteristicTypes.Contains(c)).Select(c => new
            {
                Value = c.id, 
                Text = c.name, 
                Selected = false, 
                Linkable = c.linkable
            });
        }
    }
}