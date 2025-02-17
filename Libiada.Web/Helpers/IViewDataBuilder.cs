namespace Libiada.Web.Helpers;

using Libiada.Database.Models;

using System;
using System.Collections.Generic;

public interface IViewDataBuilder : IDisposable
{
    IViewDataBuilder AddCharacteristicsData(CharacteristicCategory characteristicsCategory);
    IViewDataBuilder AddFeatures();
    IViewDataBuilder AddGroups(bool onlyGenetic = false);
    IViewDataBuilder AddImageTransformers();
    IViewDataBuilder AddLanguages();
    IViewDataBuilder AddMaxPercentageDifferenceRequiredFlag();
    IViewDataBuilder AddMinMaxResearchObjects(int min = 1, int max = int.MaxValue);
    IViewDataBuilder AddMultisequences();
    IViewDataBuilder AddNatures();
    IViewDataBuilder AddNotations(bool onlyGenetic = false);
    IViewDataBuilder AddOrderTransformations();
    IViewDataBuilder AddPauseTreatments();
    IViewDataBuilder AddRemoteDatabases();
    IViewDataBuilder AddResearchObjects();
    IViewDataBuilder AddResearchObjects(Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selection);
    IViewDataBuilder AddResearchObjectsWithSubsequences();
    IViewDataBuilder AddSequenceGroups();
    IViewDataBuilder AddSequenceGroupTypes();
    IViewDataBuilder AddSequenceTypes(bool onlyGenetic = false);
    IViewDataBuilder AddSubmitName(string submitName = "Calculate");
    IViewDataBuilder AddTrajectories();
    IViewDataBuilder AddTranslators();
    IViewDataBuilder SetNature(Nature nature);
    Dictionary<string, object> Build();
}
