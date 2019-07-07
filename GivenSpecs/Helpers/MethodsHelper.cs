using GivenSpecs.Application.Tables;
using GivenSpecs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GivenSpecs.Helpers
{
    public static class MethodsHelper
    {
        public static List<MethodInfo> GetMethodsOfType<T>(Assembly _assembly)
        {
            var bindings = _assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(x => x is BindingAttribute))
                .ToList();
            var methods = bindings.SelectMany(x => x.GetMethods().Where(m => m.GetCustomAttributes().Any(mAttr => mAttr is T))).ToList();
            return methods;
        }

        public static bool MatchMethodAndInvoke<T>(List<MethodInfo> methods, string text, string multiline, Table table, ScenarioContext context) where T : StepBaseAttribute
        {
            foreach (var m in methods)
            {
                var attribute = m.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
                var rgx = new Regex(attribute.Regex);
                var match = rgx.Match(text);
                if (match.Success)
                {
                    var groups = match.Groups.Select(x => x.Value).Skip(1).ToList<object>();
                    if (multiline != null)
                    {
                        groups.Add(multiline);
                    }
                    if (table != null)
                    {
                        groups.Add(table);
                    }
                    var ctrParams = new object[]
                    {
                        context
                    };
                    var obj = Activator.CreateInstance(m.DeclaringType, ctrParams);
                    m.Invoke(obj, groups.ToArray());
                    return true;
                }
            }
            return false;
        }
    }
}
