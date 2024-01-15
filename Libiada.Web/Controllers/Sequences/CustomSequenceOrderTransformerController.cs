namespace Libiada.Web.Controllers.Sequences;

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bio.Extensions;

using Libiada.Core.Core;
using Libiada.Core.DataTransformers;
using Libiada.Core.Extensions;

using Libiada.Database.Helpers;
using Libiada.Database.Tasks;

using Newtonsoft.Json;
using EnumExtensions = Libiada.Core.Extensions.EnumExtensions;
using Libiada.Web.Tasks;


/// <summary>
/// The custom sequence order transformer controller.
/// </summary>
[Authorize(Roles = "Admin")]
public class CustomSequenceOrderTransformerController : AbstractResultController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomSequenceOrderTransformerController"/> class.
    /// </summary>
    public CustomSequenceOrderTransformerController(ITaskManager taskManager) : base(TaskType.CustomSequenceOrderTransformer, taskManager)
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
        var transformations = Extensions.EnumExtensions.GetSelectList<OrderTransformation>();
        var imageTransformers = Extensions.EnumExtensions.GetSelectList<ImageTransformer>();
        var data = new Dictionary<string, object>
        {
            { "transformations", transformations },
            { "imageTransformers", imageTransformers }
        };
        ViewBag.data = JsonConvert.SerializeObject(data);
        return View();
    }

    /// <summary>
    /// The index.
    /// </summary>
    /// <param name="transformationsSequence">
    /// The transformation link ids.
    /// </param>
    /// <param name="iterationsCount">
    /// Number of transformations iterations.
    /// </param>
    /// <param name="customSequences">
    /// Custom sequences from user input.
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
        IFormFileCollection file)
    {
        return CreateTask(() =>
        {
            int sequencesCount = localFile ? file.Count : customSequences.Length;
            var sourceSequences = new string[sequencesCount];
            var sequences = new Chain[sequencesCount];
            var names = new string[sequencesCount];

            for (int i = 0; i < sequencesCount; i++)
            {
                if (localFile)
                {
                    var sequenceStream = Helpers.FileHelper.GetFileStream(file[i]);
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

            for (int k = 0; k < sequencesCount; k++)
            {
                sequences[k] = new Chain(sourceSequences[k]);
                for (int j = 0; j < iterationsCount; j++)
                {
                    for (int i = 0; i < transformationsSequence.Length; i++)
                    {
                        sequences[k] = transformationsSequence[i] == OrderTransformation.Dissimilar
                                           ? DissimilarChainFactory.Create(sequences[k])
                                           : HighOrderFactory.Create(sequences[k], EnumExtensions.GetLink(transformationsSequence[i]));
                    }
                }
            }

            var transformations = transformationsSequence.Select(ts => ts.GetDisplayValue());

            var result = new Dictionary<string, object>
            {
                { "names", names },
                { "sequences", sequences.Select((s, i) => new { name = names[i], value = s.ToString(" ") }).ToArray() },
                { "transformationsList", transformations },
                { "iterationsCount", iterationsCount }
            };
            
            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
        });
    }
}
