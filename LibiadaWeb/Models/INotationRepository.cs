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
    public interface INotationRepository : IDisposable
    {
        IQueryable<notation> All { get; }
        IQueryable<notation> AllIncluding(params Expression<Func<notation, object>>[] includeProperties);
        notation Find(int id);
        void InsertOrUpdate(notation notation);
        void Delete(int id);
        void Save();
    }
}
