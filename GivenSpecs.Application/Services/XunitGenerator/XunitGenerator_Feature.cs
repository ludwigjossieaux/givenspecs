using GivenSpecs.Application.Reporting;
using System.Collections.Generic;

namespace GivenSpecs.Application.Services.XunitGenerator
{
    public class XunitGenerator_Feature
    {
        public string Namespace { get; set; }
        public string Class { get; set; }
        public List<XunitGenerator_Step> BackgroundSteps { get; set; }
        public List<XunitGenerator_Scenario> Scenarios { get; set; }
        public bool GenerateCollectionFixture { get; set; }

        public ReportedFeature Reported { get; set; }

        public XunitGenerator_Feature()
        {
            this.BackgroundSteps = new List<XunitGenerator_Step>();
            this.Scenarios = new List<XunitGenerator_Scenario>();
        }
    }
}
