using Newtonsoft.Json;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedStepResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("duration")]
        public long Duration { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
    }
}
