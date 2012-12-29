using System;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public interface ICharacteristicApplicabilityRepository : IDisposable
    {
        IQueryable<characteristic_applicability> All { get; }
        IQueryable<characteristic_applicability> AllIncluding(params Expression<Func<characteristic_applicability, object>>[] includeProperties);
        characteristic_applicability Find(int id);
        void InsertOrUpdate(characteristic_applicability characteristic_applicability);
        void Delete(int id);
        void Save();
    }
}