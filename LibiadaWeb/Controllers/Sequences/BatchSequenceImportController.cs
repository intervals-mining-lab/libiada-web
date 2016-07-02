namespace LibiadaWeb.Controllers.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;

    using Bio;
    using Bio.IO;
    using Bio.IO.GenBank;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The batch sequence import controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class BatchSequenceImportController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        protected readonly LibiadaWebEntities Db;

        /// <summary>
        /// The thread disposable.
        /// </summary>
        protected bool ThreadDisposable = true;

        /// <summary>
        /// The DNA sequence repository.
        /// </summary>
        private readonly DnaSequenceRepository dnaSequenceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchSequenceImportController"/> class.
        /// </summary>
        public BatchSequenceImportController() : base("Batch sequences import")
        {
            Db = new LibiadaWebEntities();
            dnaSequenceRepository = new DnaSequenceRepository(Db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.data = JsonConvert.SerializeObject(string.Empty);
            return View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="accessions">
        /// The accessions.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string[] accessions)
        {
            ThreadDisposable = false;

            return Action(() =>
            {
                var existingAccessions = Db.DnaSequence.Select(d => d.RemoteId).Distinct().ToArray();
                var matterNames = new string[accessions.Length];
                var results = new string[accessions.Length];
                var statuses = new string[accessions.Length];

                for (int i = 0; i < accessions.Length; i++)
                {
                    try
                    {
                        string accession = accessions[i];
                        matterNames[i] = accession;
                        if (existingAccessions.Contains(accession) || existingAccessions.Contains(accession + ".1"))
                        {
                            results[i] = "Sequence already exists";
                            statuses[i] = "Exist";
                        }
                        else
                        {
                            var genBankFile = NcbiHelper.GetGenBankFileStream(accession);
                            ISequenceParser parser = new GenBankParser();
                            ISequence bioSequence = parser.ParseOne(genBankFile);
                            GenBankMetadata metadata = bioSequence.Metadata["GenBank"] as GenBankMetadata;
                            var sequenceStream = NcbiHelper.GetFileStream(accession);

                            if (metadata == null)
                            {
                                throw new Exception("sequence metadata is missing.");
                            }

                            if (existingAccessions.Contains(metadata.Version.CompoundAccession))
                            {
                                results[i] = "Sequence already exists";
                                statuses[i] = "Exist";
                            }
                            else
                            {
                                var matterName = NcbiHelper.ExtractSequenceName(metadata) + " | " + metadata.Version.CompoundAccession;
                                matterNames[i] = "Common name=" + metadata.Source.CommonName +
                                                 ", Species=" + metadata.Source.Organism.Species +
                                                 ", Definition=" + metadata.Definition +
                                                 ", Matter name=" + matterName;

                                var matter = new Matter
                                                 {
                                                     Name = matterName,
                                                     Nature = Nature.Genetic
                                                 };
                                
                                var sequence = new CommonSequence
                                                   {
                                                       Matter = matter,
                                                       FeatureId = Aliases.Feature.FullGenome,
                                                       NotationId = Aliases.Notation.Nucleotide,
                                                       RemoteDbId = Aliases.RemoteDb.RemoteDbNcbi,
                                                       RemoteId = metadata.Version.CompoundAccession
                                                   };
                                dnaSequenceRepository.Create(sequence, sequenceStream, metadata.Definition.ToLower().Contains("partial"));
                                results[i] = "successfully imported sequence";
                                statuses[i] = "Success";
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        results[i] = "Error:" + exception.Message + (exception.InnerException == null ? string.Empty : exception.InnerException.Message);
                        statuses[i] = "Error";
                    }
                }

                var orphanMatter = Db.Matter.Include(m => m.Sequence).Where(m => matterNames.Contains(m.Name) && m.Sequence.Count == 0).ToArray();

                if (orphanMatter.Length > 0)
                {
                    Db.Matter.Remove(orphanMatter[0]);
                    Db.SaveChanges();
                }

                ThreadDisposable = true;
                Dispose(true);
                return new Dictionary<string, object>
                           {
                               { "matterNames", matterNames },
                               { "results", results },
                               { "status", statuses }
                           };
            });
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing flag.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && ThreadDisposable)
            {
                Db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
