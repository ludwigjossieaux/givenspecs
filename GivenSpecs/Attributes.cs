using System;
using System.Collections.Generic;
using System.Text;

namespace GivenSpecs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BindingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class StepBaseAttribute : Attribute
    {
        internal StepType[] Types { get; private set; }
        public string Regex { get; set; }

        internal StepBaseAttribute(string regex, StepType type)
            : this(regex, new[] { type })
        {
        }

        protected StepBaseAttribute(string regex, StepType[] types)
        {
            Regex = regex;
            Types = types;
        }
    }

    public class GivenAttribute : StepBaseAttribute
    {
        public GivenAttribute() : this(null)
        {
        }
        
        public GivenAttribute(string regex): base(regex, StepType.Given)
        {
        }
    }
    
    public class WhenAttribute : StepBaseAttribute
    {
        public WhenAttribute() : this(null)
        {
        }

        public WhenAttribute(string regex): base(regex, StepType.When)
        {
        }
    }
    
    public class ThenAttribute : StepBaseAttribute
    {
        public ThenAttribute() : this(null)
        {
        }

        public ThenAttribute(string regex): base(regex, StepType.Then)
        {
        }
    }
    
    public class AnyStepAttribute : StepBaseAttribute
    {
        public AnyStepAttribute()
            : this(null)
        {
        }

        public AnyStepAttribute(string regex)
            : base(regex, new[] { StepType.Given, StepType.When, StepType.Then })
        {
        }
    }
}
