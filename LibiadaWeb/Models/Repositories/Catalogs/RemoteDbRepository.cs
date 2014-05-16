using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{ 
    public class RemoteDbRepository : IRemoteDbRepository
    {
        private readonly LibiadaWebEntities db;

        public RemoteDbRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = false,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(int selectedDb)
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = n.id == selectedDb,
                Nature = n.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(List<int> selectedDbs)
        {
            return db.remote_db.Select(n => new
            {
                Value = n.id,
                Text = n.name,
                Selected = selectedDbs.Contains(n.id),
                Nature = n.nature_id
            });
        }

        public void Dispose() 
        {
            db.Dispose();
        }
    }
}