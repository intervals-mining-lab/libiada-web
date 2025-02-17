namespace Libiada.Web.Helpers;

using System.ComponentModel;
using System.Security.Claims;

using Libiada.Core.Music;

using Libiada.Web.Extensions;
using Libiada.Web.Models.CalculatorsData;

using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Attributes;

using EnumExtensions = Core.Extensions.EnumExtensions;

/// <summary>
/// Builds data dictionary for views.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ViewDataBuilder"/> class.
/// </remarks>
/// <param name="dbFactory">
/// Database context factory.
/// </param>
public class ViewDataBuilder(IDbContextFactory<LibiadaDatabaseEntities> dbFactory,
                      IResearchObjectsCache cache,
                      ClaimsPrincipal user,
                      IFullCharacteristicRepository fullCharacteristicRepository,
                      ICongenericCharacteristicRepository congenericCharacteristicRepository,
                      IAccordanceCharacteristicRepository accordanceCharacteristicRepository,
                      IBinaryCharacteristicRepository binaryCharacteristicRepository) : IViewDataBuilder
{
    /// <summary>
    /// The database model.
    /// </summary>
    private readonly LibiadaDatabaseEntities db = dbFactory.CreateDbContext();
    private readonly IResearchObjectsCache cache = cache;

    /// <summary>
    /// The current user.
    /// </summary>
    private readonly ClaimsPrincipal user = user;
    private readonly IFullCharacteristicRepository fullCharacteristicModelRepository = fullCharacteristicRepository;
    private readonly ICongenericCharacteristicRepository congenericCharacteristicModelRepository = congenericCharacteristicRepository;
    private readonly IAccordanceCharacteristicRepository accordanceCharacteristicModelRepository = accordanceCharacteristicRepository;
    private readonly IBinaryCharacteristicRepository binaryCharacteristicModelRepository = binaryCharacteristicRepository;

    private readonly Dictionary<string, object> viewData = [];

    /// <summary>
    /// Adds the list of research objects table rows to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddResearchObjects()
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
    public IViewDataBuilder AddResearchObjects(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selection)
    {
        IOrderedEnumerable<ResearchObject> researchObjects = cache.ResearchObjects
                                                                  .Where(filter)
                                                                  .OrderBy(m => m.Created);
        IEnumerable<ResearchObjectTableRow> researchObjectsList = researchObjects.Select(m => new ResearchObjectTableRow(m, selection(m)));

        viewData.Add("researchObjects", researchObjectsList);
        return this;
    }

    /// <summary>
    /// Adds the list of only research objects with subsequences 
    /// in form of table rows to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddResearchObjectsWithSubsequences()
    {
        var sequenceIds = db.Subsequences
                            .Select(s => s.SequenceId)
                            .Distinct();
        var researchObjectIds = db.CombinedSequenceEntities
                                  .Where(c => sequenceIds.Contains(c.Id))
                                  .Select(c => c.ResearchObjectId)
                                  .ToList();

        return AddResearchObjects(m => researchObjectIds.Contains(m.Id), m => false);
    }

    /// <summary>
    /// Adds the sequence groups select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddSequenceGroups()
    {
        ResearchObjectTableRow[] sequenceGoups = db.SequenceGroups
                                                   .OrderBy(m => m.Created)
                                                   .Select(sg => new ResearchObjectTableRow(sg, false))
                                                   .ToArray();

        viewData.Add("sequenceGroups", sequenceGoups);

        return this;
    }

    /// <summary>
    /// Adds natures select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddNatures()
    {
        Nature[] natures = user.IsAdmin() ? EnumExtensions.ToArray<Nature>() : [Nature.Genetic];
        viewData.Add("natures", natures.ToSelectList());
        return this;
    }

    /// <summary>
    /// Adds nature as currently selected in the view data dictionary.
    /// </summary>
    /// <param name="nature">Nature value as <see cref="Nature"/></param>
    /// <returns></returns>
    public IViewDataBuilder SetNature(Nature nature)
    {
        //TODO: check if conversion to string is necessary
        viewData.Add("nature", ((byte)nature).ToString());
        return this;
    }

    /// <summary>
    /// Adds notations select list to the view data dictionary.
    /// </summary>
    /// <param name="onlyGenetic">
    /// If set to <c>true</c> includes only genetic notations.
    /// </param>
    /// <returns></returns>
    public IViewDataBuilder AddNotations(bool onlyGenetic = false)
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
    public IViewDataBuilder AddRemoteDatabases()
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
    /// If set to <c>true</c> includes only genetic sequence types.
    /// </param>
    /// <returns></returns>
    public IViewDataBuilder AddSequenceTypes(bool onlyGenetic = false)
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
    /// If set to <c>true</c> includes only genetic groups.
    /// </param>
    /// <returns></returns>
    public IViewDataBuilder AddGroups(bool onlyGenetic = false)
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
    /// Adds sequences groups types select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddSequenceGroupTypes()
    {
        SequenceGroupType[] sequenceGroupTypes = EnumExtensions.ToArray<SequenceGroupType>();
        viewData.Add("sequenceGroupTypes", sequenceGroupTypes.ToSelectListWithNature());
        return this;
    }

    /// <summary>
    ///  Adds features select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddFeatures()
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
    public IViewDataBuilder AddMultisequences()
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
    public IViewDataBuilder AddLanguages()
    {
        viewData.Add("languages", Extensions.EnumExtensions.GetSelectList<Language>());
        return this;
    }

    /// <summary>
    /// Adds translators select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddTranslators()
    {
        viewData.Add("translators", Extensions.EnumExtensions.GetSelectList<Translator>());
        return this;
    }

    /// <summary>
    /// Adds image reading trajectories select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddTrajectories()
    {
        var imageOrderExtractors = EnumExtensions.SelectAllWithAttribute<ImageOrderExtractor>(typeof(ImageOrderExtractorAttribute));
        viewData.Add("trajectories", imageOrderExtractors.ToSelectList());
        return this;
    }

    /// <summary>
    /// Adds image transformers select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddImageTransformers()
    {
        viewData.Add("imageTransformers", Extensions.EnumExtensions.GetSelectList<ImageTransformer>());
        return this;
    }

    /// <summary>
    /// Adds pause treatments select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddPauseTreatments()
    {
        viewData.Add("pauseTreatments", Extensions.EnumExtensions.GetSelectList<PauseTreatment>());
        return this;
    }

    /// <summary>
    /// Adds order transformations select list to the view data dictionary.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddOrderTransformations()
    {
        viewData.Add("pauseTreatments", Extensions.EnumExtensions.GetSelectList<OrderTransformation>());
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
    public IViewDataBuilder AddMinMaxResearchObjects(int min = 1, int max = int.MaxValue)
    {
        viewData.Add("minimumSelectedResearchObjects", min);
        viewData.Add("maximumSelectedResearchObjects", max);
        return this;
    }

    /// <summary>
    /// Adds to the view data dictionary flag 
    /// indicating that characteristics should have a field 
    /// with maximum difference between characteristic values (in percent)
    /// for it to be considered similar.
    /// </summary>
    /// <returns></returns>
    public IViewDataBuilder AddMaxPercentageDifferenceRequiredFlag()
    {
        viewData.Add("percentageDifferenseNeeded", true);
        return this;
    }
    /// <summary>
    /// Adds the characteristics data to the view data dictionary.
    /// </summary>
    /// <param name="characteristicsCategory">
    /// The characteristics category.
    /// </param>
    /// <returns></returns>
    public IViewDataBuilder AddCharacteristicsData(CharacteristicCategory characteristicsCategory)
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

    public void Dispose()
    {
        db.Dispose();
    }
}
