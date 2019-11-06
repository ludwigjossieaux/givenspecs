using GivenSpecs.Application.Exceptions;
using GivenSpecs.Application.Reporting;
using GivenSpecs.Application.Tables;
using GivenSpecs.Attributes;
using GivenSpecs.Enumerations;
using GivenSpecs.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace GivenSpecs
{
    public class StepResolver
    {
        private readonly Assembly _assembly;
        private StepTypeEnum _lastType;
        private ITestOutputHelper _output;

        private bool _shouldSkipStep;

        //private bool hasError = false;
        private bool _isUndefined = false;
        private string _undefinedTag = "inconclusive";
        //private string inconclusiveStatusTag = "inconclusive";
        private string _lastError = "";
        private ScenarioContext _context;
        private ReportedFeature _feature;
        private ReportedScenario _scenario;
        public List<ReportedStepEmbeddings> _currentEmbeddings;
        private List<(string, string)> _replacements;
        private FixtureClass _fixture;

        public StepResolver(Assembly assembly, ITestOutputHelper output)
        {
            _output = output;
            _assembly = assembly;
            _lastType = StepTypeEnum.Given;
            _context = new ScenarioContext(this);
        }

        public ReportedScenario GetReportedScenario()
        {
            return _scenario;
        }

        public void ScenarioReset(FixtureClass fixture, ReportedFeature feature, List<(string, string)> replacements, ReportedScenario scenario)
        {
            _shouldSkipStep = false;
            _lastError = "";
            _context = new ScenarioContext(this);
            _fixture = fixture;
            _feature = feature;
            _scenario = scenario;
            _replacements = replacements;
        }

        private void ProcessStep<T>(string text, StepTypeEnum step, string multiline, Table table = null) where T : StepBaseAttribute
        {
            var applyReplacements = new Func<string, string>((string input) =>
            {
                if (_replacements == null || _replacements.Count == 0)
                {
                    return input;
                }
                var result = input;
                foreach (var r in _replacements)
                {
                    result = result.Replace($"<{r.Item1}>", r.Item2);
                }
                return result;
            });

            text = applyReplacements(text);

            _currentEmbeddings = new List<ReportedStepEmbeddings>();
            var reportedStep = new ReportedStep()
            {
                Keyword = step.ToString() + " ",
                Name = text,
            };
            _output.WriteLine($"-> {step.ToString()} {text}");

            // Multiline
            if (multiline != null)
            {
                multiline = applyReplacements(multiline);

                _output.WriteLine(multiline);
                var reportedDocstring = new ReportedArgument_DocString()
                {
                    Content = multiline
                };
                reportedStep.Arguments.Add(reportedDocstring);
            }

            // Table
            if (table != null)
            {
                table.ApplyReplacements(applyReplacements);

                _output.WriteLine(table.ToString());
                var reportedTable = new ReportedArgument_Table();
                var reportedTable_RowHeader = new ReportedArgument_TableRow();
                foreach (var h in table.GetHeaders())
                {
                    reportedTable_RowHeader.Cells.Add(h);
                }
                reportedTable.Rows.Add(reportedTable_RowHeader);
                foreach (var r in table.GetRows())
                {
                    var reportedTable_Row = new ReportedArgument_TableRow();
                    foreach (var cell in r.Cells)
                    {
                        reportedTable_Row.Cells.Add(cell.Value);
                    }
                    reportedTable.Rows.Add(reportedTable_Row);
                }
                reportedStep.Arguments.Add(reportedTable);
            }

            var stepStart = DateTime.UtcNow;

            if (_isUndefined)
            {
                _output.WriteLine($"   ... undefined");
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "undefined",
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                return;
            }
            if (_shouldSkipStep)
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

            // Find & Invoke

            var methods = MethodsHelper.GetMethodsOfType<T>(_assembly);
            var paramConverters = MethodsHelper.GetParameterConverters(_assembly);
            try
            {
                MethodsHelper.MatchMethodAndInvoke<T>(methods, paramConverters, reportedStep.Keyword, text, multiline, table, _context);
                reportedStep.Embeddings = _currentEmbeddings;
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "passed",
                    Duration = (long)DateTime.UtcNow.Subtract(stepStart).TotalMilliseconds * 1000000
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                _output.WriteLine($"   ... ok");
            }
            catch (StepNotFoundException ex)
            {
                _output.WriteLine($"   ... error: {ex.Message}");
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "pending",
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                _shouldSkipStep = true;
                _lastError = ex.Message;
                return;
            }
            catch (MultipleStepsFoundException ex)
            {
                _output.WriteLine($"   ... error: {ex.Message}");
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "ambiguous",
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                _shouldSkipStep = true;
                _lastError = ex.Message;
                return;
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _output.WriteLine($"   ... error: {errorMessage}");
                reportedStep.Embeddings = _currentEmbeddings;
                reportedStep.Result = new ReportedStepResult()
                {
                    Status = "failed",
                    Duration = (long)DateTime.UtcNow.Subtract(stepStart).TotalMilliseconds * 1000000,
                    ErrorMessage = errorMessage
                };
                _fixture.ReportStep(_feature, _scenario, reportedStep);
                _shouldSkipStep = true;
                _lastError = errorMessage;
                return;
            }
        }

        public void SetCucumberReportPath(string path)
        {
            _fixture.SetCucumberReportPath(path);
        }

        public void SetUndefined(bool value, string tagName)
        {
            _isUndefined = value;
            _undefinedTag = tagName;
        }

        public void Given(string text, string multiline, Table table = null)
        {
            ProcessStep<GivenAttribute>(text, StepTypeEnum.Given, multiline, table);
        }

        public void When(string text, string multiline, Table table = null)
        {
            ProcessStep<WhenAttribute>(text, StepTypeEnum.When, multiline, table);
        }

        public void Then(string text, string multiline, Table table = null)
        {
            ProcessStep<ThenAttribute>(text, StepTypeEnum.Then, multiline, table);
        }

        public void And(string text, string multiline, Table table = null)
        {
            if (_lastType == StepTypeEnum.Given) Given(text, multiline, table);
            if (_lastType == StepTypeEnum.When) When(text, multiline, table);
            if (_lastType == StepTypeEnum.Then) Then(text, multiline, table);
        }

        public void But(string text, string multiline, Table table = null)
        {
            And(text, multiline, table);
        }

        public void BeforeScenario()
        {
            var methods = MethodsHelper.GetMethodsOfType<BeforeScenarioAttribute>(_assembly);
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

        public void AfterScenario()
        {
            var methods = MethodsHelper.GetMethodsOfType<AfterScenarioAttribute>(_assembly);
            foreach (var m in methods)
            {
                var ctrParams = new object[]
                {
                    _context
                };
                var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                m.Invoke(obj, null);
            }
            if(_shouldSkipStep && !string.IsNullOrWhiteSpace(_lastError))
            {
                throw new Exception(_lastError);
            }
        }

        public void BeforeStep(string stepText)
        {
            var methods = MethodsHelper.GetMethodsOfType<BeforeStepAttribute>(_assembly);
            foreach (var m in methods)
            {
                var ctrParams = new object[]
                {
                    _context
                };
                var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                var methodParams = new object[]
                {
                    stepText
                };
                m.Invoke(obj, methodParams);
            }
        }

        public void AfterStep(string stepText)
        {
            var methods = MethodsHelper.GetMethodsOfType<AfterStepAttribute>(_assembly);
            foreach (var m in methods)
            {
                var ctrParams = new object[]
                {
                    _context
                };
                var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                var methodParams = new object[]
                {
                    stepText
                };
                m.Invoke(obj, methodParams);
            }
        }
    }
}
