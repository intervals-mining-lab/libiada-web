namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Misc.Iterators;

    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The building comparison controller.
    /// </summary>
    public class BuildingComparisonController : AbstractResultController
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
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingComparisonController"/> class.
        /// </summary>
        public BuildingComparisonController()
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            chainRepository = new ChainRepository(db);

            ControllerName = "BuildingComparison";
            DisplayName = "Building comparison";
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="firstMatterId">
        /// The first matter id.
        /// </param>
        /// <param name="secondMatterId">
        /// The second matter id.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="congeneric">
        /// The congeneric.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(long firstMatterId, long secondMatterId, int length, bool congeneric)
        {
            return Action(() =>
            {
                string chainName1 = db.matter.Single(m => m.id == firstMatterId).name;
                string chainName2 = db.matter.Single(m => m.id == secondMatterId).name;
                matter matter1 = db.matter.Single(m => m.id == firstMatterId);
                long chainId1 = matter1.chain.Single(c => c.notation_id == Aliases.NotationNucleotide).id;
                Chain libiadaChain1 = chainRepository.ToLibiadaChain(chainId1);
                matter matter2 = db.matter.Single(m => m.id == secondMatterId);
                long chainId2 = matter2.chain.Single(c => c.notation_id == Aliases.NotationNucleotide).id;
                Chain libiadaChain2 = chainRepository.ToLibiadaChain(chainId2);

                BaseChain res1 = null;
                BaseChain res2 = null;

                int i = 0;
                int j = 0;
                var iter1 = new IteratorStart(libiadaChain1, length, 1);
                bool duplicate = false;
                while (!duplicate && iter1.Next())
                {
                    i++;
                    var tempChain1 = (BaseChain) iter1.Current();
                    var iter2 = new IteratorStart(libiadaChain2, length, 1);
                    j = 0;
                    while (!duplicate && iter2.Next())
                    {
                        j++;
                        var tempChain2 = (BaseChain) iter2.Current();

                        if (congeneric)
                        {
                            for (int a = 0; a < tempChain1.Alphabet.Cardinality; a++)
                            {
                                /*  CongenericChain firstChain = tempChain1.CongenericChain(a);
                              for (int b = 0; b < tempChain2.Alphabet.Cardinality; b++)
                              {

                                  CongenericChain secondChain = tempChain2.CongenericChain(b);
                                  if (!firstChain.Equals(secondChain) && this.CompareBuldings(firstChain.Building, secondChain.Building))
                                  {
                                      res1 = firstChain;
                                      res2 = secondChain;
                                      duplicate = true;
                                  }
                              }*/
                            }
                        }
                        else
                        {
                            if (!tempChain1.Equals(tempChain2) &&
                                this.CompareBuildings(tempChain2.Building, tempChain1.Building))
                            {
                                res1 = tempChain1;
                                res2 = tempChain2;
                                duplicate = true;
                            }
                        }
                    }
                }


                return new Dictionary<string, object>
                {
                    {"duplicate", duplicate},
                    {"chainName1", chainName1},
                    {"chainName2", chainName2},
                    {"res1", res1},
                    {"res2", res2},
                    {"pos1", i},
                    {"pos2", j}
                };
            });
        }

        /// <summary>
        /// The compare buildings.
        /// </summary>
        /// <param name="firstBuilding">
        /// The first building.
        /// </param>
        /// <param name="secondbuilding">
        /// The secondbuilding.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CompareBuildings(int[] firstBuilding, int[] secondbuilding)
        {
            if (firstBuilding.Length != secondbuilding.Length)
            {
                return false;
            }

            for (int i = 0; i < firstBuilding.Length; i++)
            {
                if (firstBuilding[i] != secondbuilding[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}