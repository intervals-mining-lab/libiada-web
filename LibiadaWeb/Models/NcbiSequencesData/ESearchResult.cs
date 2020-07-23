using Newtonsoft.Json;

namespace LibiadaWeb.Models.NcbiSequencesData
{
    public class ESearchResult
    {
        [JsonProperty(PropertyName = "webenv")]
        public string NcbiWebEnvironment { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }
    }
}
