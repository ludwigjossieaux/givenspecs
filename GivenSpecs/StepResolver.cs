using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GivenSpecs
{
    public class StepResolver
    {
        private readonly Assembly _assembly;
        private StepType _lastType;

        public StepResolver(Assembly assembly)
        {
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

        public void Given(string text, Table table = null)
        {
            _lastType = StepType.Given;
            var methods = GetMethodsOfType<GivenAttribute>();
            MatchMethod<GivenAttribute>(methods, text, table);
        }

        public void When(string text, Table table = null)
        {
            _lastType = StepType.When;
            var methods = GetMethodsOfType<WhenAttribute>();
            MatchMethod<WhenAttribute>(methods, text, table);
        }

        public void Then(string text, Table table = null)
        {
            _lastType = StepType.Then;
            var methods = GetMethodsOfType<ThenAttribute>();
            MatchMethod<ThenAttribute>(methods, text, table);
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
    }
}
