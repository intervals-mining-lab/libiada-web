using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public interface IFmotivTypeRepository : IDisposable
    {
        IQueryable<fmotiv_type> All { get; }
        IQueryable<fmotiv_type> AllIncluding(params Expression<Func<fmotiv_type, object>>[] includeProperties);
        fmotiv_type Find(int id);
        void InsertOrUpdate(fmotiv_type fmotiv_type);
        void Delete(int id);
        void Save();
    }
}