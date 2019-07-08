using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class AfterScenarioAttribute : HookAttribute
    {
        public AfterScenarioAttribute(params string[] tags) : base(HookTypeEnum.AfterScenario, tags) { }
    }

}
