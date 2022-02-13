namespace LibiadaWeb.Models.CalculatorsData
{
    public struct LocalCharacteristicsData
    {
        public readonly string MatterName;

        public readonly FragmentData[] FragmentsData;

        public readonly double[][] DifferenceData;

        public readonly double[][] FourierData;

        public readonly double[][] AutocorrelationData;

        public LocalCharacteristicsData(
            string matterName, 
            FragmentData[] fragmentsData, 
            double[][] differenceData, 
            double[][] fourierData, 
            double[][] autocorrelationData)
        {
            MatterName = matterName;
            FragmentsData = fragmentsData;
            DifferenceData = differenceData;
            FourierData = fourierData;
            AutocorrelationData = autocorrelationData;
        }
    }
}
