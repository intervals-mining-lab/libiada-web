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
    public interface IConnectionRepository : IDisposable
    {
        IQueryable<connection> All { get; }
        IQueryable<connection> AllIncluding(params Expression<Func<connection, object>>[] includeProperties);
        connection Find(long id);
        void InsertOrUpdate(connection connection);
        void Delete(long id);
        void Save();
    }
}
