namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Account;
    using LibiadaWeb.Models.CalculatorsData;
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
        /// The characteristic type link repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The remote db repository.
        /// </summary>
        private readonly RemoteDbRepository remoteDbRepository;

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
            notationRepository = new NotationRepository();
            featureRepository = new FeatureRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            remoteDbRepository = new RemoteDbRepository(db);
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
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(int minimumSelectedMatters, int maximumSelectedMatters, string submitName)
        {
            var translators = new SelectList(db.Translator, "id", "name").ToList();

            var data = GetMattersData(minimumSelectedMatters, maximumSelectedMatters, m => true, submitName);

            IEnumerable<SelectListItem> natures;
            IEnumerable<object> notations;
            IEnumerable<SelectListItemWithNature> sequenceTypes;
            IEnumerable<SelectListItemWithNature> groups;

            if (UserHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = notationRepository.GetSelectListWithNature();
                sequenceTypes = EnumExtensions.ToArray<SequenceType>()
                    .Select(st => new SelectListItemWithNature { Text = st.GetDisplayValue(), Value = st.GetDisplayValue(), Nature = (byte)st.GetNature() });
                groups = EnumExtensions.ToArray<Group>()
                    .Select(g => new SelectListItemWithNature { Text = g.GetDisplayValue(), Value = g.GetDisplayValue(), Nature = (byte)g.GetNature() });
            }
            else
            {
                natures = new List<Nature> { Nature.Genetic }.ToSelectList();
                notations = notationRepository.GetSelectListWithNature(new List<Notation> { Notation.Nucleotides });
                sequenceTypes = EnumExtensions.ToArray<SequenceType>()
                    .Where(st => st.GetNature() == Nature.Genetic)
                    .Select(st => new SelectListItemWithNature { Text = st.GetDisplayValue(), Value = st.GetDisplayValue(), Nature = (byte)st.GetNature() });
                groups = EnumExtensions.ToArray<Group>()
                    .Where(g => g.GetNature() == Nature.Genetic)
                    .Select(g => new SelectListItemWithNature { Text = g.GetDisplayValue(), Value = g.GetDisplayValue(), Nature = (byte)g.GetNature() });
            }

            data.Add("natures",  natures);
            data.Add("notations", notations);
            data.Add("languages", new SelectList(db.Language, "id", "name"));
            data.Add("translators", translators);
            data.Add("sequenceTypes", sequenceTypes);
            data.Add("groups", groups);

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
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(Func<CharacteristicType, bool> filter, int minimumSelectedMatters, int maximumSelectedMatters, string submitName)
        {
            var data = FillViewData(minimumSelectedMatters, maximumSelectedMatters, submitName);
            data.Add("characteristicTypes", GetCharacteristicTypes(filter));

            return data;
        }

        /// <summary>
        /// Fills subsequences calculation data dictionary.
        /// </summary>
        /// <param name="minimumSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillSubsequencesViewData(int minimumSelectedMatters, int maximumSelectedMatters, string submitName)
        {
            var sequenceIds = db.Subsequence.Select(s => s.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToList();

            var data = GetMattersData(minimumSelectedMatters, maximumSelectedMatters, m => matterIds.Contains(m.Id), submitName);

            var geneticNotations = EnumExtensions.ToArray<Notation>().Where(n => n.GetNature() == Nature.Genetic).ToList();
            var characteristicTypes = GetCharacteristicTypes(c => c.FullSequenceApplicable);
            var featureIds = featureRepository.Features.Where(f => f.Nature == Nature.Genetic && !f.Complete).Select(f => f.Id);
            var sequenceTypes = EnumExtensions.ToArray<SequenceType>()
                    .Where(st => st.GetNature() == Nature.Genetic)
                    .Select(st => new SelectListItemWithNature { Text = st.GetDisplayValue(), Value = st.GetDisplayValue(), Nature = (byte)st.GetNature() });
            var groups = EnumExtensions.ToArray<Group>()
                .Where(g => g.GetNature() == Nature.Genetic)
                .Select(g => new SelectListItemWithNature { Text = g.GetDisplayValue(), Value = g.GetDisplayValue(), Nature = (byte)g.GetNature() });

            data.Add("characteristicTypes", characteristicTypes);
            data.Add("notations", notationRepository.GetSelectListWithNature(geneticNotations));
            data.Add("nature", (byte)Nature.Genetic);
            data.Add("features", featureRepository.GetSelectListWithNature(featureIds, featureIds));
            data.Add("sequenceTypes", sequenceTypes);
            data.Add("groups", groups);

            return data;
        }

        /// <summary>
        /// Gets characteristics types.
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
                .Select(c => new CharacteristicData(c.Id, c.Name, c.CharacteristicTypeLink.OrderBy(ctl => ctl.Link).Select(ctl => new CharacteristicLinkData(ctl.Id)).ToList())).ToList();

            var links = UserHelper.IsAdmin() ? EnumExtensions.ToArray<Link>() : new[] { Link.NotApplied, Link.Start, Link.Cycle };

            var characteristicTypeLinks = characteristicTypeLinkRepository.CharacteristicTypeLinks;

            var linksData = links.Select(l => new
                                                {
                                                    Value = (int)l,
                                                    Text = l.GetDisplayValue(),
                                                    CharacteristicTypeLink = characteristicTypeLinks.Where(ctl => ctl.Link == l).Select(ctl => ctl.Id)
                                                });

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
        /// Fills matters data dictionary.
        /// </summary>
        /// <param name="minimumSelectedMatters">
        /// The minimum selected matters.
        /// </param>
        /// <param name="maximumSelectedMatters">
        /// The maximum selected matters.
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
        public Dictionary<string, object> GetMattersData(int minimumSelectedMatters, int maximumSelectedMatters, Func<Matter, bool> filter, string submitName)
        {
            var radiobuttonsForMatters = maximumSelectedMatters == 1 && minimumSelectedMatters == 1;

            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(filter) },
                    { "radiobuttonsForMatters", radiobuttonsForMatters },
                    { "submitName", submitName }
                };
        }

        /// <summary>
        /// Fills matter creation data.
        /// </summary>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillMatterCreationData()
        {
            var translators = new SelectList(db.Translator, "id", "name").ToList();

            IEnumerable<SelectListItemWithNature> notations;
            IEnumerable<SelectListItem> natures;
            IEnumerable<SelectListItemWithNature> sequenceTypes;
            IEnumerable<SelectListItemWithNature> groups;

            if (UserHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = notationRepository.GetSelectListWithNature();
                
                sequenceTypes = EnumExtensions.ToArray<SequenceType>()
                    .Select(st => new SelectListItemWithNature { Text = st.GetDisplayValue(), Value = st.GetDisplayValue(), Nature = (byte)st.GetNature() });
                groups = EnumExtensions.ToArray<Group>()
                    .Select(g => new SelectListItemWithNature { Text = g.GetDisplayValue(), Value = g.GetDisplayValue(), Nature = (byte)g.GetNature() });
            }
            else
            {
                natures = new List<Nature> { Nature.Genetic }.ToSelectList();
                notations = notationRepository.GetSelectListWithNature(new List<Notation> { Notation.Nucleotides });
                sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic)
                    .Select(st => new SelectListItemWithNature { Text = st.GetDisplayValue(), Value = st.GetDisplayValue(), Nature = (byte)st.GetNature() });
                groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic)
                    .Select(g => new SelectListItemWithNature { Text = g.GetDisplayValue(), Value = g.GetDisplayValue(), Nature = (byte)g.GetNature() });
            }

            return new Dictionary<string, object>
                           {
                                   { "matters", matterRepository.GetMatterSelectList() },
                                   { "natures", natures },
                                   { "notations", notations },
                                   { "languages", new SelectList(db.Language, "id", "name") },
                                   { "remoteDbs", remoteDbRepository.GetSelectListWithNature() },
                                   { "translators", translators },
                                   { "sequenceTypes", sequenceTypes },
                                   { "groups", groups }
                           };
        }
    }
}
