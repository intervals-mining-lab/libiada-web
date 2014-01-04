using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace LibiadaWeb.Models.Repositories.Catalogs
{ 
    public class PieceTypeRepository : IPieceTypeRepository
    {
        private readonly LibiadaWebEntities db;

        public PieceTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        public IQueryable<piece_type> All
        {
            get { return db.piece_type; }
        }

        public IQueryable<piece_type> AllIncluding(params Expression<Func<piece_type, object>>[] includeProperties)
        {
            IQueryable<piece_type> query = db.piece_type;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public piece_type Find(int id)
        {
            return db.piece_type.Single(p => p.id == id);
        }

        public void InsertOrUpdate(piece_type piece_type)
        {
            if (piece_type.id == default(int)) {
                // New entity
                db.piece_type.AddObject(piece_type);
            } else {
                // Existing entity
                db.piece_type.Attach(piece_type);
                db.ObjectStateManager.ChangeObjectState(piece_type, EntityState.Modified);
            }
        }

        public void Delete(int id)
        {
            var piece_type = Find(id);
            db.piece_type.DeleteObject(piece_type);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose() 
        {
            db.Dispose();
        }

        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.piece_type.Select(p => new
            {
                Value = p.id,
                Text = p.name,
                Selected = false,
                Nature = p.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(int selectedPieceType)
        {
            return db.piece_type.Select(p => new
            {
                Value = p.id,
                Text = p.name,
                Selected = p.id == selectedPieceType,
                Nature = p.nature_id
            });
        }

        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedPieceTypes)
        {
            return db.piece_type.Select(p => new
            {
                Value = p.id,
                Text = p.name,
                Selected = selectedPieceTypes.Contains(p.id),
                Nature = p.nature_id
            });
        }
    }
}