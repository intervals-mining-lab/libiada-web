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
    public interface IMatterRepository : IDisposable
    {
        IQueryable<matter> All { get; }
        IQueryable<matter> AllIncluding(params Expression<Func<matter, object>>[] includeProperties);
        matter Find(long id);
        void InsertOrUpdate(matter matter);
        void Delete(long id);
        void Save();
    }
}
