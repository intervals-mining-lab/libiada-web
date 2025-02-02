namespace Libiada.Web.Helpers;

using System.ComponentModel;
using System.Security.Claims;

using Libiada.Core.Music;

using Libiada.Web.Extensions;
using Libiada.Web.Models.CalculatorsData;

using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Attributes;

using EnumExtensions = Core.Extensions.EnumExtensions;
using Libiada.Database.Models.Repositories.Sequences;

/// <summary>
/// Class filling data for Views.
/// </summary>
public class ViewDataHelper : IViewDataHelper
{
    /// <summary>
    /// The database model.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;
    private readonly ResearchObjectsCache cache;

    /// <summary>
    /// The current user.
    /// </summary>
    private readonly ClaimsPrincipal user;
    private readonly IFullCharacteristicRepository fullCharacteristicModelRepository;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicModelRepository;
    private readonly IAccordanceCharacteristicRepository accordanceCharacteristicModelRepository;
    private readonly IBinaryCharacteristicRepository binaryCharacteristicModelRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewDataHelper"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public ViewDataHelper(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                          ResearchObjectsCache cache,
                          ClaimsPrincipal user,
                          IFullCharacteristicRepository fullCharacteristicRepository,
                          ICongenericCharacteristicRepository congenericCharacteristicRepository,
                          IAccordanceCharacteristicRepository accordanceCharacteristicRepository,
                          IBinaryCharacteristicRepository binaryCharacteristicRepository)
    {
        this.db = dbFactory.CreateDbContext();
        this.cache = cache;
        this.user = user;
        this.fullCharacteristicModelRepository = fullCharacteristicRepository;
        this.congenericCharacteristicModelRepository = congenericCharacteristicRepository;
        this.accordanceCharacteristicModelRepository = accordanceCharacteristicRepository;
        this.binaryCharacteristicModelRepository = binaryCharacteristicRepository;
    }

    /// <summary>
    /// Fills research object creation data.
    /// </summary>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillResearchObjectCreationViewData()
    {
        IEnumerable<SelectListItem> natures;
        IEnumerable<Notation> notations;

        IEnumerable<RemoteDb> remoteDbs = EnumExtensions.ToArray<RemoteDb>();
        IEnumerable<SequenceType> sequenceTypes = EnumExtensions.ToArray<SequenceType>();
        IEnumerable<Group> groups = EnumExtensions.ToArray<Group>();

        if (user.IsAdmin())
        {
            natures = Extensions.EnumExtensions.GetSelectList<Nature>();
            notations = EnumExtensions.ToArray<Notation>();
        }
        else
        {
            natures = new[] { Nature.Genetic }.ToSelectList();
            notations = [Notation.Nucleotides];
            remoteDbs = EnumExtensions.ToArray<RemoteDb>().Where(rd => rd.GetNature() == Nature.Genetic);
            sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
            groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
        }

        return new Dictionary<string, object>
        {
            { "researchObjects", SelectListHelper.GetResearchObjectSelectList(cache) },
            { "natures", natures },
            { "notations", notations.ToSelectListWithNature() },
            { "remoteDbs", remoteDbs.ToSelectListWithNature() },
            { "sequenceTypes", sequenceTypes.ToSelectListWithNature() },
            { "groups", groups.ToSelectListWithNature() },
            { "multisequences", SelectListHelper.GetMultisequenceSelectList(db) },
            { "languages", Extensions.EnumExtensions.GetSelectList<Language>() },
            { "translators", Extensions.EnumExtensions.GetSelectList<Translator>() },
            { "trajectories", EnumExtensions.SelectAllWithAttribute<ImageOrderExtractor>(typeof(ImageOrderExtractorAttribute)).ToSelectList() }
        };
    }

    /// <summary>
    /// Fills view data.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="filter">
    /// The research objects filter.
    /// </param>
    /// <param name="submitName">
    /// The submit button name.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillViewData(int minSelectedResearchObjects,
                                                   int maxSelectedResearchObjects,
                                                   Func<ResearchObject, bool> filter,
                                                   string submitName)
    {
        return FillViewData(minSelectedResearchObjects, maxSelectedResearchObjects, filter, m => false, submitName);
    }

    /// <summary>
    /// Fills view data.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="filter">
    /// The research objects filter.
    /// </param>
    /// /// <param name="selectionFilter">
    /// The research objects selection filter.
    /// </param>
    /// <param name="submitName">
    /// The submit button name.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillViewData(int minSelectedResearchObjects,
                                                   int maxSelectedResearchObjects,
                                                   Func<ResearchObject, bool> filter,
                                                   Func<ResearchObject, bool> selectionFilter,
                                                   string submitName)
    {
        Dictionary<string, object> data = GetResearchObjectsData(minSelectedResearchObjects, maxSelectedResearchObjects, filter, selectionFilter);

        IEnumerable<SelectListItem> natures;
        IEnumerable<Notation> notations;
        IEnumerable<SequenceType> sequenceTypes;
        IEnumerable<Group> groups;

        if (user.IsAdmin())
        {
            natures = Extensions.EnumExtensions.GetSelectList<Nature>();
            notations = EnumExtensions.ToArray<Notation>();
            sequenceTypes = EnumExtensions.ToArray<SequenceType>();
            groups = EnumExtensions.ToArray<Group>();
        }
        else
        {
            natures = new[] { Nature.Genetic }.ToSelectList();
            notations = [Notation.Nucleotides];
            sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
            groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
        }

        data.Add("submitName", submitName);
        data.Add("natures", natures);
        data.Add("notations", notations.ToSelectListWithNature());
        data.Add("languages", Extensions.EnumExtensions.GetSelectList<Language>());
        data.Add("translators", Extensions.EnumExtensions.GetSelectList<Translator>());
        data.Add("pauseTreatments", Extensions.EnumExtensions.GetSelectList<PauseTreatment>());
        data.Add("trajectories", EnumExtensions.SelectAllWithAttribute<ImageOrderExtractor>(typeof(ImageOrderExtractorAttribute)).ToSelectList());
        data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature());
        data.Add("groups", groups.ToSelectListWithNature());

        return data;
    }

    /// <summary>
    /// Fills view data.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="submitName">
    /// The submit button name.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName)
    {
        return FillViewData(minSelectedResearchObjects, maxSelectedResearchObjects, m => true, submitName);
    }

    /// <summary>
    /// The fill calculation data.
    /// </summary>
    /// <param name="characteristicsType">
    /// The characteristics category.
    /// </param>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="submitName">
    /// The submit button name.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsType, int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName)
    {
        Dictionary<string, object> data = FillViewData(minSelectedResearchObjects, maxSelectedResearchObjects, submitName);
        return data.Concat(GetCharacteristicsData(characteristicsType)).ToDictionary(x => x.Key, y => y.Value);
    }

    /// <summary>
    /// Fills subsequences calculation data dictionary.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="submitName">
    /// The submit button name.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillSubsequencesViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName)
    {
        var sequenceIds = db.Subsequences.Select(s => s.SequenceId).Distinct();
        var researchObjectIds = db.CombinedSequenceEntities.Where(c => sequenceIds.Contains(c.Id)).Select(c => c.ResearchObjectId).ToList();

        Dictionary<string, object> data = GetResearchObjectsData(minSelectedResearchObjects, maxSelectedResearchObjects, m => researchObjectIds.Contains(m.Id));

        var geneticNotations = EnumExtensions.ToArray<Notation>().Where(n => n.GetNature() == Nature.Genetic);
        var sequenceTypes = EnumExtensions.ToArray<SequenceType>().Where(st => st.GetNature() == Nature.Genetic);
        var groups = EnumExtensions.ToArray<Group>().Where(g => g.GetNature() == Nature.Genetic);
        var features = EnumExtensions.ToArray<Feature>().Where(f => f.GetNature() == Nature.Genetic).ToArray();
        var selectedFeatures = features.Where(f => f != Feature.NonCodingSequence);
        data = data.Concat(GetCharacteristicsData(CharacteristicCategory.Full)).ToDictionary(x => x.Key, y => y.Value); ;
        data.Add("submitName", submitName);
        data.Add("notations", geneticNotations.ToSelectListWithNature());
        data.Add("nature", ((byte)Nature.Genetic).ToString());
        data.Add("features", features.ToSelectListWithNature(selectedFeatures));
        data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature());
        data.Add("groups", groups.ToSelectListWithNature());

        return data;
    }

    /// <summary>
    /// Get characteristics data (characteristics select list and dictionary).
    /// </summary>
    /// <param name="characteristicsCategory">
    /// The characteristics category.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    /// <exception cref="InvalidEnumArgumentException">
    /// Thrown if <see cref="CharacteristicCategory"/> is unknown.
    /// </exception>
    public Dictionary<string, object> GetCharacteristicsData(CharacteristicCategory characteristicsCategory)
    {
        List<CharacteristicSelectListItem> characteristicTypes;
        var characteristicsDictionary = new Dictionary<(short, short, short), short>();

        switch (characteristicsCategory)
        {
            case CharacteristicCategory.Full:
                var fullCharacteristicRepository = new Models.Repositories.Catalogs.FullCharacteristicRepository(db, user);
                characteristicTypes = fullCharacteristicRepository.GetCharacteristicTypes();

                var fullCharacteristics = fullCharacteristicModelRepository.CharacteristicLinks;
                foreach (var characteristic in fullCharacteristics)
                {
                    characteristicsDictionary.Add(((short)characteristic.FullCharacteristic,
                                                   (short)characteristic.Link,
                                                   (short)characteristic.ArrangementType),
                                                  characteristic.Id);
                }

                break;
            case CharacteristicCategory.Congeneric:
                var congenericCharacteristicRepository = new Models.Repositories.Catalogs.CongenericCharacteristicRepository(db, user);
                characteristicTypes = congenericCharacteristicRepository.GetCharacteristicTypes();

                var congenericCharacteristics = congenericCharacteristicModelRepository.CharacteristicLinks;
                foreach (var characteristic in congenericCharacteristics)
                {
                    characteristicsDictionary.Add(((short)characteristic.CongenericCharacteristic,
                                                   (short)characteristic.Link,
                                                   (short)characteristic.ArrangementType),
                                                  characteristic.Id);
                }

                break;
            case CharacteristicCategory.Accordance:
                var accordanceCharacteristicRepository = new Models.Repositories.Catalogs.AccordanceCharacteristicRepository(db, user);
                characteristicTypes = accordanceCharacteristicRepository.GetCharacteristicTypes();

                var accordanceCharacteristics = accordanceCharacteristicModelRepository.CharacteristicLinks;
                foreach (var characteristic in accordanceCharacteristics)
                {
                    characteristicsDictionary.Add(((short)characteristic.AccordanceCharacteristic, (short)characteristic.Link, 0), characteristic.Id);
                }

                break;
            case CharacteristicCategory.Binary:
                var binaryCharacteristicRepository = new Models.Repositories.Catalogs.BinaryCharacteristicRepository(db, user);
                characteristicTypes = binaryCharacteristicRepository.GetCharacteristicTypes();

                var binaryCharacteristics = binaryCharacteristicModelRepository.CharacteristicLinks;
                foreach (var characteristic in binaryCharacteristics)
                {
                    characteristicsDictionary.Add(((short)characteristic.BinaryCharacteristic, (short)characteristic.Link, 0), characteristic.Id);
                }

                break;
            default:
                throw new InvalidEnumArgumentException(nameof(characteristicsCategory), (byte)characteristicsCategory, typeof(CharacteristicCategory));
        }

        return new Dictionary<string, object>()
        {
            { "characteristicTypes", characteristicTypes },
            { "characteristicsDictionary", characteristicsDictionary }
        };
    }

    /// <summary>
    /// Fills research objects data dictionary.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> GetResearchObjectsData(int minSelectedResearchObjects, int maxSelectedResearchObjects)
    {
        Dictionary<string, object> data = GetResearchObjectsData(minSelectedResearchObjects, maxSelectedResearchObjects, m => true);

        // TODO: refactor to remove duplication with other methods or rewrite this whole class as builder
        IEnumerable<SelectListItem> natures;
        IEnumerable<SequenceType> sequenceTypes;
        IEnumerable<Group> groups;

        if (user.IsAdmin())
        {
            natures = Extensions.EnumExtensions.GetSelectList<Nature>();
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
        data.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature());
        data.Add("groups", groups.ToSelectListWithNature());

        return data;
    }

    /// <summary>
    /// Fills research objects data dictionary.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="filter">
    /// Filter for research objects.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    private Dictionary<string, object> GetResearchObjectsData(int minSelectedResearchObjects,
                                                      int maxSelectedResearchObjects,
                                                      Func<ResearchObject, bool> filter)
    {
        return GetResearchObjectsData(minSelectedResearchObjects, maxSelectedResearchObjects, filter, m => false);
    }

    /// <summary>
    /// Fills research objects data dictionary.
    /// </summary>
    /// <param name="minSelectedResearchObjects">
    /// The minimum selected number of research objects.
    /// </param>
    /// <param name="maxSelectedResearchObjects">
    /// The maximum selected number of research objects.
    /// </param>
    /// <param name="filter">
    /// Filter for research objects.
    /// </param>
    /// <param name="selectionFilter">
    /// Filter for research object selection.
    /// </param>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    private Dictionary<string, object> GetResearchObjectsData(int minSelectedResearchObjects,
                                                      int maxSelectedResearchObjects,
                                                      Func<ResearchObject, bool> filter,
                                                      Func<ResearchObject, bool> selectionFilter)
    {
        return new Dictionary<string, object>
        {
            { "minimumSelectedResearchObjects", minSelectedResearchObjects },
            { "maximumSelectedResearchObjects", maxSelectedResearchObjects },
            { "researchObjects", SelectListHelper.GetResearchObjectSelectList(filter, selectionFilter, cache) },
            { "sequenceGroups", SelectListHelper.GetSequenceGroupSelectList(db) }
        };
    }

    public void Dispose()
    {
        db.Dispose();
    }
}
