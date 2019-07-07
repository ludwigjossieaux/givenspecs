using GivenSpecs.Application.Reporting;
using System.Collections.Generic;
using System.Linq;

namespace GivenSpecs
{
    public class ScenarioContext
    {
        private readonly StepResolver _resolver;

        public Dictionary<string, object> Data { get; set; }

        public ScenarioContext(StepResolver resolver)
        {
            this._resolver = resolver;
            this.Data = new Dictionary<string, object>();
        }

        public void SetCucumberReportPath(string path)
        {
            this._resolver.SetCucumberReportPath(path);
        }

        public List<string> CurrentScenario_Tags()
        {
            var scenario = _resolver.GetReportedScenario();
            return scenario.Tags.Select(x => x.Name).ToList();
        }

        public void Attach(string data, string mimeType)
        {
            _resolver._currentEmbeddings.Add(new ReportedStepEmbeddings()
            {
                Data = data,
                MimeType = mimeType
            });
        }
    }
}
