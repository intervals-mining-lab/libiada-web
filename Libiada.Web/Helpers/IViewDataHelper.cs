namespace Libiada.Web.Helpers;

using Libiada.Database.Models;
using System;
using System.Collections.Generic;

public interface IViewDataHelper : IDisposable
{
    IViewDataHelper AddCharacteristicsData(CharacteristicCategory characteristicsCategory);
    IViewDataHelper AddFeatures();
    IViewDataHelper AddGroups(bool onlyGenetic = false);
    IViewDataHelper AddLanguages();
    IViewDataHelper AddMinMaxResearchObjects(int min = 1, int max = int.MaxValue);
    IViewDataHelper AddMultisequences();
    IViewDataHelper AddNatures(bool onlyGenetic = false);
    IViewDataHelper AddNotations(bool onlyGenetic = false);
    IViewDataHelper AddPauseTreatments();
    IViewDataHelper AddRemoteDatabases();
    IViewDataHelper AddResearchObjects();
    IViewDataHelper AddResearchObjects(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selection);
    IViewDataHelper AddSequenceGroups();
    IViewDataHelper AddSequenceGroups(Func<SequenceGroup, bool> filter);
    IViewDataHelper AddSequenceTypes(bool onlyGenetic = false);
    IViewDataHelper AddSubmitName(string submitName = "Calculate");
    IViewDataHelper AddTrajectories();
    IViewDataHelper AddTranslators();
    Dictionary<string, object> Build();
    Dictionary<string, object> FillResearchObjectCreationViewData();
    Dictionary<string, object> FillSubsequencesViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName);
    Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsCategory, int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selectionFilter, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, Func<ResearchObject, bool> filter, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName);
    Dictionary<string, object> GetCharacteristicsData(CharacteristicCategory characteristicsCategory);
    Dictionary<string, object> GetResearchObjectsData(int minSelectedResearchObjects, int maxSelectedResearchObjects);
}