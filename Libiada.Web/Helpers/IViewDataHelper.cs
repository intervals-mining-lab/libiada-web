namespace Libiada.Web.Helpers;

public interface IViewDataHelper : IDisposable
{
    Dictionary<string, object> FillResearchObjectCreationViewData();
    Dictionary<string, object> FillSubsequencesViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName);
    Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsType, int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, Func<ResearchObject, bool> filter, Func<ResearchObject, bool> selectionFilter, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, Func<ResearchObject, bool> filter, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedResearchObjects, int maxSelectedResearchObjects, string submitName);
    Dictionary<string, object> GetCharacteristicsData(CharacteristicCategory characteristicsCategory);
    Dictionary<string, object> GetResearchObjectsData(int minSelectedResearchObjects, int maxSelectedResearchObjects);
}