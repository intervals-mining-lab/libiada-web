namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Clusterizator.KMeans;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics.Calculators.FullCalculators;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Models.Repositories.Catalogs;

    using Newtonsoft.Json;
    using LibiadaCore.Extensions;
    using Clusterizator;

    /// <summary>
    /// The clusterization controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ClusterizationController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterizationController"/> class.
        /// </summary>
        public ClusterizationController() : base("Clusterization")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var viewDataHelper = new ViewDataHelper(db);
            var viewData = viewDataHelper.FillViewData(c => c.FullSequenceApplicable, 3, int.MaxValue, "Calculate");
            viewData.Add("ClusterizatorsTypes", ArrayExtensions.ToArray<ClusterizationType>().ToSelectList());
            ViewBag.data = JsonConvert.SerializeObject(viewData);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicTypeLinkIds">
        /// The characteristic type and link ids.
        /// </param>
        /// <param name="notations">
        /// The notation ids.
        /// </param>
        /// <param name="languages">
        /// The language ids.
        /// </param>
        /// <param name="clustersCount">
        /// The clusters count.
        /// </param>
        /// <param name="clusterizationType">
        /// Clusterization method param
        /// </param>
        /// <param name="equipotencyWeight">
        /// The power weight.
        /// </param>
        /// <param name="normalizedDistanceWeight">
        /// The normalized distance weight.
        /// </param>
        /// <param name="distanceWeight">
        /// The distance weight.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            long[] matterIds,
            int[] characteristicTypeLinkIds,
            Notation[] notations,
            Language[] languages,
            int clustersCount,
            ClusterizationType clusterizationType,
            double equipotencyWeight = 1,
            double normalizedDistanceWeight = 1,
            double distanceWeight = 1)
        {
            return Action(() =>
            {
                var characteristicNames = new List<string>();
                var mattersCharacteristics = new object[matterIds.Length];
                var characteristics = new double[matterIds.Length][];
                matterIds = matterIds.OrderBy(m => m).ToArray();
                var matters = db.Matter.Where(m => matterIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name);

                for (int j = 0; j < matterIds.Length; j++)
                {
                    var matterId = matterIds[j];
                    characteristics[j] = new double[characteristicTypeLinkIds.Length];
                    for (int i = 0; i < characteristicTypeLinkIds.Length; i++)
                    {
                        var notation = notations[i];
                        long sequenceId = db.Matter.Single(m => m.Id == matterId).Sequence.Single(c => c.Notation == notation).Id;

                        int characteristicTypeLinkId = characteristicTypeLinkIds[i];
                        if (db.CharacteristicValue.Any(c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId))
                        {
                            characteristics[j][i] = db.CharacteristicValue.Single(c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == characteristicTypeLinkId).Value;
                        }
                        else
                        {
                            Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);

                            var link = characteristicTypeLinkRepository.GetLibiadaLink(characteristicTypeLinkId);
                            string className = characteristicTypeLinkRepository.GetCharacteristicType(characteristicTypeLinkId).ClassName;
                            IFullCalculator calculator = FullCalculatorsFactory.CreateFullCalculator(className);
                            characteristics[j][i] = calculator.Calculate(tempChain, link);
                        }
                    }
                }

                for (int k = 0; k < characteristicTypeLinkIds.Length; k++)
                {
                    characteristicNames.Add(characteristicTypeLinkRepository.GetCharacteristicName(characteristicTypeLinkIds[k], notations[k]));
                }

                var clusterizationParams = new Dictionary<string, double>
                {
                    { "equipotencyWeight", equipotencyWeight },
                    { "normalizedDistanceWeight", normalizedDistanceWeight },
                    { "distanceWeight", distanceWeight }
                };


                var clusterizator = ClusterizatorsFactory.CreateClusterizator(clusterizationType, clusterizationParams);
                int[] clusterizationResult = clusterizator.Cluster(clustersCount, characteristics);
                for (int i = 0; i < clusterizationResult.Length; i++)
                {
                    mattersCharacteristics[i] = new
                    {
                        MatterName = matters[matterIds[i]],
                        cluster = clusterizationResult[i] + 1,
                        Characteristics = characteristics[i]
                    };
                }

                var characteristicsList = new List<SelectListItem>();
                for (int i = 0; i < characteristicNames.Count; i++)
                {
                    characteristicsList.Add(new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = characteristicNames[i],
                        Selected = false
                    });
                }

                var result = new Dictionary<string, object>
                {
                    { "characteristicNames", characteristicNames },
                    { "characteristics", mattersCharacteristics },
                    { "characteristicsList", characteristicsList },
                    { "clustersCount", clustersCount }
                };

                return new Dictionary<string, object>
                           {
                               { "data", JsonConvert.SerializeObject(result) }
                           };
            });
        }
    }
}
