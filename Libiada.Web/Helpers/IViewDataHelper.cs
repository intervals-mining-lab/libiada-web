namespace Libiada.Web.Helpers;

using Libiada.Database.Models;

using System;
using System.Collections.Generic;

public interface IViewDataHelper : IDisposable
{
    IViewDataHelper AddCharacteristicsData(CharacteristicCategory characteristicsCategory);
    IViewDataHelper AddFeatures();
    IViewDataHelper AddGroups(bool onlyGenetic = false);
    IViewDataHelper AddImageTransformers();
    IViewDataHelper AddLanguages();
    IViewDataHelper AddMaxPercentageDifferenceRequiredFlag();
    IViewDataHelper AddMinMaxResearchObjects(int min = 1, int max = int.MaxValue);
    IViewDataHelper AddMultisequences();
    IViewDataHelper AddNatures();
    IViewDataHelper AddNotations(bool onlyGenetic = false);
    IViewDataHelper AddOrderTransformations();
    IViewDataHelper AddPauseTreatments();
    IViewDataHelper AddRemoteDatabases();
    IViewDataHelper AddResearchObjects();
    IViewDataHelper AddResearchObjects(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selection);
    IViewDataHelper AddResearchObjectsWithSubsequences();
    IViewDataHelper AddSequenceGroups();
    IViewDataHelper AddSequenceGroupTypes();
    IViewDataHelper AddSequenceTypes(bool onlyGenetic = false);
    IViewDataHelper AddSubmitName(string submitName = "Calculate");
    IViewDataHelper AddTrajectories();
    IViewDataHelper AddTranslators();
    IViewDataHelper SetNature(Nature nature);
    Dictionary<string, object> Build();
}
