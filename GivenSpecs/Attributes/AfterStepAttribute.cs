using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class AfterStepAttribute : HookAttribute
    {
        public AfterStepAttribute(params string[] tags) : base(HookTypeEnum.AfterStep, tags) { }
    }

}
