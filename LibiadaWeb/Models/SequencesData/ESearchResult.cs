namespace LibiadaWeb.Models.SequencesData
{
    using Newtonsoft.Json;
    public class ESearchResult
    {
        [JsonProperty(PropertyName = "esearchresult")]
        public NcbiWebEnvironment Response { get; set; }
    }
}
