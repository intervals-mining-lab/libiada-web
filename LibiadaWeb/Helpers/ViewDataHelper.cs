namespace LibiadaWeb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using LibiadaCore.Core.Characteristics.Calculators.AccordanceCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.BinaryCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;
    using LibiadaCore.Extensions;

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

            data.Add("natures", natures);
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
        /// <param name="characteristicsType">
        /// The characteristics category.
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
        public Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsType, int minimumSelectedMatters, int maximumSelectedMatters, string submitName)
        {
            Dictionary<string, object> data = FillViewData(minimumSelectedMatters, maximumSelectedMatters, submitName);

            List<CharacteristicData> characteristicTypes;

            switch (characteristicsType)
            {
                case CharacteristicCategory.Full:
                    characteristicTypes = GetFullCharacteristicTypes();
                    break;
                case CharacteristicCategory.Congeneric:
                    characteristicTypes = GetCongenericCharacteristicTypes();
                    break;
                case CharacteristicCategory.Accordance:
                    characteristicTypes = GetAccordanceCharacteristicTypes();
                    break;
                case CharacteristicCategory.Binary:
                    characteristicTypes = GetBinaryCharacteristicTypes();
                    break;
                default:
                    throw new ArgumentException("Unknown characteristic category");
            }

            data.Add("characteristicTypes", characteristicTypes);

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
            var features = ArrayExtensions.ToArray<Feature>().Where(f => f.GetNature() == Nature.Genetic).ToArray();
            var selectedFeatures = features.Where(f => f != Feature.NonCodingSequence);
            var characteristicTypes = GetFullCharacteristicTypes();

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
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetFullCharacteristicTypes()
        {
            var characteristicTypeRepository = new FullCharacteristicRepository(db);

            Link[] links;
            FullCharacteristic[] characteristics;
            if (UserHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<FullCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableFullCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (FullCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = characteristicTypeRepository.FullCharacteristicLinks
                    .Where(cl => cl.FullCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }

        /// <summary>
        /// The get congeneric characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetCongenericCharacteristicTypes()
        {
            var characteristicTypeRepository = new CongenericCharacteristicRepository(db);

            Link[] links;
            CongenericCharacteristic[] characteristics;
            if (UserHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<CongenericCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableCongenericCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (CongenericCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = characteristicTypeRepository.CongenericCharacteristicLinks
                    .Where(cl => cl.CongenericCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }

        /// <summary>
        /// The get accordance characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetAccordanceCharacteristicTypes()
        {
            var characteristicTypeRepository = new AccordanceCharacteristicRepository(db);

            Link[] links;
            AccordanceCharacteristic[] characteristics;
            if (UserHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<AccordanceCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableAccordanceCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (AccordanceCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = characteristicTypeRepository.AccordanceCharacteristicLinks
                    .Where(cl => cl.AccordanceCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
        }

        /// <summary>
        /// The get binary characteristic types.
        /// </summary>
        /// <returns>
        /// The <see cref="List{CharacteristicData}"/>.
        /// </returns>
        public List<CharacteristicData> GetBinaryCharacteristicTypes()
        {
            var characteristicTypeRepository = new BinaryCharacteristicRepository(db);

            Link[] links;
            BinaryCharacteristic[] characteristics;
            if (UserHelper.IsAdmin())
            {
                links = ArrayExtensions.ToArray<Link>();
                characteristics = ArrayExtensions.ToArray<BinaryCharacteristic>();
            }
            else
            {
                links = Aliases.UserAvailableLinks.ToArray();
                characteristics = Aliases.UserAvailableBinaryCharacteristics.ToArray();
            }

            var result = new List<CharacteristicData>();

            foreach (BinaryCharacteristic characteristic in characteristics)
            {
                List<LinkSelectListItem> linkSelectListItems = characteristicTypeRepository.BinaryCharacteristicLinks
                    .Where(cl => cl.BinaryCharacteristic == characteristic && links.Contains(cl.Link))
                    .Select(ctl => new LinkSelectListItem(ctl.Id, ctl.Link.ToString(), ctl.Link.GetDisplayValue()))
                    .ToList();

                result.Add(new CharacteristicData((byte)characteristic, characteristic.GetDisplayValue(), linkSelectListItems));
            }

            return result;
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
            return new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", minimumSelectedMatters },
                    { "maximumSelectedMatters", maximumSelectedMatters },
                    { "matters", matterRepository.GetMatterSelectList(filter) },
                    { "radiobuttonsForMatters", maximumSelectedMatters == 1 && minimumSelectedMatters == 1 },
                    { "submitName", submitName }
                };
        }
    }
}
