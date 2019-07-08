using Newtonsoft.Json;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedTag
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("line")]
        public int Line { get; set; }
    }
}
