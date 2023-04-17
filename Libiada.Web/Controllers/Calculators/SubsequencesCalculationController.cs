namespace Libiada.Web.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    using LibiadaCore.Extensions;

    using Libiada.Web.Helpers;
    using Libiada.Database.Models.Repositories.Catalogs;
    using Libiada.Database.Tasks;

    using Newtonsoft.Json;

    using Microsoft.AspNetCore.Authorization;
    using Libiada.Database;
    using Libiada.Database.Models.CalculatorsData;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Libiada.Web.Tasks;
    using Libiada.Database.Models.Calculators;
    using Libiada.Database.Models.Repositories.Sequences;

    /// <summary>
    /// The subsequences calculation controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SubsequencesCalculationController : AbstractResultController
    {
        private readonly LibiadaDatabaseEntities db;
        private readonly IViewDataHelper viewDataHelper;
        private readonly IFullCharacteristicRepository characteristicTypeLinkRepository;
        private readonly ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator;
        private readonly ICommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesCalculationController"/> class.
        /// </summary>
        public SubsequencesCalculationController(LibiadaDatabaseEntities db,
                                                 IViewDataHelper viewDataHelper,
                                                 ITaskManager taskManager,
                                                 IFullCharacteristicRepository characteristicTypeLinkRepository,
                                                 ISubsequencesCharacteristicsCalculator subsequencesCharacteristicsCalculator,
                                                 ICommonSequenceRepository commonSequenceRepository)
            : base(TaskType.SubsequencesCalculation, taskManager)
        {
            this.db = db;
            this.viewDataHelper = viewDataHelper;
            this.characteristicTypeLinkRepository = characteristicTypeLinkRepository;
            this.subsequencesCharacteristicsCalculator = subsequencesCharacteristicsCalculator;
            this.commonSequenceRepository = commonSequenceRepository;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(viewDataHelper.FillSubsequencesViewData(1, int.MaxValue, "Calculate"));
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="features">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long[] matterIds, short[] characteristicLinkIds, Feature[] features)
        {
            return CreateTask(() =>
            {
                var sequencesData = new SequenceData[matterIds.Length];

                long[] parentSequenceIds;
                var matterNames = new string[matterIds.Length];
                var remoteIds = new string[matterIds.Length];
                var subsequencesCharacteristicsNames = new string[characteristicLinkIds.Length];
                var subsequencesCharacteristicsList = new SelectListItem[characteristicLinkIds.Length];

                var parentSequences = db.DnaSequence.Include(s => s.Matter)
                                        .Where(s => s.Notation == Notation.Nucleotides && matterIds.Contains(s.MatterId))
                                        .Select(s => new { s.Id, MatterName = s.Matter.Name, s.RemoteId })
                                        .ToDictionary(s => s.Id);
                parentSequenceIds = parentSequences.Keys.ToArray();

                for (int n = 0; n < parentSequenceIds.Length; n++)
                {
                    matterNames[n] = parentSequences[parentSequenceIds[n]].MatterName;
                    remoteIds[n] = parentSequences[parentSequenceIds[n]].RemoteId;
                }

                for (int k = 0; k < characteristicLinkIds.Length; k++)
                {
                    subsequencesCharacteristicsNames[k] = characteristicTypeLinkRepository.GetCharacteristicName(characteristicLinkIds[k]);
                    subsequencesCharacteristicsList[k] = new SelectListItem
                    {
                        Value = k.ToString(),
                        Text = subsequencesCharacteristicsNames[k],
                        Selected = false
                    };
                }

                // TODO: Maybe AttributesValueCache should be created in the Subsequences calculator
                var attributeValuesCache = new AttributeValueCacheManager(db);
                for (int i = 0; i < parentSequenceIds.Length; i++)
                {
                    var subsequencesData = subsequencesCharacteristicsCalculator.CalculateSubsequencesCharacteristics(characteristicLinkIds, features, parentSequenceIds[i]);

                    attributeValuesCache.FillAttributeValues(subsequencesData);

                    sequencesData[i] = new SequenceData(matterIds[i], matterNames[i], remoteIds[i], default, subsequencesData);
                }

                List<AttributeValue> allAttributeValues = attributeValuesCache.AllAttributeValues;

                var result = new Dictionary<string, object>
                {
                    { "sequencesData", sequencesData },
                    { "features", features.ToDictionary(f => (byte)f, f => f.GetDisplayValue()) },
                    { "attributes", EnumExtensions.ToArray<Attribute>().ToDictionary(a => (byte)a, a => a.GetDisplayValue()) },
                    { "attributeValues", allAttributeValues.Select(sa => new { attribute = sa.AttributeId, value = sa.Value }) },
                    { "subsequencesCharacteristicsNames", subsequencesCharacteristicsNames },
                    { "subsequencesCharacteristicsList", subsequencesCharacteristicsList }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}
