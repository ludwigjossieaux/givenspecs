using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GivenSpecs
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

    public class ReportedStepMatch
    {
        [JsonProperty("location")]
        public string Location { get; set; }
    }

    public class ReportedStepEmbeddings
    {
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("mime_type")]
        public string MimeType { get; set; }
    }

    public class ReportedStepResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("duration")]
        public long Duration { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
    }

    public class ReportedArgument
    {

    }

    public class ReportedArgument_Table: ReportedArgument
    {
        [JsonProperty("rows")]
        public List<ReportedArgument_TableRow> Rows { get; set; }
        public ReportedArgument_Table()
        {
            this.Rows = new List<ReportedArgument_TableRow>();
        }
    }

    public class ReportedArgument_DocString : ReportedArgument
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class ReportedArgument_TableRow
    {
        [JsonProperty("cells")]
        public List<string> Cells { get; set; }
        public ReportedArgument_TableRow()
        {
            this.Cells = new List<string>();
        }
    }

    public class ReportedTag
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("line")]
        public int Line { get; set; }
    }

    public class GivenSpecsConfig
    {
        [JsonProperty("cucumber_report")]
        public string CucumberReport { get; set; }
    }

    public class FixtureClass: IDisposable
    {
        private List<ReportedFeature> features;
        private int currentScenarioIndex = -1;
        private GivenSpecsConfig config;

        public FixtureClass()
        {
            this.features = new List<ReportedFeature>();

            // Load configuration
            config = new GivenSpecsConfig();
            if(File.Exists(Path.Combine(Environment.CurrentDirectory, "givenspecs.json")))
            {
                var configStr = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "givenspecs.json"));
                config = JsonConvert.DeserializeObject<GivenSpecsConfig>(configStr);
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(config.CucumberReport))
            {
                var json = JsonConvert.SerializeObject(features);
                File.WriteAllText(config.CucumberReport, json, Encoding.UTF8);
            }
        }

        // Json reporter
        
        public void ReportFeature(ReportedFeature feature)
        {
            if (!features.Any(x => x.Id == feature.Id))
            {
                features.Add(feature);
            }
        }

        public void ReportScenario(ReportedFeature feature, ReportedScenario scenario)
        {
            var idx = features.FindIndex(x => x.Id == feature.Id);
            var featToModify = features[idx];
            featToModify.Elements.Add(scenario);
            features[idx] = featToModify;
        }

        public void ReportStep(ReportedFeature feature, ReportedScenario scenario, ReportedStep step)
        {
            var idxFeat = features.FindIndex(x => x.Id == feature.Id);
            var idxScenario = features[idxFeat].Elements.FindIndex(x => x.Id == scenario.Id);
            var scenarioToModify = features[idxFeat].Elements[idxScenario];
            scenarioToModify.Steps.Add(step);
            features[idxFeat].Elements[idxScenario] = scenarioToModify;
        }
    }
}
