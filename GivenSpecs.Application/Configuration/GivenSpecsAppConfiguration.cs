using GivenSpecs.Application.Interfaces;

namespace GivenSpecs.Application.Configuration
{
    public class GivenSpecsAppConfiguration : IGivenSpecsAppConfiguration
    {
        public string FeatureNamespace { get; set; }
    }
}
