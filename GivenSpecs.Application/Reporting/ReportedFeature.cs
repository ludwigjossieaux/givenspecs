using Newtonsoft.Json;
using System.Collections.Generic;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedFeature
    {
        [JsonProperty("keyword")]
        public string Keyword { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("line")]
        public int Line { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("tags")]
        public List<ReportedTag> Tags { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("elements")]
        public List<ReportedScenario> Elements { get; set; }

        public ReportedFeature()
        {
            this.Elements = new List<ReportedScenario>();
            this.Tags = new List<ReportedTag>();
        }
    }
}
