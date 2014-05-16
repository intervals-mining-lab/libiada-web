using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class CharacteristicTypeRepository : ICharacteristicTypeRepository
    {
        private readonly LibiadaWebEntities db;

        public CharacteristicTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<characteristic_type> characteristicTypes)
        {
            HashSet<int> characteristicTypeIds = characteristicTypes != null
                                                ? new HashSet<int>(characteristicTypes.Select(c => c.id))
                                                : new HashSet<int>();
            var allCharacteristicTypes = db.characteristic_type;
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
                allcharacteristicTypes = db.characteristic_type;
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

        public void Dispose()
        {
            db.Dispose();
        }

        public IEnumerable<object> GetSelectListWithLinkable(IEnumerable<characteristic_type> characteristicTypes)
        {
            return db.characteristic_type.Where(c => characteristicTypes.Contains(c)).Select(c => new
            {
                Value = c.id,
                Text = c.name,
                Selected = false,
                Linkable = c.linkable
            });
        }
    }
}