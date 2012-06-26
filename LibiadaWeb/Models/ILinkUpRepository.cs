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
    public interface ILinkUpRepository : IDisposable
    {
        IQueryable<link_up> All { get; }
        IQueryable<link_up> AllIncluding(params Expression<Func<link_up, object>>[] includeProperties);
        link_up Find(int id);
        void InsertOrUpdate(link_up link_up);
        void Delete(int id);
        void Save();
    }
}
