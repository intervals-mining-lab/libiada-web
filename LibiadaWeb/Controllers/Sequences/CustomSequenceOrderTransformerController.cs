namespace LibiadaWeb.Controllers.Sequences
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using Bio.Extensions;

    using LibiadaCore.Core;
    using LibiadaCore.Extensions;
    using LibiadaCore.Misc;

    using LibiadaWeb.Extensions;
    using LibiadaWeb.Helpers;
    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;
    using EnumExtensions = LibiadaCore.Extensions.EnumExtensions;

    /// <summary>
    /// The custom sequence order transformer controller.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CustomSequenceOrderTransformerController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSequenceOrderTransformerController"/> class.
        /// </summary>
        public CustomSequenceOrderTransformerController() : base(TaskType.CustomSequenceOrderTransformer)
        {
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
            
            var transformations = EnumHelper.GetSelectList(typeof(OrderTransformation));
            data.Add("transformations", transformations);

            ViewBag.data = JsonConvert.SerializeObject(data);
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="transformationLinkIds">
        /// The transformation link ids.
        /// </param>
        /// <param name="transformationsSequence">
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
            OrderTransformation[] transformationsSequence,
            int iterationsCount,
            string[] customSequences,
            bool localFile,
            HttpPostedFileBase[] file)
        {
            return CreateTask(() =>
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
                        names[i] = $"Custom sequence {i + 1}. Length: {customSequences[i].Length}";
                    }
                }

                for (int k = 0; k < iterationsCount; k++)
                {
                    var sequence = new Chain(sourceSequences[k]);
                    for (int j = 0; j < iterationsCount; j++)
                    {
                        for (int i = 0; i < transformationsSequence.Length; i++)
                        {
                            sequence = transformationsSequence[i] == OrderTransformation.Dissimilar ? DissimilarChainFactory.Create(sequence)
                                                                 : HighOrderFactory.Create(sequence, EnumExtensions.GetLink(transformationsSequence[i]));
                        }
                    }
                }

                var transformations = new Dictionary<int, string>();
                for (int i = 0; i < transformationsSequence.Length; i++)
                {
                    transformations.Add(i, transformationsSequence[i] == OrderTransformation.Dissimilar ? "dissimilar" : $"higher order {EnumExtensions.GetLink(transformationsSequence[i]).GetDisplayValue()}");
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
