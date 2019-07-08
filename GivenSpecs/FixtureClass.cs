using GivenSpecs.Application.Reporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GivenSpecs
{
    public class FixtureClass : IDisposable
    {
        private List<ReportedFeature> features;
        private string _cucumberReportPath;

        public FixtureClass()
        {
            this.features = new List<ReportedFeature>();
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(_cucumberReportPath))
            {
                var json = JsonConvert.SerializeObject(features);
                File.WriteAllText(_cucumberReportPath, json, Encoding.UTF8);
            }
        }

        // Configuration
        public void SetCucumberReportPath(string path)
        {
            this._cucumberReportPath = path;
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
