using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Interfaces
{
    public interface IFmotivRepository : IDisposable
    {
        IQueryable<fmotiv> All { get; }
        IQueryable<fmotiv> AllIncluding(params Expression<Func<fmotiv, object>>[] includeProperties);
        fmotiv Find(long id);
        void InsertOrUpdate(fmotiv fmotiv);
        void Delete(long id);
        void Save();
    }
}