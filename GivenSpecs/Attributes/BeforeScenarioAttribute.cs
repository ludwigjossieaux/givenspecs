using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class BeforeScenarioAttribute : HookAttribute
    {
        public BeforeScenarioAttribute(params string[] tags) : base(HookTypeEnum.BeforeScenario, tags) { }
    }
}
