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
    public interface IChainRepository : IDisposable
    {
        IQueryable<chain> All { get; }
        IQueryable<chain> AllIncluding(params Expression<Func<chain, object>>[] includeProperties);
        chain Find(long id);
        void InsertOrUpdate(chain chain);
        void Delete(long id);
        void Save();
    }
}
