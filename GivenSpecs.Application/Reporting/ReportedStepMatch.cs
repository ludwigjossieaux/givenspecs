using Newtonsoft.Json;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedStepMatch
    {
        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
