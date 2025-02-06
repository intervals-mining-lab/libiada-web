namespace Libiada.Web.Helpers;

using Libiada.Database.Models;
using System;
using System.Collections.Generic;

public interface IViewDataHelper : IDisposable
{
    ViewDataHelper AddCharacteristicsData(CharacteristicCategory characteristicsCategory);
    ViewDataHelper AddFeatures();
    ViewDataHelper AddGroups(bool onlyGenetic = false);
    ViewDataHelper AddLanguages();
    ViewDataHelper AddMinMaxResearchObjects(int min, int max);
    ViewDataHelper AddMultisequences();
    ViewDataHelper AddNatures(bool onlyGenetic = false);
    ViewDataHelper AddNotations(bool onlyGenetic = false);
    ViewDataHelper AddPauseTreatments();
    ViewDataHelper AddRemoteDatabases();
    ViewDataHelper AddResearchObjects();
    ViewDataHelper AddResearchObjects(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selection);
    ViewDataHelper AddSequenceGroups();
    ViewDataHelper AddSequenceGroups(Func<SequenceGroup, bool> filter);
    ViewDataHelper AddSequenceTypes(bool onlyGenetic = false);
    ViewDataHelper AddSubmitName(string submitName);
    ViewDataHelper AddTrajectories();
    ViewDataHelper AddTranslators();
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