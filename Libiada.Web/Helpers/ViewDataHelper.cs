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
using Libiada.Database.Models;
using Libiada.Segmenter.Base.Seekers.Converters;

/// <summary>
/// Class filling data for Views.
/// </summary>
public class ViewDataHelper : IViewDataHelper
{
    /// <summary>
    /// The database model.
    /// </summary>
    private readonly LibiadaDatabaseEntities db;
    private readonly IResearchObjectsCache cache;

    /// <summary>
    /// The current user.
    /// </summary>
    private readonly ClaimsPrincipal user;
    private readonly IFullCharacteristicRepository fullCharacteristicModelRepository;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicModelRepository;
    private readonly IAccordanceCharacteristicRepository accordanceCharacteristicModelRepository;
    private readonly IBinaryCharacteristicRepository binaryCharacteristicModelRepository;

    private readonly Dictionary<string, object> viewData = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewDataHelper"/> class.
    /// </summary>
    /// <param name="dbFactory">
    /// Database context factory.
    /// </param>
    public ViewDataHelper(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                          IResearchObjectsCache cache,
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
    /// Adds the list of research objects table rows to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddResearchObjects()
    {
        return AddResearchObjects(m => true, m => false);
    }

    /// <summary>
    /// Adds the list of research objects table rows to the view data dictionary.
    /// </summary>
    /// <param name="filter">
    /// Filter for research objects.
    /// </param>
    /// <param name="selection">
    /// Function for determining which research objects should be selected.
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddResearchObjects(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selection)
    {
        IOrderedEnumerable<ResearchObject> researchObjects = cache.ResearchObjects
                                                                  .Where(filter)
                                                                  .OrderBy(m => m.Created);
        IEnumerable<ResearchObjectTableRow> researchObjectsList = researchObjects.Select(m => new ResearchObjectTableRow(m, selection(m)));

        viewData.Add("researchObjects", researchObjectsList);
        return this;
    }

    /// <summary>
    /// Adds natures select list to the view data dictionary.
    /// </summary>
    /// <param name="onlyGenetic">
    /// If set to <c>true</c> [includes only genetic nature].
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddNatures(bool onlyGenetic = false)
    {
        Nature[] natures = user.IsAdmin() || onlyGenetic ? EnumExtensions.ToArray<Nature>() : [Nature.Genetic];
        viewData.Add("natures", natures.ToSelectList());
        return this;
    }

    /// <summary>
    /// Adds notations select list to the view data dictionary.
    /// </summary>
    /// <param name="onlyGenetic">
    /// If set to <c>true</c> [includes only genetic notations].
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddNotations(bool onlyGenetic = false)
    {
        Notation[] notations = user.IsAdmin() ? EnumExtensions.ToArray<Notation>() : [Notation.Nucleotides];
        if (onlyGenetic)
        {
            notations = notations.Where(n => n.GetNature() == Nature.Genetic).ToArray();
        }

        viewData.Add("notations", notations.ToSelectListWithNature());
        return this;
    }

    /// <summary>
    /// Adds remote databases select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddRemoteDatabases()
    {
        IEnumerable<RemoteDb> remoteDbs = EnumExtensions.ToArray<RemoteDb>();
        if (!user.IsAdmin())
        {
            remoteDbs = remoteDbs.Where(rd => rd.GetNature() == Nature.Genetic);
        }

        viewData.Add("remoteDbs", remoteDbs.ToSelectListWithNature());
        return this;
    }

    /// <summary>
    /// Adds sequence types select list to the view data dictionary.
    /// </summary>
    /// <param name="onlyGenetic">
    /// If set to <c>true</c> [includes only genetic sequence types].
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddSequenceTypes(bool onlyGenetic = false)
    {
        IEnumerable<SequenceType> sequenceTypes = EnumExtensions.ToArray<SequenceType>();
        if (!user.IsAdmin() || onlyGenetic)
        {
            sequenceTypes = sequenceTypes.Where(st => st.GetNature() == Nature.Genetic);
        }

        viewData.Add("sequenceTypes", sequenceTypes.ToSelectListWithNature());
        return this;
    }

    /// <summary>
    /// Adds groups select list to the view data dictionary.
    /// </summary>
    /// <param name="onlyGenetic">
    /// If set to <c>true</c> [includes only genetic groups].
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddGroups(bool onlyGenetic = false)
    {
        IEnumerable<Group> groups = EnumExtensions.ToArray<Group>();
        if (!user.IsAdmin() || onlyGenetic)
        {
            groups = groups.Where(g => g.GetNature() == Nature.Genetic);
        }

        viewData.Add("groups", groups.ToSelectListWithNature());
        return this;
    }

    /// <summary>
    ///  Adds features select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddFeatures()
    {
        IEnumerable<Feature> features = EnumExtensions.ToArray<Feature>().Where(f => f.GetNature() == Nature.Genetic);
        IEnumerable<Feature> selectedFeatures = features.Where(f => f != Feature.NonCodingSequence);
        viewData.Add("features", features.ToSelectListWithNature(selectedFeatures));
        return this;
    }

    /// <summary>
    /// Adds multisequences select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddMultisequences()
    {
        SelectListItemWithNature[] multisequences = db.Multisequences
                                                      .Select(ms => new SelectListItemWithNature
                                                      {
                                                          Value = ms.Id.ToString(),
                                                          Text = ms.Name,
                                                          Nature = (byte)ms.Nature
                                                      })
                                                      .ToArray();

        viewData.Add("multisequences", multisequences);
        return this;
    }

    /// <summary>
    /// Adds languages select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddLanguages()
    {
        viewData.Add("languages", Extensions.EnumExtensions.GetSelectList<Language>());
        return this;
    }

    /// <summary>
    /// Adds translators select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddTranslators()
    {
        viewData.Add("translators", Extensions.EnumExtensions.GetSelectList<Translator>());
        return this;
    }

    /// <summary>
    /// Adds image reading trajectories select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddTrajectories()
    {
        var imageOrderExtractors = EnumExtensions.SelectAllWithAttribute<ImageOrderExtractor>(typeof(ImageOrderExtractorAttribute));
        viewData.Add("trajectories", imageOrderExtractors.ToSelectList());
        return this;
    }

    /// <summary>
    /// Adds pause treatments select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddPauseTreatments()
    {
        viewData.Add("pauseTreatments", Extensions.EnumExtensions.GetSelectList<PauseTreatment>());
        return this;
    }

    /// <summary>
    /// Adds the name of the submit button to the view data dictionary.
    /// </summary>
    /// <param name="submitName">
    /// Name of the submit button.
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddSubmitName(string submitName = "Calculate")
    {
        viewData.Add("submitName", submitName);
        return this;
    }

    /// <summary>
    /// Adds the minimum and maximum number of selected research objects to the view data dictionary.
    /// </summary>
    /// <param name="min">
    /// The minimum research objects selected.
    /// </param>
    /// <param name="max">
    /// The maximum research objects selected.
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddMinMaxResearchObjects(int min = 1, int max = int.MaxValue)
    {
        viewData.Add("minimumSelectedResearchObjects", min);
        viewData.Add("maximumSelectedResearchObjects", max);
        return this;
    }

    /// <summary>
    /// Adds the sequence groups select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public ViewDataHelper AddSequenceGroups()
    {
        return AddSequenceGroups(sg => true);
    }

    /// <summary>
    /// Adds the sequence groups select list to the view data dictionary.
    /// </summary>
    /// <param name="filter">
    /// Filter for sequence groups.
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddSequenceGroups(Func<SequenceGroup, bool> filter)
    {
        ResearchObjectTableRow[] sequenceGoups = db.SequenceGroups
                                                   .Where(filter)
                                                   .OrderBy(m => m.Created)
                                                   .Select(sg => new ResearchObjectTableRow(sg, false))
                                                   .ToArray();

        viewData.Add("sequenceGroups", sequenceGoups);
        return this;
    }

    /// <summary>
    /// Adds the characteristics data to the view data dictionary.
    /// </summary>
    /// <param name="characteristicsCategory">
    /// The characteristics category.
    /// </param>
    /// <returns></returns>
    public ViewDataHelper AddCharacteristicsData(CharacteristicCategory characteristicsCategory)
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

        viewData.Add("characteristicTypes", characteristicTypes);
        viewData.Add("characteristicsDictionary", characteristicsDictionary);

        return this;
    }

    /// <summary>
    /// Returns view data dictionary.
    /// </summary>
    public Dictionary<string, object> Build()
    {
        return new Dictionary<string, object>(viewData);
    }

    /// <summary>
    /// Fills research object creation data.
    /// </summary>
    /// <returns>
    /// The <see cref="Dictionary{string, object}"/>.
    /// </returns>
    public Dictionary<string, object> FillResearchObjectCreationViewData()
    {
        viewData.Clear();

        return this.AddResearchObjects()
                   .AddNatures()
                   .AddNotations()
                   .AddRemoteDatabases()
                   .AddSequenceTypes()
                   .AddGroups()
                   .AddMultisequences()
                   .AddLanguages()
                   .AddTranslators()
                   .AddTrajectories()
                   .Build();
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
        viewData.Clear();

        return this.AddResearchObjects(filter, selectionFilter)
                   .AddMinMaxResearchObjects(minSelectedResearchObjects, maxSelectedResearchObjects)
                   .AddSequenceGroups()
                   .AddNatures()
                   .AddNotations()
                   .AddLanguages()
                   .AddTranslators()
                   .AddPauseTreatments()
                   .AddTrajectories()
                   .AddSequenceTypes()
                   .AddGroups()
                   .AddSubmitName(submitName)
                   .Build();
    }

    /// <summary>
    /// The fill calculation data.
    /// </summary>
    /// <param name="characteristicsCategory">
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
    public Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsCategory, int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName)
    {
        viewData.Clear();

        return this.AddResearchObjects()
                   .AddMinMaxResearchObjects(minSelectedResearchObjects, maxSelectedResearchObjects)
                   .AddSequenceGroups()
                   .AddNatures()
                   .AddNotations()
                   .AddLanguages()
                   .AddTranslators()
                   .AddPauseTreatments()
                   .AddTrajectories()
                   .AddSequenceTypes()
                   .AddGroups()
                   .AddSubmitName(submitName)
                   .AddCharacteristicsData(characteristicsCategory)
                   .Build();
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

        viewData.Clear();

        return this.AddResearchObjects(m => researchObjectIds.Contains(m.Id), m => false)
                   .AddMinMaxResearchObjects(minSelectedResearchObjects, maxSelectedResearchObjects)
                   .AddSequenceGroups()
                   .AddCharacteristicsData(CharacteristicCategory.Full)
                   .AddSubmitName(submitName)
                   .AddNatures(onlyGenetic: true)
                   .AddNotations(onlyGenetic: true)
                   .AddSequenceTypes(onlyGenetic: true)
                   .AddGroups(onlyGenetic: true)
                   .AddFeatures()
                   .Build();
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
        viewData.Clear();

        return this.AddMinMaxResearchObjects(minSelectedResearchObjects, maxSelectedResearchObjects)
                   .AddResearchObjects()
                   .AddSequenceGroups()
                   .AddNatures()
                   .AddSequenceTypes()
                   .AddGroups()
                   .Build();
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
        return this.AddCharacteristicsData(characteristicsCategory)
                   .Build();
    }

    public void Dispose()
    {
        db.Dispose();
    }
}
