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
        public Dictionary<string, object> Data { get; set; }
        public ScenarioContext()
        {
            this.Data = new Dictionary<string, object>();
        }
    }

    public class StepResolver
    {
        private readonly Assembly _assembly;
        private StepType _lastType;
        private ITestOutputHelper _output;
        private bool hasError = false;
        private ScenarioContext _context;
        private ReportedFeature _feature;
        private ReportedScenario _scenario;

        public StepResolver(Assembly assembly, ITestOutputHelper output)
        {
            _output = output;
            _assembly = assembly;
            _lastType = StepType.Given;
            _context = new ScenarioContext();
        }

        private FixtureClass _fixture;
        public void ScenarioReset(FixtureClass fixture, ReportedFeature feature, ReportedScenario scenario)
        {
            hasError = false;
            _context = new ScenarioContext();
            _fixture = fixture;
            _feature = feature;
            _scenario = scenario;
        }

        private List<MethodInfo> GetMethodsOfType<T>()
        {
            var bindings = _assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(x => x is BindingAttribute))
                .ToList();
            var methods = bindings.SelectMany(x => x.GetMethods().Where(m => m.GetCustomAttributes().Any(mAttr => mAttr is T))).ToList();
            return methods;
        }

        private void MatchMethod<T>(List<MethodInfo> methods, string text, Table table, ReportedStep step) where T: StepBaseAttribute
        {
            foreach(var m in methods)
            {
                var attribute = m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
                var rgx = new Regex(attribute.Regex);
                var match = rgx.Match(text);
                if(match.Success)
                {
                   
                    var groups = match.Groups.Select(x => x.Value).Skip(1).ToList<object>();
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
            throw new Exception($"no step for {typeof(T).ToString()} -> {text}");
        }

        public void CleanErrorState()
        {
            hasError = false;
        }

        private void ProcessStep<T>(string text, StepType step, Table table = null) where T: StepBaseAttribute
        {
            var reportedStep = new ReportedStep()
            {
                Keyword = step.ToString() + " ",
                Name = text,
            };
            _output.WriteLine($"-> {step.ToString()} {text}");
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
                MatchMethod<T>(methods, text, table, reportedStep);
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "passed",
                    Duration = (long) DateTime.UtcNow.Subtract(stepStart).TotalMilliseconds
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                _output.WriteLine($"   ... ok");
            }
            catch (Exception ex)
            {
                hasError = true;
                _output.WriteLine($"   ... error: {ex.Message}");
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "failed",
                    Duration = (long) DateTime.UtcNow.Subtract(stepStart).TotalMilliseconds,
                    ErrorMessage = ex.Message
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
            }
        }

        public void Given(string text, Table table = null)
        {
            ProcessStep<GivenAttribute>(text, StepType.Given, table);
        }

        public void When(string text, Table table = null)
        {
            ProcessStep<WhenAttribute>(text, StepType.When, table);
        }

        public void Then(string text, Table table = null)
        {
            ProcessStep<ThenAttribute>(text, StepType.Then, table);
        }

        public void And(string text, Table table = null)
        {
            if (_lastType == StepType.Given) Given(text, table);
            if (_lastType == StepType.When) When(text, table);
            if (_lastType == StepType.Then) Then(text, table);
        }

        public void But(string text, Table table = null)
        {
            And(text, table);
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
        }
    }
}
