namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using Bio;
    using Bio.IO;
    using Bio.IO.GenBank;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The genes import controller.
    /// </summary>
    public class GenesImportController : AbstractResultController
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
        /// The feature repository.
        /// </summary>
        private readonly FeatureRepository featureRepository;

        /// <summary>
        /// The attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenesImportController"/> class.
        /// </summary>
        public GenesImportController() : base("GenesImport", "Genes Import")
        {
            db = new LibiadaWebEntities();
            matterRepository = new MatterRepository(db);
            featureRepository = new FeatureRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var genesSequenceIds = db.Subsequence.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => c.WebApiId != null && !genesSequenceIds.Contains(c.Id)).Select(c => c.MatterId);

            var matters = db.Matter.Where(m => matterIds.Contains(m.Id));

            ViewBag.data = new Dictionary<string, object>
                {
                    { "minimumSelectedMatters", 1 },
                    { "maximumSelectedMatters", 1 },
                    { "natureId", Aliases.Nature.Genetic }, 
                    { "matters", matterRepository.GetMatterSelectList(matters) }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterId">
        /// The matter id.
        /// </param>
        /// <param name="localFile">
        /// The local file.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if there is no file with sequence.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if unknown part is found.
        /// </exception>
        [HttpPost]
        public ActionResult Index(long matterId, bool localFile)
        {
            return Action(() =>
            {
                long sequenceId = db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                DnaSequence parentSequence = db.DnaSequence.Single(c => c.Id == sequenceId);

                Stream stream;
                if (localFile)
                {
                    HttpPostedFileBase file = Request.Files[0];

                    if (file == null || file.ContentLength == 0)
                    {
                        throw new ArgumentNullException("file", "Sequence file is empty");
                    }

                    stream = file.InputStream;
                }
                else
                {
                    stream = NcbiHelper.GetGenesFileStream(parentSequence.WebApiId.ToString());
                }

                var gbFile = stream;

                ISequenceParser parser = new GenBankParser();
                ISequence sequence = parser.ParseOne(gbFile);

                GenBankMetadata metadata = sequence.Metadata["GenBank"] as GenBankMetadata;

                if (metadata == null)
                {
                    throw new Exception("GenBank file metadata is empty.");
                }

                 var starts = new List<int>();
                 var stops = new List<int>();

                List<FeatureItem> features = metadata.Features.All;

                for (int i = 1; i < features.Count; i++)
                {
                    var feature = features[i];
                    var location = feature.Location;

                    int featureId;

                    if (feature.Key == "gene")
                    {
                        bool pseudo = feature.Qualifiers.Any(qualifier => qualifier.Key == "pseudo");
                        if (!pseudo)
                        {
                            continue;
                        }

                        featureId = Aliases.Feature.PseudoGen;
                    }
                    else
                    {
                        featureId = featureRepository.GetFeatureIdByName(feature.Key);
                    }

                    var subsequence = new Subsequence
                    {
                        Id = DbHelper.GetNewElementId(db),
                        FeatureId = featureId,
                        Partial = false,
                        Complementary = location.Operator == LocationOperator.Complement,
                        SequenceId = sequenceId,
                        Start = location.LocationStart - 1,
                        Length = location.LocationEnd - location.LocationStart
                    };

                    foreach (var qualifier in feature.Qualifiers)
                    {
                        sequenceAttributeRepository.CreateSequenceAttribute(qualifier, subsequence);
                    }
                }

                db.SaveChanges();

                var matterName = db.Matter.Single(m => m.Id == matterId).Name;

                var sequenceSubsequences = db.Subsequence.Where(g => g.SequenceId == sequenceId).Include(g => g.Position).Include(g => g.Feature).Include(g => g.SequenceAttribute);

                return new Dictionary<string, object>
                                     {
                                         { "matterName", matterName }, 
                                         { "genes", sequenceSubsequences }
                                     };
            });
        }
    }
}
