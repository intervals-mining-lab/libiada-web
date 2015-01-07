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
            db.Dispose();
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature()
        {
            return db.PieceType.Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = false, 
                Nature = p.NatureId
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedPieceType">
        /// The selected piece type.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(int selectedPieceType)
        {
            return db.PieceType.Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = p.Id == selectedPieceType, 
                Nature = p.NatureId
            });
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="selectedPieceTypes">
        /// The selected piece types.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedPieceTypes)
        {
            return db.PieceType.Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = selectedPieceTypes.Contains(p.Id), 
                Nature = p.NatureId
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
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetSelectListWithNature(IEnumerable<int> selectedPieceTypes, IEnumerable<int> filter)
        {
            return db.PieceType.Where(p => filter.Contains(p.Id)).Select(p => new
            {
                Value = p.Id, 
                Text = p.Name, 
                Selected = selectedPieceTypes.Contains(p.Id), 
                Nature = p.NatureId
            });
        }
    }
}
