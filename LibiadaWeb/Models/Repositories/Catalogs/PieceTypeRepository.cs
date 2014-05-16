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

        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedPieceTypes, IEnumerable<int> filter)
        {
            return db.piece_type.Where(p => filter.Contains(p.id)).Select(p => new
            {
                Value = p.id,
                Text = p.name,
                Selected = selectedPieceTypes.Contains(p.id),
                Nature = p.nature_id
            });
        }
    }
}