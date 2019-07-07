using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class GivenAttribute : StepBaseAttribute
    {
        public GivenAttribute() : this(null)
        {
        }

        public GivenAttribute(string regex) : base(regex, StepTypeEnum.Given)
        {
        }
    }
}
