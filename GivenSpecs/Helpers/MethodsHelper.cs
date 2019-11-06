using GivenSpecs.Application.Exceptions;
using GivenSpecs.Application.Tables;
using GivenSpecs.Attributes;
using GivenSpecs.Interfaces;
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

        public static List<Type> GetParameterConverters(Assembly _assembly)
        {
            var types = _assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.Name == "IParameterConverter`1"))
                .ToList();
            return types;
        }

        public static bool MatchMethodAndInvoke<T>(
            List<MethodInfo> methods, 
            List<Type> paramConverters,
            string keyword,
            string text, 
            string multiline, 
            Table table, 
            ScenarioContext context) where T : StepBaseAttribute
        {
            var matchingMethods = new List<(MethodInfo, Match)>();

            // Search for matching methods
            foreach(var method in methods)
            {
                var attribute = method.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
                var rgx = new Regex(attribute.Regex);
                var methodMatch = rgx.Match(text);
                if(methodMatch.Success)
                {
                    matchingMethods.Add((method, methodMatch));
                }
            }

            // If no match -> exception
            if(!matchingMethods.Any())
            {
                throw new StepNotFoundException(keyword, text);
            }

            // If more than 1 match
            if(matchingMethods.Count > 1)
            {
                throw new MultipleStepsFoundException(keyword, text);
            }

            var m = matchingMethods[0].Item1;
            var match = matchingMethods[0].Item2;

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
            var parameters = groups.ToArray();
            var transformedParameters = new object[parameters.Length];
            for (var k = 0; k < parameters.Length; k++)
            {
                var paramInfo = m.GetParameters()[k];
                var paramValue = parameters[k];

                // Basic types
                switch (paramInfo.ParameterType.FullName)
                {
                    case "System.String":
                        {
                            transformedParameters[k] = (string)paramValue;
                            continue;
                        }
                    case "System.Boolean":
                        {
                            transformedParameters[k] = bool.Parse((string)paramValue);
                            continue;
                        }
                    case "System.Decimal":
                        {
                            transformedParameters[k] = decimal.Parse((string)paramValue);
                            continue;
                        }
                    case "System.Double":
                        {
                            transformedParameters[k] = double.Parse((string)paramValue);
                            continue;
                        }
                    case "System.Single":
                        {
                            transformedParameters[k] = float.Parse((string)paramValue);
                            continue;
                        }
                    case "System.Int32":
                        {
                            transformedParameters[k] = int.Parse((string)paramValue);
                            continue;
                        }
                    case "System.UInt32":
                        {
                            transformedParameters[k] = uint.Parse((string)paramValue);
                            continue;
                        }
                    case "System.Int64":
                        {
                            transformedParameters[k] = long.Parse((string)paramValue);
                            continue;
                        }
                    case "System.UInt64":
                        {
                            transformedParameters[k] = ulong.Parse((string)paramValue);
                            continue;
                        }
                    case "System.Int16":
                        {
                            transformedParameters[k] = short.Parse((string)paramValue);
                            continue;
                        }
                    case "System.UInt16":
                        {
                            transformedParameters[k] = ushort.Parse((string)paramValue);
                            continue;
                        }
                }

                // Look for a parameter converters
                var converter = paramConverters.FirstOrDefault(t =>
                {
                    var iis = t.GetInterfaces();
                    return iis.Any(i => i.GenericTypeArguments[0] == paramInfo.ParameterType);
                }
                );
                if (converter != null)
                {
                    var convObj = Activator.CreateInstance(converter);
                    var mi = converter.GetMethod("ConvertParameter");
                    transformedParameters[k] = mi.Invoke(convObj, new object[] { (string)paramValue });
                    continue;
                }

                // Assign unmatched as is
                transformedParameters[k] = paramValue;
            }
            m.Invoke(obj, transformedParameters);
            return true;
        }
    }
}
