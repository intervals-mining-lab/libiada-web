namespace LibiadaWeb.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The piece type repository.
    /// </summary>
    public class PieceTypeRepository : IPieceTypeRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="PieceTypeRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public PieceTypeRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose() 
        {
            this.db.Dispose();
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return this.db.piece_type.Select(p => new
            {
                Value = p.id, 
                Text = p.name, 
                Selected = false, 
                Nature = p.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedPieceType">
        /// The selected piece type.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedPieceType)
        {
            return this.db.piece_type.Select(p => new
            {
                Value = p.id, 
                Text = p.name, 
                Selected = p.id == selectedPieceType, 
                Nature = p.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedPieceTypes">
        /// The selected piece types.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedPieceTypes)
        {
            return this.db.piece_type.Select(p => new
            {
                Value = p.id, 
                Text = p.name, 
                Selected = selectedPieceTypes.Contains(p.id), 
                Nature = p.nature_id
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedPieceTypes">
        /// The selected piece types.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedPieceTypes, IEnumerable<int> filter)
        {
            return this.db.piece_type.Where(p => filter.Contains(p.id)).Select(p => new
            {
                Value = p.id, 
                Text = p.name, 
                Selected = selectedPieceTypes.Contains(p.id), 
                Nature = p.nature_id
            });
        }
    }
}