using Newtonsoft.Json;
using System.Collections.Generic;

namespace GivenSpecs.Application.Reporting
{
    public class ReportedStep
    {
        [JsonProperty("arguments")]
        public List<ReportedArgument> Arguments { get; set; }
        [JsonProperty("keyword")]
        public string Keyword { get; set; }
        [JsonProperty("line")]
        public int Line { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("match")]
        public ReportedStepMatch Match { get; set; }
        [JsonProperty("result")]
        public ReportedStepResult Result { get; set; }
        [JsonProperty("embeddings")]
        public List<ReportedStepEmbeddings> Embeddings { get; set; }

        public ReportedStep()
        {
            this.Arguments = new List<ReportedArgument>();
            this.Match = new ReportedStepMatch();
            this.Result = new ReportedStepResult();
            this.Embeddings = new List<ReportedStepEmbeddings>();
        }
    }
}
