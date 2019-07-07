using Newtonsoft.Json;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedArgument_DocString : ReportedArgument
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
