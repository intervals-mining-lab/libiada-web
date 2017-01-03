namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Misc;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models.Repositories.Sequences;

    using Newtonsoft.Json;

    /// <summary>
    /// The custom sequence order transformer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CustomSequenceOrderTransformerController : AbstractResultController
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
        /// Initializes a new instance of the <see cref="CustomSequenceOrderTransformerController"/> class.
        /// </summary>
        public CustomSequenceOrderTransformerController() : base("Custom sequences order transformation")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var data = new Dictionary<string, object>();

            var transformationLinks = new[] { Link.Start, Link.End, Link.CycleStart, Link.CycleEnd };
            transformationLinks = transformationLinks.OrderBy(n => (int)n).ToArray();
            data.Add("transformationLinks", transformationLinks.ToSelectList());

            var operations = new List<SelectListItem> { new SelectListItem { Text = "Dissimilar", Value = 1.ToString() }, new SelectListItem { Text = "Higher order", Value = 2.ToString() } };
            data.Add("operations", operations);

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="transformationLinkIds">
        /// The transformation link ids.
        /// </param>
        /// <param name="transformationIds">
        /// The transformation ids.
        /// </param>
        /// <param name="iterationsCount">
        /// Number of transformations iterations.
        /// </param>
        /// <param name="customSequences">
        /// Custom sequences inputed by user.
        /// </param>
        /// <param name="localFile">
        /// Local file flag.
        /// </param>
        /// <param name="file">
        /// Sequences as fasta files.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            int[] transformationLinkIds,
            int[] transformationIds,
            int iterationsCount,
            string[] customSequences,
            bool localFile,
            HttpPostedFileBase[] file)
        {
            return Action(() =>
            {
                int sequencesCount = localFile ? Request.Files.Count : customSequences.Length;
                var sourceSequences = new string[sequencesCount];
                var sequences = new string[sequencesCount];
                var names = new string[sequencesCount];

                for (int i = 0; i < sequencesCount; i++)
                {
                    if (localFile)
                    {
                        var sequenceStream = FileHelper.GetFileStream(file[i]);
                        var fastaSequence = NcbiHelper.GetFastaSequence(sequenceStream);
                        sourceSequences[i] = fastaSequence.ConvertToString();
                        names[i] = fastaSequence.ID;
                    }
                    else
                    {
                        sourceSequences[i] = customSequences[i];
                        names[i] = "Custom sequence " + (i + 1) + ". Length: " + customSequences[i].Length;
                    }
                }

                for (int k = 0; k < iterationsCount; k++)
                {
                    var sequence = new Chain(sourceSequences[k]);
                    for (int j = 0; j < iterationsCount; j++)
                    {
                        for (int i = 0; i < transformationIds.Length; i++)
                        {
                            if (transformationIds[i] == 1)
                            {
                                sequence = DissimilarChainFactory.Create(sequence);
                            }
                            else
                            {
                                sequence = HighOrderFactory.Create(sequence, (Link)transformationLinkIds[i]);
                            }
                        }
                    }
                }

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationIds.Length; i++)
                {
                    var link = ((Link)transformationLinkIds[i]).GetDisplayValue();
                    transformations.Add(i, transformationIds[i] == 1 ? "dissimilar" : "higher order " + link);
                }

                var result = new Dictionary<string, object>
                {
                    { "names", names },
                    { "sequences", sequences },
                    { "transformationsList", transformations },
                    { "iterationsCount", iterationsCount }
                };
                return result;
            });
        }
    }
}
