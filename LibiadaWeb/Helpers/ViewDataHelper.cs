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
    /// Class filling data for ViewBag.
    /// </summary>
    public class ViewDataHelper
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
        /// Initializes a new instance of the <see cref="ViewDataHelper"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public ViewDataHelper(LibiadaWebEntities db)
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
        /// <param name="mattersCheckboxes">
        /// Flag, identifying whether to creates checkboxes or radiobutton table for matters
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillCalculationData(Func<CharacteristicType, bool> filter, int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes)
        {
            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var data = FillMattersData(minimumSelectedMatters, maximumSelectedMatters, mattersCheckboxes, m => true);

            data.Add("characteristicTypes", GetCharacteristicTypes(filter));
            data.Add("natures", new SelectList(db.Nature, "id", "name"));
            data.Add("notations", notationRepository.GetSelectListWithNature());
            data.Add("languages", new SelectList(db.Language, "id", "name"));
            data.Add("translators", translators);

            return data;
        }

        /// <summary>
        /// The get subsequences calculation data.
        /// </summary>
        /// <param name="minimumSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <param name="mattersCheckboxes">
        /// Flag, identifying whether to creates checkboxes or radiobutton table for matters
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> GetSubsequencesCalculationData(int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes)
        {
            var featureIds = db.Feature.Where(p => p.NatureId == Aliases.Nature.Genetic
                                         && p.Id != Aliases.Feature.FullGenome
                                         && p.Id != Aliases.Feature.ChloroplastGenome
                                         && p.Id != Aliases.Feature.MitochondrionGenome).Select(p => p.Id);

            var sequenceIds = db.Subsequence.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToList();

            var data = FillMattersData(minimumSelectedMatters, maximumSelectedMatters, mattersCheckboxes, m => matterIds.Contains(m.Id));

            data.Add("characteristicTypes", GetCharacteristicTypes(c => c.FullSequenceApplicable));
            data.Add("notationsFiltered", new SelectList(db.Notation.Where(n => n.NatureId == Aliases.Nature.Genetic), "id", "name"));
            data.Add("natureId", Aliases.Nature.Genetic);
            data.Add("features", featureRepository.GetSelectListWithNature(featureIds, featureIds));

            return data;
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

        /// <summary>
        /// The fill matters data.
        /// </summary>
        /// <param name="minimumSelectedMatters">
        /// The minimum selected matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum selected matters.
        /// </param>
        /// <param name="mattersCheckboxes">
        /// The matters checkboxes.
        /// </param>
        /// <param name="filter">
        /// Filter for matters.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillMattersData(int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes, Func<Matter, bool> filter)
        {
            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(filter) },
                    { "mattersCheckboxes", mattersCheckboxes }
                };
        }
    }
}
