using Newtonsoft.Json;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedStepEmbeddings
    {
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("mime_type")]
        public string MimeType { get; set; }
    }
}
