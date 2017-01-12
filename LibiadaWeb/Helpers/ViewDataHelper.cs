namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Extensions;
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
        /// The characteristic type link repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

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
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// Fills matter creation data.
        /// </summary>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillMatterCreationData()
        {
            IEnumerable<SelectListItem> natures;
            IEnumerable<Notation> notations;
            IEnumerable<SequenceType> sequenceTypes;
            IEnumerable<Group> groups;
            IEnumerable<RemoteDb> remoteDbs;

            if (UserHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = ArrayExtensions.ToArray<Notation>();
                remoteDbs = ArrayExtensions.ToArray<RemoteDb>();
                sequenceTypes = ArrayExtensions.ToArray<SequenceType>();
                groups = ArrayExtensions.ToArray<Group>();
            }
            else
            {
                natures = new[] { Nature.Genetic }.ToSelectList();
                notations = new[] { Notation.Nucleotides };
                remoteDbs = ArrayExtensions.ToArray<RemoteDb>().Where(rd => rd.GetNature() == Nature.Genetic);
                sequenceTypes = ArrayExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
                groups = ArrayExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            }

            return new Dictionary<string, object>
                           {
                                   { "matters", matterRepository.GetMatterSelectList() },
                                   { "natures", natures },
                                   { "notations", notations.ToSelectListWithNature() },
                                   { "languages", EnumHelper.GetSelectList(typeof(Language)) },
                                   { "remoteDbs", remoteDbs.ToSelectListWithNature() },
                                   { "translators", EnumHelper.GetSelectList(typeof(Translator)) },
                                   { "sequenceTypes", sequenceTypes.ToSelectListWithNature(true) },
                                   { "groups", groups.ToSelectListWithNature(true) }
                           };
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
            var data = GetMattersData(minimumSelectedMatters, maximumSelectedMatters, m => true, submitName);

            IEnumerable<SelectListItem> natures;
            IEnumerable<Notation> notations;
            IEnumerable<SequenceType> sequenceTypes;
            IEnumerable<Group> groups;

            if (UserHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = ArrayExtensions.ToArray<Notation>();
                sequenceTypes = ArrayExtensions.ToArray<SequenceType>();
                groups = ArrayExtensions.ToArray<Group>();
            }
            else
            {
                natures = new[] { Nature.Genetic }.ToSelectList();
                notations = new[] { Notation.Nucleotides };
                sequenceTypes = ArrayExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
                groups = ArrayExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            }

            data.Add("natures",  natures);
            data.Add("notations", notations.ToSelectListWithNature());
            data.Add("languages", EnumHelper.GetSelectList(typeof(Language)));
            data.Add("translators", EnumHelper.GetSelectList(typeof(Translator)));
            data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature(true));
            data.Add("groups", groups.ToSelectListWithNature(true));

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

            var geneticNotations = ArrayExtensions.ToArray<Notation>().Where(n => n.GetNature() == Nature.Genetic);
            var sequenceTypes = ArrayExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
            var groups = ArrayExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            var features = ArrayExtensions.ToArray<Feature>().Where(f => f.GetNature() == Nature.Genetic);
            var selectedFeatures = features.Where(f => f != Feature.NonCodingSequence);
            var characteristicTypes = GetCharacteristicTypes(c => c.FullSequenceApplicable);

            data.Add("characteristicTypes", characteristicTypes);
            data.Add("notations", geneticNotations.ToSelectListWithNature());
            data.Add("nature", (byte)Nature.Genetic);
            data.Add("features", features.ToSelectListWithNature(selectedFeatures));
            data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature(true));
            data.Add("groups", groups.ToSelectListWithNature(true));

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

            var links = UserHelper.IsAdmin() ? ArrayExtensions.ToArray<Link>() : new[] { Link.NotApplied, Link.Start, Link.Cycle };

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
    }
}
