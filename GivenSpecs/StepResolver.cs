using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

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

    public class StepResolver
    {
        private readonly Assembly _assembly;
        private StepType _lastType;
        private ITestOutputHelper _output;
        private bool hasError = false;
        private string lastError = "";
        private ScenarioContext _context;
        private ReportedFeature _feature;
        private ReportedScenario _scenario;
        public List<ReportedStepEmbeddings> _currentEmbeddings;
        private List<(string, string)> _replacements;

        public StepResolver(Assembly assembly, ITestOutputHelper output)
        {
            _output = output;
            _assembly = assembly;
            _lastType = StepType.Given;
            _context = new ScenarioContext(this);
        }

        public ReportedScenario GetReportedScenario()
        {
            return _scenario;
        }

        private FixtureClass _fixture;
        public void ScenarioReset(FixtureClass fixture, ReportedFeature feature, List<(string, string)> replacements, ReportedScenario scenario)
        {
            hasError = false;
            lastError = "";
            _context = new ScenarioContext(this);
            _fixture = fixture;
            _feature = feature;
            _scenario = scenario;
            _replacements = replacements;
        }

        private List<MethodInfo> GetMethodsOfType<T>()
        {
            var bindings = _assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(x => x is BindingAttribute))
                .ToList();
            var methods = bindings.SelectMany(x => x.GetMethods().Where(m => m.GetCustomAttributes().Any(mAttr => mAttr is T))).ToList();
            return methods;
        }

        private void MatchMethod<T>(List<MethodInfo> methods, string text, string multiline, Table table, ReportedStep step) where T: StepBaseAttribute
        {
            foreach(var m in methods)
            {
                var attribute = m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
                var rgx = new Regex(attribute.Regex);
                var match = rgx.Match(text);
                if(match.Success)
                {
                    var groups = match.Groups.Select(x => x.Value).Skip(1).ToList<object>();
                    if(multiline != null)
                    {
                        groups.Add(multiline);
                    }
                    if(table != null)
                    {
                        groups.Add(table);
                    }
                    var ctrParams = new object[]
                    {
                        _context
                    };
                    var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                    m.Invoke(obj, groups.ToArray());
                    return;
                }
            }
            throw new Exception($"no step for {step.Keyword}-> {text}");
        }

        private void ProcessStep<T>(string text, StepType step, string multiline, Table table = null) where T: StepBaseAttribute
        {
            foreach (var r in _replacements)
            {
                text = text.Replace($"<{r.Item1}>", r.Item2);
            }

            _currentEmbeddings = new List<ReportedStepEmbeddings>();
            var reportedStep = new ReportedStep()
            {
                Keyword = step.ToString() + " ",
                Name = text,
            };
            _output.WriteLine($"-> {step.ToString()} {text}");
            if (multiline != null)
            {
                _output.WriteLine(multiline);
                var reportedDocstring = new ReportedArgument_DocString()
                {
                    Content = multiline
                };
                reportedStep.Arguments.Add(reportedDocstring);
            }
            if(table != null)
            {
                _output.WriteLine(table.ToString());
                var reportedTable = new ReportedArgument_Table();
                var reportedTable_RowHeader = new ReportedArgument_TableRow();
                foreach (var h in table.Header)
                {
                    reportedTable_RowHeader.Cells.Add(h);
                }
                reportedTable.Rows.Add(reportedTable_RowHeader);
                foreach (var r in table.Rows)
                {
                    var reportedTable_Row = new ReportedArgument_TableRow();
                    foreach(var cell in r)
                    {
                        reportedTable_Row.Cells.Add(cell.Value);
                    }
                    reportedTable.Rows.Add(reportedTable_Row);
                }
                reportedStep.Arguments.Add(reportedTable);
            }
            var stepStart = DateTime.UtcNow;
            if (hasError)
            {
                _output.WriteLine($"   ... skipped");
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "skipped",
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                return;
            }
            _lastType = step;
            var methods = GetMethodsOfType<T>();
            try
            {
                MatchMethod<T>(methods, text, multiline, table, reportedStep);
                reportedStep.Embeddings = _currentEmbeddings;
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "passed",
                    Duration = (long) DateTime.UtcNow.Subtract(stepStart).TotalMilliseconds * 1000000
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                _output.WriteLine($"   ... ok");
            }
            catch (Exception ex)
            {
                hasError = true;
                lastError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _output.WriteLine($"   ... error: {lastError}");
                reportedStep.Embeddings = _currentEmbeddings;
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "failed",
                    Duration = (long) DateTime.UtcNow.Subtract(stepStart).TotalMilliseconds * 1000000,
                    ErrorMessage = ex.Message
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
            }
        }

        public void Given(string text, string multiline, Table table = null)
        {
            ProcessStep<GivenAttribute>(text, StepType.Given, multiline, table);
        }

        public void When(string text, string multiline, Table table = null)
        {
            ProcessStep<WhenAttribute>(text, StepType.When, multiline, table);
        }

        public void Then(string text, string multiline, Table table = null)
        {
            ProcessStep<ThenAttribute>(text, StepType.Then, multiline, table);
        }

        public void And(string text, string multiline, Table table = null)
        {
            if (_lastType == StepType.Given) Given(text, multiline, table);
            if (_lastType == StepType.When) When(text, multiline, table);
            if (_lastType == StepType.Then) Then(text, multiline, table);
        }

        public void But(string text, string multiline, Table table = null)
        {
            And(text, multiline, table);
        }

        public void BeforeScenario()
        {
            var methods = GetMethodsOfType<BeforeScenarioAttribute>();
            foreach(var m in methods)
            {
                var ctrParams = new object[]
                {
                    _context
                };
                var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                m.Invoke(obj, null);
            }
        }

        public void AfterScenario()
        {
            var methods = GetMethodsOfType<AfterScenarioAttribute>();
            foreach (var m in methods)
            {
                var ctrParams = new object[]
                {
                    _context
                };
                var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                m.Invoke(obj, null);
            }
            if(hasError)
            {
                throw new Exception(lastError);
            }
        }
    }
}
