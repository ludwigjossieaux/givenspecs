using Gherkin.Ast;
using GivenSpecs.Application.Reporting;
using GivenSpecs.Application.Services.XunitGenerator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GivenSpecs.Application.Interfaces
{
    public interface IXunitGeneratorService
    {
        Task<(List<ReportedTag>, List<string>)> ProcessTags(IEnumerable<Tag> tags);
        Task<XunitGenerator_Step> ProcessStep(Step step);
        Task<List<XunitGenerator_Scenario>> ProcessScenario(Scenario scenario, string featureId, IXunitGeneratorService service);
        Task<XunitGenerator_Feature> ProcessFeature(Feature feature, string fileLocation, bool generateCollectionFixture, IXunitGeneratorService service);
        Task<string> Generate(GherkinDocument doc, string fileLocation, bool generateCollectionFixture, IXunitGeneratorService service);
    }
}
