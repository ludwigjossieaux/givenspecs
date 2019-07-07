using GivenSpecs.Enumerations;
using System;

namespace GivenSpecs.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class StepBaseAttribute : Attribute
    {
        internal StepTypeEnum[] Types { get; private set; }
        public string Regex { get; set; }

        internal StepBaseAttribute(string regex, StepTypeEnum type)
            : this(regex, new[] { type })
        {
        }

        protected StepBaseAttribute(string regex, StepTypeEnum[] types)
        {
            Regex = regex;
            Types = types;
        }
    }
}
