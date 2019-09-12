using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class BeforeStepAttribute : HookAttribute
    {
        public BeforeStepAttribute(params string[] tags) : base(HookTypeEnum.BeforeStep, tags) { }
    }
}
