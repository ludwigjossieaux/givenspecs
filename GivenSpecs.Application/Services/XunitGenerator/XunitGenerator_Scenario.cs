using GivenSpecs.Application.Reporting;
using System.Collections.Generic;
using System.Linq;

namespace GivenSpecs.Application.Services.XunitGenerator
{
    public class XunitGenerator_Scenario
    {
        public string DisplayName { get; set; }
        public List<string> Tags { get; set; }
        public string MethodName { get; set; }
        public List<XunitGenerator_Step> Steps { get; set; }
        public List<(string, string)> Parameters { get; set; }
        public List<XunitGenerator_ScenarioExample> Examples { get; set; }
        public ReportedScenario Reported { get; set; }
        public string ParametersString
        {
            get
            {
                return string.Join(", ", Parameters.Select(x => $"string {x.Item2}"));
            }
        }
        public string ParametersMap
        {
            get
            {
                return string.Join(", ", Parameters.Select(x => $"(@\"{x.Item1}\", {x.Item2})"));
            }
        }

        public XunitGenerator_Scenario()
        {
            this.Tags = new List<string>();
            this.Steps = new List<XunitGenerator_Step>();
            this.Parameters = new List<(string, string)>();
            this.Examples = new List<XunitGenerator_ScenarioExample>();
        }
    }
}
