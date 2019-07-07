using GivenSpecs.Enumerations;
using System;

namespace GivenSpecs.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class HookAttribute : Attribute
    {
        public HookTypeEnum Event { get; private set; }
        public string[] Tags { get; private set; }
        public int Order { get; set; }
        public const int DefaultOrder = 10000;

        internal HookAttribute(HookTypeEnum bindingEvent, string[] tags)
        {
            Event = bindingEvent;
            Tags = tags;
            Order = DefaultOrder;
        }
    }
}
