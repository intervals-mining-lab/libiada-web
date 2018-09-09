namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Models.CalculatorsData;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

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
        /// Initializes a new instance of the <see cref="ViewDataHelper"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public ViewDataHelper(LibiadaWebEntities db)
        {
            this.db = db;
            matterRepository = new MatterRepository(db);
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

            IEnumerable<RemoteDb> remoteDbs = EnumExtensions.ToArray<RemoteDb>();
            IEnumerable<SequenceType> sequenceTypes = EnumExtensions.ToArray<SequenceType>();
            IEnumerable<Group> groups = EnumExtensions.ToArray<Group>();

            if (AccountHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = EnumExtensions.ToArray<Notation>();
            }
            else
            {
                natures = new[] { Nature.Genetic }.ToSelectList();
                notations = new[] { Notation.Nucleotides };
                remoteDbs = EnumExtensions.ToArray<RemoteDb>().Where(rd => rd.GetNature() == Nature.Genetic);
                sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
                groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            }

            return new Dictionary<string, object>
                       {
                           { "matters", matterRepository.GetMatterSelectList() },
                           { "natures", natures },
                           { "notations", notations.ToSelectListWithNature() },
                           { "remoteDbs", remoteDbs.ToSelectListWithNature() },
                           { "sequenceTypes", sequenceTypes.ToSelectListWithNature() },
                           { "groups", groups.ToSelectListWithNature() },
                           { "languages", EnumHelper.GetSelectList(typeof(Language)) },
                           { "translators", EnumHelper.GetSelectList(typeof(Translator)) }
                       };
        }

        /// <summary>
        /// Fills view data.
        /// </summary>
        /// <param name="minSelectedMatters">
        /// The minimum selected matters.
        /// </param>
        /// <param name="maxSelectedMatters">
        /// The maximum selected matters.
        /// </param>
        /// <param name="filter">
        /// The matters filter.
        /// </param>
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(int minSelectedMatters, int maxSelectedMatters, Func<Matter, bool> filter, string submitName)
        {
            Dictionary<string, object> data = GetMattersData(minSelectedMatters, maxSelectedMatters, filter);

            IEnumerable<SelectListItem> natures;
            IEnumerable<Notation> notations;
            IEnumerable<SequenceType> sequenceTypes;
            IEnumerable<Group> groups;

            if (AccountHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                notations = EnumExtensions.ToArray<Notation>();
                sequenceTypes = EnumExtensions.ToArray<SequenceType>();
                groups = EnumExtensions.ToArray<Group>();
            }
            else
            {
                natures = new[] { Nature.Genetic }.ToSelectList();
                notations = new[] { Notation.Nucleotides };
                sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
                groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            }

            data.Add("submitName", submitName);
            data.Add("natures", natures);
            data.Add("notations", notations.ToSelectListWithNature());
            data.Add("languages", EnumHelper.GetSelectList(typeof(Language)));
            data.Add("translators", EnumHelper.GetSelectList(typeof(Translator)));
            data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature(true));
            data.Add("groups", groups.ToSelectListWithNature(true));

            return data;
        }

        /// <summary>
        /// Fills view data.
        /// </summary>
        /// <param name="minSelectedMatters">
        /// The minimum selected matters.
        /// </param>
        /// <param name="maxSelectedMatters">
        /// The maximum selected matters.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(int minSelectedMatters, int maxSelectedMatters)
        {
            Dictionary<string, object> data = GetMattersData(minSelectedMatters, maxSelectedMatters, m => true);

            IEnumerable<SelectListItem> natures;
            IEnumerable<SequenceType> sequenceTypes;
            IEnumerable<Group> groups;

            if (AccountHelper.IsAdmin())
            {
                natures = EnumHelper.GetSelectList(typeof(Nature));
                sequenceTypes = EnumExtensions.ToArray<SequenceType>();
                groups = EnumExtensions.ToArray<Group>();
            }
            else
            {
                natures = new[] { Nature.Genetic }.ToSelectList();
                sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
                groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            }

            data.Add("natures", natures);
            data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature(true));
            data.Add("groups", groups.ToSelectListWithNature(true));

            return data;
        }

        /// <summary>
        /// Fills view data.
        /// </summary>
        /// <param name="minSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maxSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(int minSelectedMatters, int maxSelectedMatters, string submitName)
        {
            return FillViewData(minSelectedMatters, maxSelectedMatters, m => true, submitName);
        }

        /// <summary>
        /// The fill calculation data.
        /// </summary>
        /// <param name="characteristicsType">
        /// The characteristics category.
        /// </param>
        /// <param name="minSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maxSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsType, int minSelectedMatters, int maxSelectedMatters, string submitName)
        {
            Dictionary<string, object> data = FillViewData(minSelectedMatters, maxSelectedMatters, submitName);

            List<CharacteristicSelectListItem> characteristicTypes;
            var characteristicsDictionary = new Dictionary<(int, int, int), int>();

            switch (characteristicsType)
            {
                case CharacteristicCategory.Full:
                    characteristicTypes = FullCharacteristicRepository.Instance.GetCharacteristicTypes();
                    var fullCharacteristics = FullCharacteristicRepository.Instance.CharacteristicLinks.ToArray();
                    foreach (var characteristic in fullCharacteristics)
                    {
                        characteristicsDictionary.Add(((int)characteristic.FullCharacteristic, (int)characteristic.Link, (int)characteristic.ArrangementType), characteristic.Id);
                    }

                    break;
                case CharacteristicCategory.Congeneric:
                    characteristicTypes = CongenericCharacteristicRepository.Instance.GetCharacteristicTypes();
                    var congenericCharacteristics = FullCharacteristicRepository.Instance.CharacteristicLinks.ToArray();
                    foreach (var characteristic in congenericCharacteristics)
                    {
                        characteristicsDictionary.Add(((int)characteristic.FullCharacteristic, (int)characteristic.Link, (int)characteristic.ArrangementType), characteristic.Id);
                    }
                    break;
                case CharacteristicCategory.Accordance:
                    characteristicTypes = AccordanceCharacteristicRepository.Instance.GetCharacteristicTypes();
                    break;
                case CharacteristicCategory.Binary:
                    characteristicTypes = BinaryCharacteristicRepository.Instance.GetCharacteristicTypes();
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(characteristicsType), (int)characteristicsType, typeof(CharacteristicCategory));
            }

            data.Add("characteristicTypes", characteristicTypes);
            data.Add("characteristicsDictionary", characteristicsDictionary);

            return data;
        }

        /// <summary>
        /// Fills subsequences calculation data dictionary.
        /// </summary>
        /// <param name="minSelectedMatters">
        /// The minimum Selected Matters.
        /// </param>
        /// <param name="maxSelectedMatters">
        /// The maximum Selected Matters.
        /// </param>
        /// <param name="submitName">
        /// The submit button name.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        public Dictionary<string, object> FillSubsequencesViewData(int minSelectedMatters, int maxSelectedMatters, string submitName)
        {
            var sequenceIds = db.Subsequence.Select(s => s.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToList();

            var data = GetMattersData(minSelectedMatters, maxSelectedMatters, m => matterIds.Contains(m.Id));

            var geneticNotations = EnumExtensions.ToArray<Notation>().Where(n => n.GetNature() == Nature.Genetic);
            var sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
            var groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
            var features = EnumExtensions.ToArray<Feature>().Where(f => f.GetNature() == Nature.Genetic).ToArray();
            var selectedFeatures = features.Where(f => f != Feature.NonCodingSequence);
            var characteristicTypes = FullCharacteristicRepository.Instance.GetCharacteristicTypes();

            data.Add("submitName", submitName);
            data.Add("characteristicTypes", characteristicTypes);
            data.Add("notations", geneticNotations.ToSelectListWithNature());
            data.Add("nature", (byte)Nature.Genetic);
            data.Add("features", features.ToSelectListWithNature(selectedFeatures));
            data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature(true));
            data.Add("groups", groups.ToSelectListWithNature(true));

            return data;
        }

        /// <summary>
        /// Fills matters data dictionary.
        /// </summary>
        /// <param name="minSelectedMatters">
        /// The minimum selected matters.
        /// </param>
        /// <param name="maxSelectedMatters">
        /// The maximum selected matters.
        /// </param>
        /// <param name="filter">
        /// Filter for matters.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{String, Object}"/>.
        /// </returns>
        private Dictionary<string, object> GetMattersData(int minSelectedMatters, int maxSelectedMatters, Func<Matter, bool> filter)
        {
            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minSelectedMatters },
                    { "maximumSelectedMatters", maxSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(filter) }
                };
        }
    }
}
