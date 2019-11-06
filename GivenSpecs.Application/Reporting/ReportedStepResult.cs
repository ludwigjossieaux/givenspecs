using Newtonsoft.Json;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedStepResult
    {
        public const string Status_Passed = "passed";
        public const string Status_Failed = "failed"; 
        public const string Status_Ambiguous = "ambiguous";
        public const string Status_Skipped = "skipped";
        public const string Status_Undefined = "undefined";
        public const string Status_Pending = "pending";

        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("status_info")]
        public string StatusInfo { get; set; }
        [JsonProperty("duration")]
        public long Duration { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
    }
}
