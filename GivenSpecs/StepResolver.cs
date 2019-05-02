using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace GivenSpecs
{
    public class StepResolver
    {
        private readonly Assembly _assembly;
        private StepType _lastType;
        private ITestOutputHelper _output;
        private bool hasError = false;

        public StepResolver(Assembly assembly, ITestOutputHelper output)
        {
            _output = output;
            _assembly = assembly;
            _lastType = StepType.Given;
        }

        private List<MethodInfo> GetMethodsOfType<T>()
        {
            var bindings = _assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(x => x is BindingAttribute))
                .ToList();
            var methods = bindings.SelectMany(x => x.GetMethods().Where(m => m.GetCustomAttributes().Any(mAttr => mAttr is T))).ToList();
            return methods;
        }

        private void MatchMethod<T>(List<MethodInfo> methods, string text, Table table) where T: StepBaseAttribute
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
                    var obj = Activator.CreateInstance(m.DeclaringType);
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

        public void Given(string text, Table table = null)
        {
            _output.WriteLine($"-> Given {text}");
            if(hasError)
            {
                _output.WriteLine($"   ... skipped");
                return;
            }
            _lastType = StepType.Given;
            var methods = GetMethodsOfType<GivenAttribute>();
            try
            {
                MatchMethod<GivenAttribute>(methods, text, table);
                _output.WriteLine($"   ... ok");
            }
            catch(Exception ex)
            {
                hasError = true;
                _output.WriteLine($"   ... error: {ex.Message}");
            }
        }

        public void When(string text, Table table = null)
        {
            _output.WriteLine($"-> When {text}");
            if (hasError)
            {
                _output.WriteLine($"   ... skipped");
                return;
            }
            _lastType = StepType.When;
            var methods = GetMethodsOfType<WhenAttribute>();
            try
            {
                MatchMethod<WhenAttribute>(methods, text, table);
                _output.WriteLine($"   ... ok");
            }
            catch (Exception ex)
            {
                hasError = true;
                _output.WriteLine($"   ... error: {ex.Message}");
            }
        }

        public void Then(string text, Table table = null)
        {
            _output.WriteLine($"-> Then {text}");
            if (hasError)
            {
                _output.WriteLine($"   ... skipped");
                return;
            }
            _lastType = StepType.Then;
            var methods = GetMethodsOfType<ThenAttribute>();
            try
            {
                MatchMethod<ThenAttribute>(methods, text, table);
                _output.WriteLine($"   ... ok");
            }
            catch (Exception ex)
            {
                hasError = true;
                _output.WriteLine($"   ... error: {ex.Message}");
            }
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
                var obj = Activator.CreateInstance(m.DeclaringType);
                m.Invoke(obj, null);
            }
        }

        public void AfterScenario()
        {
            var methods = GetMethodsOfType<AfterScenarioAttribute>();
            foreach (var m in methods)
            {
                var obj = Activator.CreateInstance(m.DeclaringType);
                m.Invoke(obj, null);
            }
        }
    }
}
