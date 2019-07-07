using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class WhenAttribute : StepBaseAttribute
    {
        public WhenAttribute() : this(null)
        {
        }

        public WhenAttribute(string regex) : base(regex, StepTypeEnum.When)
        {
        }
    }
}
