namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The gene repository.
    /// </summary>
    public class GeneRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicTypeRepository;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public GeneRepository(LibiadaWebEntities db)
        {
            this.db = db;
            characteristicTypeRepository = new CharacteristicTypeRepository(db);
            matterRepository = new MatterRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The get genes calculation data.
        /// </summary>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> GetGenesCalculationData()
        {
            var sequenceIds = db.Gene.Select(g => g.SequenceId).Distinct();
            var sequences = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id));
            var matterIds = sequences.Select(c => c.MatterId);

            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            var characteristicsList = db.CharacteristicType.Where(c => c.FullSequenceApplicable);

            var characteristicTypes = characteristicTypeRepository.GetSelectListWithLinkable(characteristicsList);

            var pieceTypeIds = db.PieceType.Where(p => p.NatureId == Aliases.Nature.Genetic
                                         && p.Id != Aliases.PieceType.FullGenome
                                         && p.Id != Aliases.PieceType.ChloroplastGenome
                                         && p.Id != Aliases.PieceType.MitochondrionGenome).Select(p => p.Id);

            var links = new SelectList(db.Link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            return new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList(matters) }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "links", links }, 
                    { "notationsFiltered", new SelectList(db.Notation.Where(n => n.NatureId == Aliases.Nature.Genetic), "id", "name") },
                    { "natureId", Aliases.Nature.Genetic },
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(pieceTypeIds, pieceTypeIds) }
                };
        }

        /// <summary>
        /// The extract chains.
        /// </summary>
        /// <param name="pieces">
        /// The pieces.
        /// </param>
        /// <param name="chainId">
        /// The sequence id.
        /// </param>
        /// <returns>
        /// The <see cref="List{Chain}"/>.
        /// </returns>
        public List<Chain> ConvertToChains(List<Piece> pieces, long chainId)
        {
            var starts = pieces.Select(p => p.Start).ToList();

            var stops = pieces.Select(p => p.Start + p.Length).ToList();

            BaseChain parentChain = commonSequenceRepository.ToLibiadaBaseChain(chainId);

            var iterator = new DefaultCutRule(starts, stops);

            var stringChains = DiffCutter.Cut(parentChain.ToString(), iterator);

            var chains = new List<Chain>();

            for (int i = 0; i < stringChains.Count; i++)
            {
                chains.Add(new Chain(stringChains[i]));
            }

            return chains;
        }
    }
}
