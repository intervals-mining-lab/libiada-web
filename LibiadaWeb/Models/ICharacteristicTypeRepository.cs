using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using LibiadaWeb;

namespace LibiadaWeb.Models
{
    public interface ICharacteristicTypeRepository : IDisposable
    {
        IQueryable<characteristic_type> All { get; }
        IQueryable<characteristic_type> AllIncluding(params Expression<Func<characteristic_type, object>>[] includeProperties);
        characteristic_type Find(int id);
        void InsertOrUpdate(characteristic_type characteristic_type);
        void Delete(int id);
        void Save();
    }
}
