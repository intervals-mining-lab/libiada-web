namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Bio;
    using Bio.IO;
    using Bio.IO.GenBank;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;

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
        public GenesImportController()
            : base("GenesImport", "Genes Import")
        {
            db = new LibiadaWebEntities();
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
            var matterIds = db.DnaSequence.Where(c => c.WebApiId != null && !genesSequenceIds.Contains(c.Id)).Select(c => c.MatterId).ToList();

            var calculatorsHelper = new ViewDataHelper(db);

            var data = calculatorsHelper.FillMattersData(1, 1, false, m => matterIds.Contains(m.Id));

            data.Add("natureId", Aliases.Nature.Genetic);

            ViewBag.data = data;

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

                List<FeatureItem> features = metadata.Features.All;

                var starts = new List<int>();
                var ends = new List<int>() { 0 };

                for (int i = 1; i < features.Count; i++)
                {
                    var feature = features[i];
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

                    var location = feature.Location;
                    var leafLocations = feature.Location.GetLeafLocations();

                    if (leafLocations.Count == 0)
                    {
                        throw new Exception("No leaf locations");
                    }

                    bool complement = location.Operator == LocationOperator.Complement;
                    bool join = leafLocations.Count > 1;
                    bool complementJoin = join && complement;

                    complement = complement || location.SubLocations[0].Operator == LocationOperator.Complement;

                    int start = leafLocations[0].LocationStart - 1;
                    int end = leafLocations[0].LocationEnd - 1;
                    int length = end - start + 1;

                    if (length < 1)
                    {
                        throw new Exception("Length of subsequence cant be less than 1.");
                    }

                    if (ends[i] < ends[i - 1])
                    {
                        throw new Exception("Wrong subsequences order.");
                    }

                    var subsequence = new Subsequence
                    {
                        Id = DbHelper.GetNewElementId(db),
                        FeatureId = featureId,
                        Partial = false,
                        Complementary = complement,
                        SequenceId = sequenceId,
                        Start = start,
                        Length = end - start + 1
                    };

                    starts.Add(start - 1);
                    ends.Add(end + 1);

                    for (int k = 1; k > leafLocations.Count; k++)
                    {
                        var leafLocation = leafLocations[k];

                        start = leafLocation.LocationStart - 1;
                        end = leafLocation.LocationEnd - 1;

                        var position = new Position
                        {
                            Subsequence = subsequence,
                            Start = start,
                            Length = end - start + 1
                        };

                        db.Position.Add(position);

                        starts.Add(start - 1);
                        ends.Add(end + 1);
                    }

                    foreach (var qualifier in feature.Qualifiers)
                    {
                        sequenceAttributeRepository.CreateSequenceAttribute(qualifier, subsequence);
                    }

                    if (complement)
                    {
                        sequenceAttributeRepository.CreateSequenceAttribute("complement", subsequence);

                        if (complementJoin)
                        {
                            sequenceAttributeRepository.CreateSequenceAttribute("complementJoin", subsequence);
                        }
                    }

                    db.Subsequence.Add(subsequence);
                }

                starts.Add(features[0].Location.LocationEnd - 1);

                for (int i = 0; i < ends.Count; i++)
                {
                    int start = ends[i];
                    int length = starts[i] - ends[i] + 1;

                    if (length > 0)
                    {
                        var subsequence = new Subsequence
                        {
                            Id = DbHelper.GetNewElementId(db),
                            FeatureId = Aliases.Feature.NonCodingSequence,
                            Partial = false,
                            Complementary = false,
                            SequenceId = sequenceId,
                            Start = start,
                            Length = length
                        };

                        db.Subsequence.Add(subsequence);
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
