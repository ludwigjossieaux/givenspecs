using Newtonsoft.Json;
using System.Collections.Generic;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedScenario
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("keyword")]
        public string Keyword { get; set; }
        [JsonProperty("line")]
        public int Line { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("tags")]
        public List<ReportedTag> Tags { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("steps")]
        public List<ReportedStep> Steps { get; set; }

        public ReportedScenario()
        {
            this.Tags = new List<ReportedTag>();
            this.Steps = new List<ReportedStep>();
        }
    }
}
