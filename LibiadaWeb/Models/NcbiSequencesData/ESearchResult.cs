namespace LibiadaWeb.Models.NcbiSequencesData
{
    using Newtonsoft.Json;

    public class ESearchResult
    {
        [JsonProperty(PropertyName = "webenv")]
        public string NcbiWebEnvironment { get; set; }

        public string QueryKey { get; set; }

        public int Count { get; set; }
    }
}
