namespace LibiadaWeb.Models.CalculatorsData
{
    using Newtonsoft.Json;

    public class LocalCharacteristicsData
    {
        public string matterName;

        public FragmentData[] fragmentsData;

        public double[][] differenceData;

        public double[][] fourierData;

        public double[][] autocorrelationData;
    }
}