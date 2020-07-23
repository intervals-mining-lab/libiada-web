using Newtonsoft.Json;

namespace LibiadaWeb.Models.NcbiSequencesData
{
    public class ESearchResponce
    {
        [JsonProperty(PropertyName = "esearchresult")]
        public ESearchResult ESearchResult { get; set; }
    }
}
