namespace Libiada.Web.Helpers;

public interface IViewDataHelper : IDisposable
{
    Dictionary<string, object> FillMatterCreationViewData();
    Dictionary<string, object> FillSubsequencesViewData(int minSelectedMatters, int maxSelectedMatters, string submitName);
    Dictionary<string, object> FillViewData(CharacteristicCategory characteristicsType, int minSelectedMatters, int maxSelectedMatters, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedMatters, int maxSelectedMatters, Func<Matter, bool> filter, Func<Matter, bool> selectionFilter, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedMatters, int maxSelectedMatters, Func<Matter, bool> filter, string submitName);
    Dictionary<string, object> FillViewData(int minSelectedMatters, int maxSelectedMatters, string submitName);
    Dictionary<string, object> GetCharacteristicsData(CharacteristicCategory characteristicsCategory);
    Dictionary<string, object> GetMattersData(int minSelectedMatters, int maxSelectedMatters);
}