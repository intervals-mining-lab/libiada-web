namespace Libiada.Web.Controllers.Sequences;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Libiada.Core.Extensions;
using Libiada.Web.Extensions;
using Libiada.Web.Helpers;
using Libiada.Database.Models.Repositories.Sequences;
using Libiada.Database.Tasks;
using Newtonsoft.Json;
using Libiada.Database.Models.CalculatorsData;
using Libiada.Web.Tasks;

[Authorize(Roles = "Admin")]
public class BatchPoemsImportController : AbstractResultController
{
    private readonly ILibiadaDatabaseEntitiesFactory dbFactory;
    private readonly Cache cache;

    public BatchPoemsImportController(ILibiadaDatabaseEntitiesFactory dbFactory, ITaskManager taskManager, Cache cache) 
        : base(TaskType.BatchPoemsImport, taskManager)
    {
        this.dbFactory = dbFactory;
        this.cache = cache;
    }

    public ActionResult Index()
    {
        var viewData = new Dictionary<string, object>
        {
            { "notations", new [] { Notation.Letters, Notation.Consonance }.ToSelectListWithNature() }
        };
        ViewBag.data = JsonConvert.SerializeObject(viewData);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(Notation notation, bool dropPunctuation, IFormFileCollection files)
    {
        return CreateTask(() =>
        {
            var importResults = new List<MatterImportResult>();

            Matter[] matters = cache.Matters.Where(m => m.Nature == Nature.Literature).ToArray();

            for (int i = 0; i < files.Count; i++)
            {
                string sequenceName = files[i].FileName.Substring(0, files[i].FileName.LastIndexOf('.'));

                var importResult = new MatterImportResult()
                {
                    MatterName = sequenceName
                };

                try
                {
                    var sequence = new CommonSequence
                    {
                        Notation = notation
                    };


                    if (matters.Any(m => m.Name == sequenceName))
                    {
                        var matter = matters.Single(m => m.Name == sequenceName);
                        sequence.MatterId = matter.Id;
                        importResult.MatterName = matter.Name;
                        importResult.SequenceType = matter.SequenceType.GetDisplayValue();
                        importResult.Group = matter.Group.GetDisplayValue();
                        importResult.Result = "Successfully imported poem for existing matter";
                    }
                    else
                    {
                        sequence.Matter = new Matter
                        {
                            Name = sequenceName,
                            Group = Group.ClassicalLiterature,
                            Nature = Nature.Literature,
                            SequenceType = SequenceType.CompleteText
                        };

                        importResult.MatterName = sequence.Matter.Name;
                        importResult.SequenceType = sequence.Matter.SequenceType.GetDisplayValue();
                        importResult.Group = sequence.Matter.Group.GetDisplayValue();
                        importResult.Result = "Successfully imported poem and created matter";
                    }

                    var repository = new LiteratureSequenceRepository(dbFactory, cache);

                    repository.Create(sequence, FileHelper.GetFileStream(files[i]), Language.Russian, true, Translator.NoneOrManual, dropPunctuation);
                    importResult.Status = "Success";

                    importResults.Add(importResult);
                }
                catch (Exception exception)
                {
                    importResult.Result = $"Failed to import poem: {exception.Message}";
                    while (exception.InnerException != null)
                    {
                        importResult.Result += $" {exception.InnerException.Message}";

                        exception = exception.InnerException;
                    }

                    importResult.Status = "Error";
                    importResults.Add(importResult);
                }
            }

            var result = new Dictionary<string, object> { { "result", importResults } };

            return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };

        });
    }
}
