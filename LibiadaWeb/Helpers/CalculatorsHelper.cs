namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The calculators helper.
    /// </summary>
    public class CalculatorsHelper
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The characteristic type link repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The piece type repository.
        /// </summary>
        private readonly PieceTypeRepository pieceTypeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorsHelper"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public CalculatorsHelper(LibiadaWebEntities db)
        {
            this.db = db;
            matterRepository = new MatterRepository(db);
            notationRepository = new NotationRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
        }

        /// <summary>
        /// The fill calculation data.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="minimumSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillCalculationData(Func<CharacteristicType, bool> filter, int minimumSelectedMatters, int maximumSelectedMatters)
        {
            var characteristicTypes = db.CharacteristicType.Include(c => c.CharacteristicTypeLink).Where(filter)
                .Select(c => new { Value = c.Id, Text = c.Name, CharacteristicTypeLink = c.CharacteristicTypeLink.Select(ctl => ctl.Id) }).ToList();
            var links = db.Link.Include(l => l.CharacteristicTypeLink)
                .Select(l => new { Value = l.Id, Text = l.Name, CharacteristicTypeLink = l.CharacteristicTypeLink.Select(ctl => ctl.Id) }).ToList();

            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "characteristicTypes", characteristicTypes },
                    { "links", links },
                    { "natures", new SelectList(db.Nature, "id", "name") }, 
                    { "notations", notationRepository.GetSelectListWithNature() }, 
                    { "languages", new SelectList(db.Language, "id", "name") }, 
                    { "translators", translators }
                };
        }

        /// <summary>
        /// The get genes calculation data.
        /// </summary>
        /// <param name="minimumSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> GetGenesCalculationData(int minimumSelectedMatters, int maximumSelectedMatters)
        {
            var sequenceIds = db.Gene.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId);
            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            var characteristicTypes = db.CharacteristicType.Include(c => c.CharacteristicTypeLink).Where(c => c.FullSequenceApplicable)
                .Select(c => new { Value = c.Id, Text = c.Name, CharacteristicTypeLink = c.CharacteristicTypeLink.Select(ctl => ctl.Id) }).ToList();
            var links = db.Link.Include(l => l.CharacteristicTypeLink)
                .Select(l => new { Value = l.Id, Text = l.Name, CharacteristicTypeLink = l.CharacteristicTypeLink.Select(ctl => ctl.Id) }).ToList();

            var pieceTypeIds = db.PieceType.Where(p => p.NatureId == Aliases.Nature.Genetic
                                         && p.Id != Aliases.PieceType.FullGenome
                                         && p.Id != Aliases.PieceType.ChloroplastGenome
                                         && p.Id != Aliases.PieceType.MitochondrionGenome).Select(p => p.Id);

            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(matters) }, 
                    { "characteristicTypes", characteristicTypes },  
                    { "links", links },
                    { "notationsFiltered", new SelectList(db.Notation.Where(n => n.NatureId == Aliases.Nature.Genetic), "id", "name") },
                    { "natureId", Aliases.Nature.Genetic },
                    { "pieceTypes", pieceTypeRepository.GetSelectListWithNature(pieceTypeIds, pieceTypeIds) }
                };
        }
    }
}