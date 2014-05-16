using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class NotationRepository : INotationRepository
    {
        private readonly LibiadaWebEntities db;

        public NotationRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public List<SelectListItem> GetSelectListItems(IEnumerable<notation> notations)
        {
            HashSet<int> notationIds = notations != null
                                           ? new HashSet<int>(notations.Select(c => c.id))
                                           : new HashSet<int>();
            var allNotations = db.notation;
            var notationsList = new List<SelectListItem>();
            foreach (var notation in allNotations)
            {
                notationsList.Add(new SelectListItem
                    {
                        Value = notation.id.ToString(),
                        Text = notation.name,
                        Selected = notationIds.Contains(notation.id)
                    });
            }
            return notationsList;
        }

        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.notation.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = false,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(int selectedNotation)
        {
            return db.notation.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = n.id == selectedNotation,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(List<int> selectedNotations)
        {
            return db.notation.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = selectedNotations.Contains(n.id),
                Nature = n.nature_id
            });
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}