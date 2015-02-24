namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System.Collections.Generic;
    using System.Data.Entity;
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
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

        /// <summary>
        /// The common sequence repository.
        /// </summary>
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
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
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
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId);
            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            var characteristicTypes = characteristicTypeLinkRepository.GetCharacteristics(c => c.FullSequenceApplicable);

            var pieceTypeIds = db.PieceType.Where(p => p.NatureId == Aliases.Nature.Genetic
                                         && p.Id != Aliases.PieceType.FullGenome
                                         && p.Id != Aliases.PieceType.ChloroplastGenome
                                         && p.Id != Aliases.PieceType.MitochondrionGenome).Select(p => p.Id);

            return new Dictionary<string, object>
                {
                    { "matters", matterRepository.GetMatterSelectList(matters) }, 
                    { "characteristicTypes", characteristicTypes },  
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

        /// <summary>
        /// The extract sequences.
        /// </summary>
        /// <param name="sequenceId">
        /// The sequence id.
        /// </param>
        /// <param name="pieceTypeIds">
        /// The piece type ids.
        /// </param>
        /// <param name="genes">
        /// The genes.
        /// </param>
        /// <returns>
        /// The <see cref="List{Gene}"/>.
        /// </returns>
        public List<Chain> ExtractSequences(long sequenceId, int[] pieceTypeIds, out List<Gene> genes)
        {
            genes = db.Gene.Where(g => g.SequenceId == sequenceId && pieceTypeIds.Contains(g.PieceTypeId)).Include(g => g.Piece).Include(g => g.Product).ToList();

            var pieces = genes.Select(g => g.Piece.First()).ToList();

            var sequences = ConvertToChains(pieces, sequenceId);

            return sequences;
        }

        /// <summary>
        /// The extract sequences.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="notationId">
        /// The notation id.
        /// </param>
        /// <param name="pieceTypeIds">
        /// The piece type ids.
        /// </param>
        /// <param name="genes">
        /// The genes.
        /// </param>
        /// <returns>
        /// The <see cref="List{Gene}"/>.
        /// </returns>
        public List<Chain> ExtractSequences(long matterId, int notationId, int[] pieceTypeIds, out List<Gene> genes)
        {
            var sequenceId = db.DnaSequence.Single(c => c.MatterId == matterId && c.NotationId == notationId).Id;
            return ExtractSequences(sequenceId, pieceTypeIds, out genes);
        }
    }
}
