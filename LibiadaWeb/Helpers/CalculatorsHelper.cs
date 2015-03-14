namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
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
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

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
            featureRepository = new FeatureRepository(db);
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
            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList() }, 
                    { "characteristicTypes", GetCharacteristicTypes(filter) },
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
            var sequenceIds = db.Subsequence.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId);
            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            var featureIds = db.Feature.Where(p => p.NatureId == Aliases.Nature.Genetic
                                         && p.Id != Aliases.Feature.FullGenome
                                         && p.Id != Aliases.Feature.ChloroplastGenome
                                         && p.Id != Aliases.Feature.MitochondrionGenome).Select(p => p.Id);

            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(matters) }, 
                    { "characteristicTypes", GetCharacteristicTypes(c => c.FullSequenceApplicable) },  
                    { "notationsFiltered", new SelectList(db.Notation.Where(n => n.NatureId == Aliases.Nature.Genetic), "id", "name") },
                    { "natureId", Aliases.Nature.Genetic },
                    { "features", featureRepository.GetSelectListWithNature(featureIds, featureIds) }
                };
        }

        /// <summary>
        /// The get characteristic types.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetCharacteristicTypes(Func<CharacteristicType, bool> filter)
        {
            var characteristicTypes = db.CharacteristicType.Include(c => c.CharacteristicTypeLink).Where(filter).OrderBy(c => c.Name)
                .Select(c => new CharacteristicData(c.Id, c.Name, c.CharacteristicTypeLink.OrderBy(ctl => ctl.LinkId).Select(ctl => new CharacteristicLinkData(ctl.Id)).ToList())).ToList();

            var links = db.Link.Include(l => l.CharacteristicTypeLink)
                .Select(l => new { Value = l.Id, Text = l.Name, CharacteristicTypeLink = l.CharacteristicTypeLink.Select(ctl => ctl.Id) }).ToList();

            foreach (var characteristicType in characteristicTypes)
            {
                foreach (var characteristicLink in characteristicType.CharacteristicLinks)
                {
                    foreach (var link in links)
                    {
                        if (link.CharacteristicTypeLink.Contains(characteristicLink.CharacteristicTypeLinkId))
                        {
                            characteristicLink.Value = link.Value.ToString();
                            characteristicLink.Text = link.Text;
                            break;
                        }
                    }
                }
            }

            return characteristicTypes;
        }
    }
}