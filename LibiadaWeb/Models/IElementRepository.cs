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
    public interface IElementRepository : IDisposable
    {
        IQueryable<element> All { get; }
        IQueryable<element> AllIncluding(params Expression<Func<element, object>>[] includeProperties);
        element Find(long id);
        void InsertOrUpdate(element element);
        void Delete(long id);
        void Save();
    }
}
