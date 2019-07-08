using GivenSpecs.Enumerations;

namespace GivenSpecs.Attributes
{
    public class ThenAttribute : StepBaseAttribute
    {
        public ThenAttribute() : this(null)
        {
        }

        public ThenAttribute(string regex) : base(regex, StepTypeEnum.Then)
        {
        }
    }
}
