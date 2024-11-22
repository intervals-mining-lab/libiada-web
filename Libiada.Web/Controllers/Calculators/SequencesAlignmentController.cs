namespace Libiada.Web.Controllers.Calculators;

using Bio.Extensions;

using Libiada.Core.Extensions;

using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Tasks;

using Newtonsoft.Json;

using Libiada.Web.Tasks;
using Libiada.Web.Helpers;

/// <summary>
/// The alignment controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class SequencesAlignmentController : AbstractResultController
{
    /// <summary>
    /// The characteristic type repository.
    /// </summary>
    private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
    private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
    private readonly Cache cache;
    private readonly IDbContextFactory<LibiadaDatabaseEntities> dbFactory;
    private readonly IViewDataHelper viewDataHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequencesAlignmentController"/> class.
    /// </summary>
    public SequencesAlignmentController(IDbContextFactory<LibiadaDatabaseEntities> dbFactory, 
                                        IViewDataHelper viewDataHelper, 
                                        ITaskManager taskManager,
                                        IFullCharacteristicRepository characteristicTypeLinkRepository,
                                        ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                        Cache cache)
        : base(TaskType.SequencesAlignment, taskManager)
    {
        this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
        this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
        this.cache = cache;
        this.dbFactory = dbFactory;
        this.viewDataHelper = viewDataHelper;
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index()
    {
        ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(2, 2, "Align"));
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="matterIds">
    /// The matter ids.
    /// </param>
    /// <param name="characteristicLinkId">
    /// The characteristic id.
    /// </param>
    /// <param name="notation">
    /// The notation id.
    /// </param>
    /// <param name="features">
    /// The feature ids.
    /// </param>
    /// <param name="validationType">
    /// The validation type.
    /// </param>
    /// <param name="cyclicShift">
    /// The cyclic shift.
    /// </param>
    /// <param name="sort">
    /// The sort.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if validationType is unknown.
    /// Or if count of matters is not 2.
    /// </exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(
        long[] matterIds,
        short characteristicLinkId,
        Notation notation,
        Feature[] features,
        string validationType,
        bool cyclicShift,
        bool sort)
    {
        return CreateTask(() =>
        {
            if (matterIds.Length != 2)
            {
                throw new ArgumentException("Count of selected matters must be 2.", nameof(matterIds));
            }

            string firstMatterName;
            string secondMatterName;
            long firstParentId;
            long secondParentId;

            using var db = dbFactory.CreateDbContext();

            long firstMatterId = matterIds[0];
            firstMatterName = cache.Matters.Single(m => m.Id == firstMatterId).Name;
            firstParentId = db.CommonSequences.Single(c => c.MatterId == firstMatterId && c.Notation == notation).Id;

            long secondMatterId = matterIds[1];
            secondMatterName = cache.Matters.Single(m => m.Id == secondMatterId).Name;
            secondParentId = db.CommonSequences.Single(c => c.MatterId == secondMatterId && c.Notation == notation).Id;

            double[] firstSequenceCharacteristics = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(firstParentId, characteristicLinkId, features);
            double[] secondSequenceCharacteristics = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(secondParentId, characteristicLinkId, features);

            if (sort)
            {
                Array.Sort(firstSequenceCharacteristics, (x, y) => y.CompareTo(x));
                Array.Sort(secondSequenceCharacteristics, (x, y) => y.CompareTo(x));
            }

            List<double> longer;
            List<double> shorter;
            if (firstSequenceCharacteristics.Length >= secondSequenceCharacteristics.Length)
            {
                longer = firstSequenceCharacteristics.ToList();
                shorter = secondSequenceCharacteristics.ToList();
            }
            else
            {
                longer = secondSequenceCharacteristics.ToList();
                shorter = firstSequenceCharacteristics.ToList();
            }

            if (!cyclicShift)
            {
                int count = longer.Count;
                for (int i = 0; i < count; i++)
                {
                    longer.Add(0);
                }
            }

            var distanceCalculator = GetDistanceCalculator(validationType);
            List<double> distances = [];
            int optimalRotation = CalculateMeasureForRotation(longer, shorter, distances, distanceCalculator);

            string characteristicName = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkId, notation);

            var result = new Dictionary<string, object>
            {
                { "firstSequenceName", firstMatterName },
                { "secondSequenceName", secondMatterName },
                { "characteristicName", characteristicName },
                { "features", features.ConvertAll(p => p.GetDisplayValue()) },
                { "optimalRotation", optimalRotation },
                { "distances", distances.Select(el => new {Value = el}) },
                { "validationType", validationType },
                { "cyclicShift", cyclicShift },
                { "sort", sort }
            };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }

    /// <summary>
    /// The get distance calculator.
    /// </summary>
    /// <param name="validationType">
    /// The validation type.
    /// </param>
    /// <returns>
    /// The <see cref="Func{Double, Double, Double}"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if validation type is unknown.
    /// </exception>
    [NonAction]
    private Func<double, double, double> GetDistanceCalculator(string validationType)
    {
        return validationType switch
        {
            "Similarity" => (first, second) => System.Math.Abs(System.Math.Min(first, second)),
            "Difference" => (first, second) => -System.Math.Abs(first - second),
            "NormalizedDifference" => (first, second) => -System.Math.Abs((first - second) / (first + second)),
            "Equality" => (first, second) => System.Math.Abs(first - second) < (System.Math.Abs(first + second) / 20) ? 1 : 0,
            _ => throw new ArgumentException("unknown validation type", nameof(validationType)),
        };
    }

    /// <summary>
    /// Calculates measure for given rotation.
    /// </summary>
    /// <param name="first">
    /// The first.
    /// </param>
    /// <param name="second">
    /// The second.
    /// </param>
    /// <param name="distances">
    /// The distances.
    /// </param>
    /// <param name="measure">
    /// The measure.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    [NonAction]
    private int CalculateMeasureForRotation(List<double> first, List<double> second, List<double> distances, Func<double, double, double> measure)
    {
        int optimalRotation = 0;
        double maximumEquality = 0;
        for (int i = 0; i < first.Count; i++)
        {
            double distance = Measure(first, second, measure);
            if (maximumEquality < distance)
            {
                optimalRotation = i;
                maximumEquality = distance;
            }

            distances.Add(distance);
            first = Rotate(first);
        }

        return optimalRotation;
    }

    /// <summary>
    /// The rotate.
    /// </summary>
    /// <param name="list">
    /// The list.
    /// </param>
    /// <returns>
    /// The <see cref="List{Double}"/>.
    /// </returns>

    [NonAction]
    private List<double> Rotate(List<double> list)
    {
        double first = list[0];
        list.RemoveAt(0);
        list.Add(first);
        return list;
    }

    /// <summary>
    /// The measure.
    /// </summary>
    /// <param name="first">
    /// The first.
    /// </param>
    /// <param name="second">
    /// The second.
    /// </param>
    /// <param name="measure">
    /// The measurer.
    /// </param>
    /// <returns>
    /// The <see cref="double"/>.
    /// </returns>
    [NonAction]
    private double Measure(List<double> first, List<double> second, Func<double, double, double> measure)
    {
        double result = 0;
        for (int i = 0; i < second.Count; i++)
        {
            if ((first[i] * second[i]) > 0)
            {
                result += measure(first[i], second[i]);
            }
        }

        return result;
    }
}
