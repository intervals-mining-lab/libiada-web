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
    public interface IBinaryCharacteristicRepository : IDisposable
    {
        IQueryable<binary_characteristic> All { get; }
        IQueryable<binary_characteristic> AllIncluding(params Expression<Func<binary_characteristic, object>>[] includeProperties);
        binary_characteristic Find(long id);
        void InsertOrUpdate(binary_characteristic binary_characteristic);
        void Delete(long id);
        void Save();
    }
}
