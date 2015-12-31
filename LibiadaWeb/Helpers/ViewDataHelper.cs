namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Account;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Link = LibiadaCore.Core.Link;

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
        /// <param name="minimumSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <param name="mattersCheckboxes">
        /// Flag, identifying whether to creates checkboxes or radiobutton table for matters
        /// </param>
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes, string submitName)
        {
            var translators = new SelectList(db.Translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Not applied" });

            var data = FillMattersData(minimumSelectedMatters, maximumSelectedMatters, mattersCheckboxes, m => true, submitName);

            IEnumerable<Nature> natures;
            IEnumerable<object> notations;

            if (UserHelper.IsAdmin())
            {
                natures = db.Nature;
                notations = notationRepository.GetSelectListWithNature();
            }
            else
            {
                natures = db.Nature.Where(n => n.Id == Aliases.Nature.Genetic);
                notations = notationRepository.GetSelectListWithNature(new List<int> { Aliases.Notation.Nucleotide });
            }

            data.Add("natures", new SelectList(natures.OrderBy(n => n.Id), "id", "name"));
            data.Add("notations", notations);
            data.Add("languages", new SelectList(db.Language, "id", "name"));
            data.Add("translators", translators);

            return data;
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
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(Func<CharacteristicType, bool> filter, int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes, string submitName)
        {
            var data = FillViewData(minimumSelectedMatters, maximumSelectedMatters, mattersCheckboxes, submitName);
            data.Add("characteristicTypes", GetCharacteristicTypes(filter));

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
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> GetSubsequencesViewData(int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes, string submitName)
        {
            var featureIds = db.Feature.Where(f => f.NatureId == Aliases.Nature.Genetic && !f.Complete).Select(f => f.Id);

            var sequenceIds = db.Subsequence.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToList();

            var data = FillMattersData(minimumSelectedMatters, maximumSelectedMatters, mattersCheckboxes, m => matterIds.Contains(m.Id), submitName);

            var geneticNotations = db.Notation.Where(n => n.NatureId == Aliases.Nature.Genetic).Select(n => n.Id).ToList();

            data.Add("characteristicTypes", GetCharacteristicTypes(c => c.FullSequenceApplicable));
            data.Add("notations", notationRepository.GetSelectListWithNature(geneticNotations));
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

            var links = Enum.GetValues(typeof(Link)).Cast<Link>();

            if (!UserHelper.IsAdmin())
            {
                links = new List<Link> { Link.NotApplied, Link.Start, Link.Cycle };
            }

            var characteristicTypeLinks = db.CharacteristicTypeLink.ToList();

            var linksData = links.Select(l => new
                                                {
                                                    Value = (int)l, 
                                                    Text = EnumHelper.GetDisplayValue(l), 
                                                    CharacteristicTypeLink = characteristicTypeLinks.Where(ctl => ctl.LinkId == (int)l).Select(ctl => ctl.Id)
                                                }).ToList();

            foreach (var characteristicType in characteristicTypes)
            {
                for (int i = 0; i < characteristicType.CharacteristicLinks.Count; i++)
                {
                    var characteristicLink = characteristicType.CharacteristicLinks[i];
                    foreach (var link in linksData)
                    {
                        if (link.CharacteristicTypeLink.Contains(characteristicLink.CharacteristicTypeLinkId))
                        {
                            characteristicLink.Value = link.Value.ToString();
                            characteristicLink.Text = link.Text;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(characteristicLink.Value))
                    {
                        characteristicType.CharacteristicLinks.Remove(characteristicLink);
                        i--;
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
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillMattersData(int minimumSelectedMatters, int maximumSelectedMatters, bool mattersCheckboxes, Func<Matter, bool> filter, string submitName)
        {
            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(filter) },
                    { "mattersCheckboxes", mattersCheckboxes },
                    { "submitName", submitName }
                };
        }
    }
}
